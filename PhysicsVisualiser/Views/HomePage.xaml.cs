using System;

namespace PhysicsVisualiser.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnFlatSurfaceClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FlatSurfacePage");
    }

    private async void OnInclinedSurfaceClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//InclinedSurfacePage");
    }
}
