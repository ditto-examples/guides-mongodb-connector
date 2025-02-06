using Guides.Services;
using Guides.ViewModels;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace Guides.Views;

public partial class MainPage : ContentPage
{
    private readonly ErrorService _errorService;
    private readonly MainPageViewModel _viewModel;

    public MainPage(ErrorService errorService, MainPageViewModel viewModel)
    {
        _viewModel = viewModel;
        _errorService = errorService;
        
        InitializeComponent();

        // Set the binding context
        BindingContext = viewModel;
        
        SetupErrorService();
    }

    private void SetupErrorService()
    {
        _errorService.OnError += message =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", message, "OK");
                _errorService.ErrorShown();
            });
        };   
    }

    private void AddButtonOnClicked(object? sender, EventArgs e)
    {
    }
}