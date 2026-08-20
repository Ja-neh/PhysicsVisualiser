using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhysicsEngine;
using PhysicsEngine.Bodies;
using PhysicsEngine.Scenarios;

namespace PhysicsVisualiser.ViewModels;

public partial class FlatSurfaceViewModel : ObservableObject
{
    private Director _director;

    // Input properties
    [ObservableProperty]
    public partial double Mass {  get; set; }
    [ObservableProperty]
    public partial double InitialVelocityX {  get; set; }
    [ObservableProperty]
    public partial double AppliedForce {  get; set; }
    [ObservableProperty]
    public partial double AppliedForceAngle {  get; set; }
    [ObservableProperty]
    public partial double FrictionCoefficient {  get; set; }
    [ObservableProperty]
    public partial double Gravity{ get; set; }


    // Runtime updating properies
    [ObservableProperty]
    public partial double CurrentTime { get; set; }
    [ObservableProperty]
    public partial double CurrentPositionX { get; set; }
    [ObservableProperty]
    public partial double CurrentVelocityX { get; set; }
    [ObservableProperty]
    public partial double CurrentAccelerationX { get; set; }
    [ObservableProperty]
    public partial double CurrentNormalForce { get; set; }
    [ObservableProperty]
    public partial double CurrentFrictionForce { get; set; }
    [ObservableProperty]
    public partial double CurrentWeightY { get; set; }
    [ObservableProperty]
    public partial double CurrentNetForceX { get; set; }

    // simulation control
    private IDispatcherTimer? _timer;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    private partial bool IsRunning { get; set; }

    public bool CanPlay => !IsRunning;
    public bool CanPause => IsRunning;
    public bool CanReset => true;

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        IsRunning = true;
        if (_timer is null)
        {
            _timer = Dispatcher.GetForCurrentThread()?.CreateTimer() ?? throw new InvalidOperationException("Dispatcher.GetForCurrent thread called from background thread");
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0); // 60 fps
            _timer.Tick += OnTimerTick;
        }
        _timer.Start();
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void Pause()
    {
        IsRunning = false;
        if(_timer is null)
        {
            throw new InvalidOperationException("Pause called while timer is null"); // the button shouldn't even be enabled -- something is wrong if this is thrown
        }
        _timer.Stop();
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        IsRunning = false;
        if( _timer is not null)
        {
            _timer.Stop();
        }

        CurrentTime = 0;
    }


    private void OnTimerTick(object? sender, EventArgs e)
    {
        if(_timer is null)
        {
            throw new InvalidOperationException("timer ticked while null");
        }

        CurrentTime += _timer.Interval.TotalSeconds;
    }


    public FlatSurfaceViewModel()
    {
        FlatSurface flatSurface = new FlatSurface();
        _director = new Director(flatSurface);
        IsRunning = false;

    }

    

}
