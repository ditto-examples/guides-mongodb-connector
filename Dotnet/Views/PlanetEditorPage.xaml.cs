using Guides.ViewModels;

namespace Guides.Views;

public partial class PlanetEditorPage : ContentPage
{
    public PlanetEditorPage(PlanetEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 