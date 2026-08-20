using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PhysicsEngine;
using PhysicsEngine.Bodies;
using PhysicsEngine.Scenarios;

namespace PhysicsVisualiser.ViewModels;

public class FlatSurfaceViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private Director _director;

    // Physics default values
    private double _mass = 5.0;
    private double _initialVelocityX = 0.0;
    private double _appliedForce = 0.0;
    private double _appliedForceAngle = 0.0;
    private double _frictionCoefficient = 0.25;
    private double _gravity = Constants.earthGravitationalAcceleration;

    // Runtime values to keep track of
    private double _currentTime = 0.0;
    private double _currentPositionX = 0.0;
    private double _currentVelocityX = 0.0;
    private double _currentAccelerationX = 0.0;
    private double _currentNormalForce = 0.0;
    private double _currentFrictionForce = 0.0;
    private double _currentWeightY = 0.0;
    private double _currentNetForceX = 0.0;

    // simulation
    private bool CanPay;
    private bool CanPause;
    private IDispatcherTimer? _timer;
    public ICommand? PlayCommand;
    public ICommand? PauseCommand;
    public ICommand? ResetCommand;


    public FlatSurfaceViewModel()
    {
        FlatSurface flatSurface = new FlatSurface();
        _director = new Director(flatSurface);


    }

    #region INPUT PROPERTIES
    public double Mass
    {
        get => _mass;
        set
        {
            SetField(ref _mass, value);
        }
    }

    public double InitialVelocityX
    {
        get => _initialVelocityX;
        set
        {
            SetField(ref _initialVelocityX, value);
        }
    }

    public double AppliedForce
    {
        get => _appliedForce;
        set
        {
            SetField(ref _appliedForce, value);
        }
    }

    public double AppliedForceAngle
    {
        get => _appliedForceAngle;
        set
        {
            SetField(ref _appliedForceAngle, value);
        }
    }

    public double FrictionCoefficient
    {
        get => _frictionCoefficient;
        set
        {
            SetField(ref _frictionCoefficient, value);
        }
    }
    public double Gravity
    {
        get => _gravity;
        set
        {
            SetField(ref _gravity, value);
        }
    }
    #endregion


    #region RUNTIME UPDATING PROPERTIES
    public double CurrentTime
    {
        get => _currentTime;
        set
        {
            SetField(ref _currentTime, value);
        }
    }

    public double CurrentPositionX
    {
        get => _currentPositionX;
        set
        {
            SetField(ref _currentPositionX, value);
        }
    }

    public double CurrentVelocityX
    {
        get => _currentVelocityX;
        set
        {
            SetField(ref _currentVelocityX, value);
        }
    }

    public double CurrentAccelerationX
    {
        get => _currentAccelerationX;
        set
        {
            SetField(ref _currentAccelerationX, value);
        }
    }

    public double CurrentNormalForce
    {
        get => _currentNormalForce;
        set
        {
            SetField(ref _currentNormalForce, value);
        }
    }

    public double CurrentFrictionForce
    {
        get => _currentFrictionForce;
        set
        {
            SetField(ref _currentFrictionForce, value);
        }
    }

    public double CurrentWeightY
    {
        get => _currentWeightY;
        set
        {
            SetField(ref _currentWeightY, value);
        }
    }

    public double CurrentNetForceX
    {
        get => _currentNetForceX;
        set
        {
            SetField(ref _currentNetForceX, value);
        }
    }
    #endregion


    // Helpers
    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
