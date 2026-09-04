using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Dispatching;
using PhysicsSolver.Formulas;
using PhysicsSolver.Scenarios;
using PhysicsVisualiser.Renderers;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PhysicsVisualiser.ViewModels;

public partial class FlatSurfaceViewModel : ObservableObject
{
    // Scenario
    private FlatSurface _flatScenario = new FlatSurface();
    public FlatSurfaceState State { get; private set; }

    // Time
    private const double _fixedTimeStep = 1.0 / 60.0; // 60 fps
    private double _accumulatedTime = 0.0;

    // Renderer
    public FlatSurfaceRenderer Renderer { get; } = new FlatSurfaceRenderer();
    public event Action? RequestInvalidateSurface;

    // Input string properties for UI data-binding (prevents mid-keystroke reformatting / cursor jumps)
    [ObservableProperty]
    public partial string UserMassInput { get; set; } = "5.0";
    [ObservableProperty]
    public partial string UserInitialVelocityInput { get; set; } = "0.0";
    [ObservableProperty]
    public partial string UserAppliedForceInput { get; set; } = "0.0";
    [ObservableProperty]
    public partial string UserAppliedForceAngleInput { get; set; } = "0.0";
    [ObservableProperty]
    public partial string UserStaticFrictionCoefficientInput { get; set; } = "0.0";
    [ObservableProperty]
    public partial string UserKineticFrictionCoefficientInput { get; set; } = "0.0";
    [ObservableProperty]
    public partial string UserGravityInput { get; set; } = "9.8";

    // Underlying double values
    [ObservableProperty]
    public partial double UserMass { get; set; } = 5.0;
    [ObservableProperty]
    public partial double UserInitialVelocity { get; set; }
    [ObservableProperty]
    public partial double UserAppliedForce { get; set; }
    [ObservableProperty]
    public partial double UserAppliedForceAngle { get; set; }
    [ObservableProperty]
    public partial double UserStaticFrictionCoefficient { get; set; }
    [ObservableProperty]
    public partial double UserKineticFrictionCoefficient { get; set; }
    [ObservableProperty]
    public partial double UserGravity { get; set; } = 9.8;

    // UI time
    [ObservableProperty]
    public partial double SolverCurrentTime { get; set; }

