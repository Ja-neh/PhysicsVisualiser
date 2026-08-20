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
    private double _surfaceInclination = 0.0;
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

    
}
