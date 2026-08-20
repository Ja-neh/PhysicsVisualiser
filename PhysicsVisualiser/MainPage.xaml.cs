using PhysicsVisualiser.ViewModels;

#if ANDROID
using Android.Content.PM;
#endif

namespace PhysicsVisualiser;

public partial class MainPage : ContentPage
{

#if ANDROID
    private ScreenOrientation? _originalOrientation;
#endif


    public MainPage()
    {
        InitializeComponent();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        LockOrientation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        UnlockOrientation();
    }

    private void LockOrientation()
    {
#if ANDROID
        if (Platform.CurrentActivity is not null)
        {
            _originalOrientation = Platform.CurrentActivity.RequestedOrientation;
            Platform.CurrentActivity.RequestedOrientation = ScreenOrientation.Landscape;
        }
#endif

    }

    private void UnlockOrientation()
    {
#if ANDROID
        if (Platform.CurrentActivity is not null && _originalOrientation.HasValue)
        {
            Platform.CurrentActivity.RequestedOrientation = _originalOrientation.Value;
        }
#endif

    }
}
