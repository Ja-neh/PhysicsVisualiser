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
using PhysicsEngine.Formulas;

namespace PhysicsVisualiser.ViewModels;

public partial class FlatSurfaceViewModel : ObservableObject
{
    private Director? _director;
    
    private const double _fixedTimeStep = 1.0 / 60.0; // 60 fps
    private double _accumulatedTime = 0.0;

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
    private partial bool IsRunning { get; set; }        // defaults to false - want false

    public bool CanPlay => !IsRunning;
    public bool CanPause => IsRunning;
    public bool CanReset => true;

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        if(CurrentTime == 0.0)
        {
            BuildScenario();
        }

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

        CurrentTime = 0.0;
        ResetUpdates();
        SyncViewAndSolver();
    }


    private void OnTimerTick(object? sender, EventArgs e)
    {
        if(_timer is null)
        {
            throw new InvalidOperationException("timer ticked while null");
        }

        _accumulatedTime += _timer.Interval.TotalSeconds;

        while(_accumulatedTime >= _fixedTimeStep)
        {
            _director!.Step(_fixedTimeStep);    // checked in Play() -- never set to null once initialised
            CurrentTime += _fixedTimeStep;
            _accumulatedTime -= _fixedTimeStep;
        }

        SyncViewAndSolver();
    }


    public FlatSurfaceViewModel()
    {

    }

    private void SyncViewAndSolver()
    {
        if (_director is null)
        {
            return;
        }

        FlatSurface scenario = (FlatSurface)_director.Scene;

        CurrentPositionX = scenario.box.PositionX;
        CurrentVelocityX = scenario.box.VelocityX;
        CurrentAccelerationX = scenario.box.AccelerationX;
        CurrentNormalForce = scenario.box.Normal.Magnitude;
        CurrentFrictionForce = Forces.Friction(FrictionCoefficient, CurrentNormalForce);
        CurrentWeightY = scenario.box.WeightY.Magnitude;
        CurrentNetForceX = Forces.FNet(Mass, CurrentAccelerationX);
    }

    private void BuildScenario()
    {
        FlatSurface scenario = new FlatSurface
        {
            Mass = this.Mass,
            InitialVelocityX = this.InitialVelocityX,
            AppliedForce = this.AppliedForce,
            AppliedForceAngle = this.AppliedForceAngle,
            FrictionCoefficient = this.FrictionCoefficient,
            Gravity = this.Gravity,
        };

        if(_director is null)
        {
            _director = new Director(scenario);
        }
        else
        {
            _director.SetScenario(scenario);
        }
    }

    private void ResetUpdates()
    {
        CurrentTime = 0.0;
        CurrentPositionX = 0.0;
        CurrentVelocityX = 0.0;
        CurrentAccelerationX = 0.0;
        CurrentNormalForce = 0.0;
        CurrentFrictionForce = 0.0;
        CurrentWeightY = 0.0;
        CurrentNetForceX = 0.0;

        if(_director is not null)
        {
            FlatSurface scenario = (FlatSurface)_director.Scene;

            scenario.ResetAccumulatedTime();
            scenario.box.PositionX = CurrentPositionX;
            scenario.box.VelocityX = CurrentVelocityX;
            scenario.box.AccelerationX = CurrentAccelerationX;
            scenario.box.Normal.Magnitude = CurrentNormalForce;
            scenario.box.WeightY.Magnitude = CurrentWeightY;
        }
    }
}
