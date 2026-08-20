using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PhysicsEngine;
using PhysicsEngine.Bodies;
using PhysicsEngine.Scenarios;

namespace PhysicsVisualiser.ViewModels;

public partial class FlatSurfaceViewModel : ObservableObject
{
    private Director _director;

    // Physics default values
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


    // Runtime values to keep track of
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