    // Runtime updating properies
    [ObservableProperty]
    public partial double SolverCurrentPosition { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentVelocity { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentAcceleration { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentNormalForce { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentMaxStaticFrictionForce { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentStaticFrictionForce { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentKineticFrictionForce { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentWeight { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentAppliedForceX { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentAppliedForceY { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentNetForceX { get; set; }
    [ObservableProperty]
    public partial double SolverCurrentNetForceY { get; set; }
    [ObservableProperty]
    public partial bool SolverLiftOffWarning { get; set; }

    // Show toggles
    [ObservableProperty]
    public partial bool ShowForceVectors { get; set; }
    [ObservableProperty]
    public partial bool ShowVelocityVectors { get; set; }


    #region TOGGLE TO RENDERER
    partial void OnShowForceVectorsChanged(bool value)
    {
        Renderer.ShowForceVectors = value;
        RequestRepaint();
    }

    partial void OnShowVelocityVectorsChanged(bool value)
    {
        Renderer.ShowVelocityVectors = value;
        RequestRepaint();
    }
    #endregion


    #region INPUT TO SOLVER
    private static bool TryParseDouble(string? text, out double result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0;
            return false;
        }

        string normalized = text.Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    partial void OnUserMassInputChanged(string value)
    {
        if (TryParseDouble(value, out double val) && val > 0)
        {
            UserMass = val;
            _flatScenario.Mass = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserInitialVelocityInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserInitialVelocity = val;
            _flatScenario.InitialVelocity = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserAppliedForceInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserAppliedForce = val;
            _flatScenario.AppliedForce = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserAppliedForceAngleInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserAppliedForceAngle = val;
            _flatScenario.AppliedForceAngle = Conversions.DegreesToRadians(val);
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserStaticFrictionCoefficientInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserStaticFrictionCoefficient = val;
            _flatScenario.StaticFrictionCoefficient = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserKineticFrictionCoefficientInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserKineticFrictionCoefficient = val;
            _flatScenario.KineticFrictionCoefficient = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserGravityInputChanged(string value)
    {
        if (TryParseDouble(value, out double val))
        {
            UserGravity = val;
            _flatScenario.Gravity = val;
            if (!IsRunning)
            {
                SyncViewWithSolver();
                RequestRepaint();
            }
        }
    }

    partial void OnUserMassChanged(double value)
    {
        _flatScenario.Mass = value;
    }

    partial void OnUserInitialVelocityChanged(double value)
    {
        _flatScenario.InitialVelocity = value;
    }

    partial void OnUserAppliedForceChanged(double value)
    {
        _flatScenario.AppliedForce = value;
    }

    partial void OnUserAppliedForceAngleChanged(double value)
    {
        _flatScenario.AppliedForceAngle = Conversions.DegreesToRadians(value);
    }

    partial void OnUserStaticFrictionCoefficientChanged(double value)
    {
        _flatScenario.StaticFrictionCoefficient = value;
    }

    partial void OnUserKineticFrictionCoefficientChanged(double value)
    {
        _flatScenario.KineticFrictionCoefficient = value;
    }

    partial void OnUserGravityChanged(double value)
    {
        _flatScenario.Gravity = value;
    }
    #endregion


    #region SIMULATION CONTROL
    private IDispatcherTimer? _timer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    public partial bool IsRunning { get; set; }        // defaults to false - want false

    public bool CanPlay => !IsRunning;
    public bool CanPause => IsRunning;
    public bool CanRestart => true;

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        IsRunning = true;
        if (_timer is null)
        {
            _timer = Dispatcher.GetForCurrentThread()?.CreateTimer() ?? throw new InvalidOperationException("Timer couldn't be created");
            _timer.Interval = TimeSpan.FromSeconds(_fixedTimeStep); // 60 fps
            _timer.Tick += OnTimerTick;
        }
        _timer.Start();
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void Pause()
    {
        IsRunning = false;
        if (_timer is null)
        {
            throw new InvalidOperationException("Pause called while timer is null"); // the button shouldn't even be enabled -- something is wrong if this is thrown
        }
        _timer.Stop();
    }

    [RelayCommand(CanExecute = nameof(CanRestart))]
    private void Restart()
    {
        IsRunning = false;
        if (_timer is not null)
        {
            _timer.Stop();
        }

        _flatScenario.Restart();
        Renderer.ResetCamera();

        SyncViewWithSolver();
        RequestRepaint();
    }
    #endregion

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_timer is null)
        {
            throw new InvalidOperationException("timer ticked while null");
        }

        _accumulatedTime += _timer.Interval.TotalSeconds;

        while (_accumulatedTime >= _fixedTimeStep)
        {
            _flatScenario.Update(_fixedTimeStep);
            _accumulatedTime -= _fixedTimeStep;
        }

        SyncViewWithSolver();
        RequestRepaint();
    }


    public FlatSurfaceViewModel()
    {
        _flatScenario.Gravity = UserGravity;
        State = _flatScenario.GetCurrentState();
        ShowForceVectors = Renderer.ShowForceVectors;
        ShowVelocityVectors = Renderer.ShowVelocityVectors;
    }

    private void SyncViewWithSolver()
    {
        State = _flatScenario.GetCurrentState();

        SolverCurrentTime = State.Time;
        SolverCurrentPosition = State.Position;
        SolverCurrentVelocity = State.Velocity;
        SolverCurrentNormalForce = State.Normal;
        SolverCurrentMaxStaticFrictionForce = State.MaxStaticFriction;
        SolverCurrentStaticFrictionForce = State.StaticFriction;
        SolverCurrentKineticFrictionForce = State.KineticFriction;
        SolverCurrentWeight = State.Weight;
        SolverCurrentAcceleration = State.Acceleration;
        SolverCurrentAppliedForceX = State.AppliedForceX;
        SolverCurrentAppliedForceY = State.AppliedForceY;
        SolverCurrentNetForceX = State.FNetX;
        SolverCurrentNetForceY = State.FNetY;
        SolverLiftOffWarning = State.LiftOffWarning;
    }

    public void RequestRepaint()
    {
        RequestInvalidateSurface?.Invoke();
    }
}

