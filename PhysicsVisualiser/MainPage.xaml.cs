using PhysicsVisualiser.ViewModels;
using SkiaSharp.Views.Maui;
using SkiaSharp;

#if ANDROID
using Android.Content.PM;
#endif

namespace PhysicsVisualiser;

public partial class MainPage : ContentPage
{

    private FlatSurfaceViewModel? ViewModel => BindingContext as FlatSurfaceViewModel;

    #region ANDROID ROTATE VARIABLES
#if ANDROID
    private ScreenOrientation? _originalOrientation;
#endif
    #endregion


    public MainPage()
    {
        InitializeComponent();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (ViewModel != null)
        {
            ViewModel.RequestInvalidateSurface += OnRequestInvalidateSurface;
        }

        LockOrientation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (ViewModel != null)
        {
            ViewModel.RequestInvalidateSurface -= OnRequestInvalidateSurface;
        }

        UnlockOrientation();
    }

    #region ANDROID ROTATE FUNCTIONS
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
    #endregion


    private void OnRequestInvalidateSurface()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SkiaCanvasView?.InvalidateSurface();
        });
    }

    private void OnSkiaCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (ViewModel != null)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            float scaleX = info.Width / (float)SkiaCanvasView.Width;
            float scaleY = info.Height / (float)SkiaCanvasView.Height;

            canvas.Save();
            canvas.Scale(scaleX, scaleY);

            ViewModel.Renderer.Render(canvas, new SKImageInfo((int)SkiaCanvasView.Width, (int)SkiaCanvasView.Height), ViewModel);
        }
    }
}
