using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Guides.Models;
using Guides.Services;

namespace Guides.ViewModels;

public partial class PlanetEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ErrorService _errorService;
    private Planet? _existingPlanet;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int orderFromSun = 1;

    [ObservableProperty]
    private bool hasRings;

    [ObservableProperty]
    private string atmosphere = string.Empty;

    [ObservableProperty]
    private string maxTemp = string.Empty;

    [ObservableProperty]
    private string meanTemp = "0.0";

    [ObservableProperty]
    private string minTemp = string.Empty;

    public PlanetEditorViewModel(
        IDataService dataService,
        ErrorService errorService,
        Planet? planet = null)
    {
        _dataService = dataService;
        _errorService = errorService;
        _existingPlanet = planet;
        
        Title = planet == null ? "Add Planet" : "Edit Planet";
        
        if (planet != null)
        {
            InitializeWithPlanet(planet);
        }
    }

    private void InitializeWithPlanet(Planet planet)
    {
        Name = planet.Name;
        OrderFromSun = planet.OrderFromSun;
        HasRings = planet.HasRings;
        Atmosphere = string.Join(", ", planet.MainAtmosphere);
        MaxTemp = planet.SurfaceTemperatureC.Max?.ToString() ?? string.Empty;
        MeanTemp = planet.SurfaceTemperatureC.Mean.ToString(CultureInfo.InvariantCulture);
        MinTemp = planet.SurfaceTemperatureC.Min?.ToString() ?? string.Empty;
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            var atmosphereList = Atmosphere
                .Split(',')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList();

            double? max = !string.IsNullOrEmpty(MaxTemp) && double.TryParse(MaxTemp, out var m) ? m : null;
            double mean = double.Parse(MeanTemp);
            double? min = !string.IsNullOrEmpty(MinTemp) && double.TryParse(MinTemp, out var n) ? n : null;

            if (_existingPlanet != null)
            {
                var updatedPlanet = new Planet(
                    _existingPlanet.Id,
                    HasRings,
                    false,
                    atmosphereList,
                    Name,
                    OrderFromSun,
                    _existingPlanet.PlanetId,
                    new Temperature(max, mean, min)
                );
                await _dataService.UpdatePlanetAsync(updatedPlanet);
            }
            else
            {
                var id = Guid.NewGuid().ToString();
                var newPlanet = new Planet(
                    id,
                    HasRings,
                    false,
                    atmosphereList,
                    Name,
                    OrderFromSun,
                    id,
                    new Temperature(max, mean, min)
                );
                await _dataService.AddPlanetAsync(newPlanet);
            }

            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to save planet: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }
} 