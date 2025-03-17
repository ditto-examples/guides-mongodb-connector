using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Guides.Models;
using Guides.Services;
using Guides.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Guides.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ErrorService _errorService;

    [ObservableProperty] 
    // ReSharper disable once InconsistentNaming
    private ObservableCollection<Planet> planets;

    [ObservableProperty]
    // ReSharper disable once InconsistentNaming
    private int planetCount;

    public MainPageViewModel(
        IDataService dataService, 
        ErrorService errorService)
    {
        planets = new ObservableCollection<Planet>();
        _errorService = errorService;
        _dataService = dataService;
        GetPlanets();
    }

    [RelayCommand]
    private async Task DeletePlanet(Planet? planet)
    {
        try
        {
            if (planet == null) return;

            var answer = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Delete Planet",
                $"Are you sure you want to delete {planet.Name}?",
                "Yes",
                "No");
            if (answer)
            {
                await _dataService.ArchivePlanetAsync(planet.PlanetId);
                Planets.Remove(planet);
                PlanetCount = Planets.Count;
            }
        }
        catch (Exception ex)
        {
            _errorService.ShowError($"Failed to delete planet: {ex.Message}");
        }
    }

    private void GetPlanets()
    {
        _dataService.GetPlanets(resultPlanets =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Planets = new ObservableCollection<Planet>();
                foreach (var planet in resultPlanets)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding planet: {planet.Name}, Atmosphere: {string.Join(",", planet.MainAtmosphere)}");
                    Planets.Add(planet);
                }
                PlanetCount = Planets.Count;
                System.Diagnostics.Debug.WriteLine($"Total planets: {PlanetCount}");
            });
        });
    }

    [RelayCommand]
    private async Task AddPlanet()
    {
        await Shell.Current.Navigation.PushAsync(
            new PlanetEditorPage(new PlanetEditorViewModel(_dataService, _errorService)));
    }

    [RelayCommand]
    private async Task EditPlanet(Planet planet)
    {
        await Shell.Current.Navigation.PushAsync(
            new PlanetEditorPage(new PlanetEditorViewModel(_dataService, _errorService, planet)));
    }
}