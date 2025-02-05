using Guides.Models;
using System.Threading;
using DittoSDK;

namespace Guides.Services;

public interface IDataService
{
    Task GetPlanetsAsync(Action<IReadOnlyList<Planet>> callback);
    Task UpdatePlanetAsync(Planet planet);
    Task AddPlanetAsync(Planet planet);
    
    Task ArchivePlanetAsync(string planetId);
}

public class DittoService : IDataService
{
    private readonly AppConfig _appConfig;
    private Ditto? _ditto = null;
    private DittoSyncSubscription _subscription = null;
    private DittoStoreObserver _planetObserver = null;
    
    public DittoService(AppConfig appConfig)
    {
        _appConfig = appConfig;
        
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
        var identity = DittoIdentity 
            .OnlinePlayground( 
                appConfig.AppId, 
                appConfig.AuthToken, 
                false);

        this._ditto = new Ditto(identity);
        // TODO add in transport configuration
        // this._ditto?.TransportConfig.Connect.WebsocketUrls.Add(webSocketUrl);
        SetupSubscriptions();
    }

    private void SetupSubscriptions()
    {
        
    }

    public Task GetPlanetsAsync(Action<IReadOnlyList<Planet>> callback)
    {
        try {
            callback(new List<Planet>());
        } catch (Exception ex) {
            Console.WriteLine($"Error initializing Ditto: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    public Task UpdatePlanetAsync(Planet planet)
    {
        throw new NotImplementedException();
    }

    public Task AddPlanetAsync(Planet planet)
    {
        throw new NotImplementedException();
    }

    public Task ArchivePlanetAsync(string planetId)
    {
        throw new NotImplementedException();
    }
}