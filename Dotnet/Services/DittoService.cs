using Guides.Models;
using DittoSDK;

namespace Guides.Services;

public interface IDataService
{
    void GetPlanets(Action<Planet[]> callback);
    Task UpdatePlanetAsync(Planet planet);
    Task AddPlanetAsync(Planet planet);
    Task ArchivePlanetAsync(string planetId);
}

public class DittoService : IDataService
{
    private readonly AppConfig _appConfig;
    private readonly ErrorService _errorService;
    private readonly Ditto? _ditto;
    private DittoSyncSubscription? _subscription = null;
    private DittoStoreObserver? _planetObserver = null;
    
    public DittoService(AppConfig appConfig, ErrorService errorService)
    {
        _appConfig = appConfig;
        _errorService = errorService;
        
        try 
        {
            //setup Ditto logging
            DittoLogger.SetMinimumLogLevel(DittoLogLevel.Debug);
            
            //
            // CustomUrl is used because Connector is in Private Preview
            // and uses a different cluster than normal production
            //
            var customAuthUrl = $"https://{appConfig.EndpointUrl}";
            var webSocketUrl = $"wss://{appConfig.EndpointUrl}";
            
            //
            // TODO remove when Connector is out of Private Preview
            // https://docs.ditto.live/sdk/latest/install-guides/c-sharp#initializing-ditto
            
            //
            //TODO once bug is fix switch enableDittoCloud to false and set customAuthUrl
            //
            var identity = DittoIdentity 
                .OnlinePlayground( 
                    appConfig.AppId, 
                    appConfig.AuthToken, 
                    true);

            this._ditto = new Ditto(identity);
            _ditto.DisableSyncWithV3(); // remove this once using customAuthUrl
            
            // TODO add in transport configuration
            // this._ditto?.TransportConfig.Connect.WebsocketUrls.Add(webSocketUrl);
            
            SetupSubscriptions();
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to initialize Ditto: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets up the initial subscription to the planets collection in Ditto.
    /// </summary>
    /// <remarks>
    /// This subscription ensures that changes to the planets collection are synced 
    /// between the local Ditto store and the MongoDB Atlas database.
    /// 
    /// The subscription:
    /// <list type="bullet">
    /// <item><description>Queries all non-archived planets (isArchived = false)</description></item>
    /// <item><description>Starts the sync process with the Ditto cloud</description></item>
    /// <item><description>Maintains the subscription until the app is terminated or sync is stopped</description></item>
    /// </list>
    /// 
    /// </remarks>
    /// <seealso href="https://docs.ditto.live/sdk/latest/sync/syncing-data#creating-subscriptions">Ditto Sync Documentation</seealso>
    private void SetupSubscriptions()
    {
        try
        {
            if (_ditto == null || _subscription != null) return;
            //create a subscription to sync the planets collection 
            _subscription = _ditto.Sync.RegisterSubscription(
                "SELECT * FROM planets WHERE isArchived = :isArchived", 
                new Dictionary<string, object> { { "isArchived", false } }
            );
            _ditto.StartSync();
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to Setup Subscription: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers an observer for the planets collection and provides real-time updates through a callback.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Sets up a live query observer for the planets collection</description></item>
    /// <item><description>Returns only non-archived planets (isArchived = false)</description></item>
    /// <item><description>Orders results by distance from the sun</description></item>
    /// <item><description>Automatically updates when changes occur</description></item>
    /// </list>
    /// </remarks>
    /// <param name="callback">Action that receives updates when the planets collection changes</param>
    /// <seealso href="https://docs.ditto.live/sdk/latest/crud/observing-data-changes#store-observer-with-query-arguments">Ditto Observer Documentation</seealso>
    public void GetPlanets(Action<Planet[]> callback)
    {
        try {
            if (_ditto != null && _subscription != null)
            {
                var queryArguments = new Dictionary<string, object>(){{"isArchived", false }};

                // Register an observer to listen for changes to any documents in the planets collection
                _planetObserver = _ditto.Store.RegisterObserver(
                    "SELECT * FROM planets WHERE isArchived = :isArchived ORDER BY orderFromSun", 
                    queryArguments,
                    result =>
                    {
                        var planets = new List<Planet>();
                        result.Items.ForEach(item =>
                        {
                            planets.Add(Planet.FromDictionary(item.Value));
                        });    
                       callback(planets.ToArray()); 
                    } );
            }
        } catch (Exception ex) {
            _errorService.ShowError($"Error with GetPlanetAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing planet's properties in the Ditto store.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Updates all mutable fields of the planet</description></item>
    /// <item><description>Maintains the planet's ID and references</description></item>
    /// <item><description>Triggers a sync with other devices</description></item>
    /// </list>
    /// </remarks>
    /// <param name="planet">The Planet object containing the updated values</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown when the update operation fails</exception>
    /// <seealso href="https://docs.ditto.live/sdk/latest/crud/update#updating">Ditto Update Documentation</seealso>
    public async Task UpdatePlanetAsync(Planet planet)
    {
        try
        {
            if (_ditto != null)
            {
                var updateArguments = planet.ToDictionary(); 
                var results = await _ditto.Store.ExecuteAsync(
                    "UPDATE planets SET hasRings = :hasRings, isArchived = :isArchived, mainAtmosphere = :atmosphere, name = :name, orderFromSun = :orderFromSun, surfaceTemperatureC = :temperature  WHERE planetId = :planetId", 
                    updateArguments);
                if (results.Items.Count == 0)
                {
                    _errorService.ShowError("Error: Planet Update didn't return any results.");
                }
            }
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to update planet: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates a new planet document in the Ditto store.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Creates a new document in the planets collection</description></item>
    /// <item><description>Assigns the provided ID and properties</description></item>
    /// <item><description>Sets initial isArchived status to false</description></item>
    /// <item><description>Triggers a sync with other devices</description></item>
    /// </list>
    /// </remarks>
    /// <param name="planet">The new Planet object to be added</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown when the insert operation fails</exception>
    /// <seealso href="https://docs.ditto.live/sdk/latest/crud/create#creating-documents">Ditto Create Documentation</seealso>
    public async Task AddPlanetAsync(Planet planet)
    {
        try
        {
            if (_ditto != null)
            {
                var arguments = planet.ToDictionary(); 
                var insertArguments = new Dictionary<string, object>{{"newPlanet", arguments}};
                var results = await _ditto.Store.ExecuteAsync(
                    "INSERT INTO planets DOCUMENTS (:newPlanet)", 
                    insertArguments);
                if (results.Items.Count == 0)
                {
                    _errorService.ShowError("Error: Adding Planet didn't return any results.");
                }
            }
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to update planet: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Archives a planet by setting its isArchived flag to true.
    /// </summary>
    /// <remarks>
    /// This method implements the 'Soft-Delete' pattern:
    /// <list type="bullet">
    /// <item><description>Marks the planet as archived instead of deleting it</description></item>
    /// <item><description>Removes it from active queries and views</description></item>
    /// <item><description>Maintains the data for historical purposes</description></item>
    /// <item><description>Triggers a sync with other devices</description></item>
    /// </list>
    /// </remarks>
    /// <param name="planetId">The unique identifier of the planet to archive</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown when the archive operation fails</exception>
    /// <seealso href="https://docs.ditto.live/sdk/latest/crud/delete#soft-delete-pattern">Ditto Soft Delete Documentation</seealso>
    public async Task ArchivePlanetAsync(string planetId)
    {
        try
        {
            if (_ditto != null)
            {
                var updateArguments = new Dictionary<string, object>
                {
                    { "isArchived", true },
                    { "planetId", planetId }
                };
                var results = await _ditto.Store.ExecuteAsync(
                    " UPDATE planets SET isArchived = :isArchived WHERE planetId = :planetId", 
                    updateArguments);
                if (results.Items.Count == 0)
                {
                    _errorService.ShowError("Error: Planet Update didn't return any results.");
                }
            }
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to update planet: {ex.Message}");
            throw;
        }
    }
}