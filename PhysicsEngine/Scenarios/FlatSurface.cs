using System;
using PhysicsEngine.Bodies;
using PhysicsEngine.Formulas;
using PhysicsEngine.Quantities;

namespace PhysicsEngine.Scenarios;

public record FlatSurfaceState(
    double Time,
    double Mass,
    double Position,
    double Velocity,
    double Acceleration,
    double Normal,
    double Weight,
    double FrictionCoefficient,
    double StaticFriction,
    double Friction,
    double AppliedForceX,
    double AppliedForceY,
    double FNetX,
    double FNetY

) : ScenarioState();


public class FlatSurface : Scenario
{
    private readonly Box box = new Box();

    #region PUBLIC PROPERTIES
    public double CurrentTime { get; private set; }

    public double Mass
    {
        get => box.Mass;
        set
        {
            box.Mass = value;
        }
    }

    public double InitialVelocity
    {
        get => box.InitialVelocityX;
        set
        {
            box.InitialVelocityX = value;
        }
    }

    public double AppliedForce { get; set; }
    public double AppliedForceAngle { get; set; }

    public double FrictionCoefficient { get; set; }

    public double Gravity { get; set; } = Constants.earthGravitationalAcceleration;
    #endregion


    #region PRIVATE PROPERTIES
    // from box properties
    private double Position
    {
        get => box.PositionX;
        set
        {
            box.PositionX = value;
        }
    }

    private double Velocity
    {
        get => box.VelocityX;
        set
        {
            box.VelocityX = value;
        }        
    }

    private double Acceleration
    {
        get => box.AccelerationX;
        set
        {
            box.AccelerationX = value;
        }
    }

    private double Weight
    {
        get => box.WeightX.SignedMagnitude;
        set
        {
            box.WeightX.Magnitude = value;
        }
    }

    private double Normal
    {
        get => box.Normal.SignedMagnitude;
        set
        {
            box.Normal.Magnitude = value;
        }
    }
    #endregion


    #region FIELDS
    private readonly Force _appliedForceX = new Force(0, DirectionXY.Xpositive);
    private readonly Force _appliedForceY = new Force(0, DirectionXY.Ypositive);

    private readonly Force _maxStaticFriction = new Force(0, DirectionXY.Xpositive);
    private readonly Force _staticFriction = new Force(0, DirectionXY.Xpositive);
    private readonly Force _kineticFriction = new Force(0, DirectionXY.Xpositive);

    private readonly Force _fNetX = new Force(0, DirectionXY.Xpositive);
    private readonly Force _fNetY = new Force(0, DirectionXY.Xpositive);

    private FlatSurfaceState? _currentState;

    private const double _surfaceInclination = 0.0;
    #endregion


    public FlatSurface() {}


    protected override void Initialize()   // To components 
    {
        _appliedForceX.Magnitude = Forces.ForceAdjacent(AppliedForce, Math.Abs(AppliedForceAngle));
        if(AppliedForce < 0)
        {
            _appliedForceX.Direction = DirectionXY.Xnegative;
        }

        _appliedForceY.Magnitude = Forces.ForceOpposite(AppliedForce, Math.Abs(AppliedForceAngle));
        if(AppliedForceAngle < 0)
        {
            _appliedForceY.Direction = DirectionXY.Ynegative;
        }
    }

    public void Restart()
    {
        CurrentTime = default;

        Position = 0.0;
        Velocity = InitialVelocity;
        Acceleration = 0.0;
        _fNetX.Magnitude = 0.0;
        _fNetX.Direction = DirectionXY.Xpositive;
        _fNetY.Magnitude = 0.0;
        _fNetY.Direction = DirectionXY.Ypositive;
    }

    public void Reset()
    {
        CurrentTime = default;
        Mass = 0.0;
        InitialVelocity = 0.0;
        Position = 0.0;
        Velocity = 0.0;
        Acceleration = 0.0;
        Weight = 0.0;
        Normal = 0.0;
    }


    public override void Update(double delta)   // using TotalTime instead of small deltas in calculations
    {                                               // to avoid double inaccuracy compounding over time
        if (CurrentTime == default) Initialize();

        CurrentTime += delta;

        // weight & normal
        Weight = Forces.WeightPerpendicular(Mass, _surfaceInclination);
        Normal = Weight + _appliedForceY.SignedMagnitude;

        // fnetY
        double tempMagnitude = _appliedForceY.SignedMagnitude + Weight + Normal;
        if (tempMagnitude < 0)
        {
            _fNetY.Direction = DirectionXY.Ynegative;
        }
        _fNetY.Magnitude = Math.Abs(tempMagnitude);

        // friction magnitude
        _staticFriction.Magnitude = Forces.Friction(FrictionCoefficient, Normal);
        _kineticFriction.Magnitude = Forces.Friction(FrictionCoefficient, Normal);
        _maxStaticFriction.Magnitude = Forces.Friction(FrictionCoefficient, Normal);


        // ja... neh
        bool updateAPV = true;
        int velocitySign = Math.Sign(Velocity);
        if(velocitySign != 0)                 // movement
        {
            _staticFriction.Magnitude = 0.0;
            _staticFriction.Direction = DirectionXY.Xpositive;

            if(velocitySign < 0)
            {
                _kineticFriction.Direction = DirectionXY.Xpositive;
            }
            else
            {
                _kineticFriction.Direction = DirectionXY.Xnegative;
            }

            _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _kineticFriction.SignedMagnitude;

            if (_appliedForceX.Direction == _kineticFriction.Direction)     // appliedForce and friction in same direction
            {
                _fNetX.Direction = _appliedForceX.Direction;
            }
            else if (_appliedForceX.Magnitude > _kineticFriction.Magnitude)     // opp direction - Fa > fk
            {
                _fNetX.Direction = _appliedForceX.Direction;
            }
            else if (_appliedForceX.Magnitude < _kineticFriction.Magnitude)     // opp direction - Fa < fk
            {
                _fNetX.Direction = _kineticFriction.Direction;
            }
        }
        else if(velocitySign == 0)          // no movement
        {

            if(_maxStaticFriction.Magnitude >= _appliedForceX.Magnitude)       // fmax >= Fa
            {
                _kineticFriction.Magnitude = 0.0;
                _kineticFriction.Direction = DirectionXY.Xpositive;

                _staticFriction.Magnitude = _appliedForceX.Magnitude;
                _staticFriction.Direction = _appliedForceX.Direction.Negate();

                _fNetX.Magnitude = 0.0;
                _fNetX.Direction = DirectionXY.Xpositive;
                updateAPV = false;
            }
            
            if(_maxStaticFriction.Magnitude < _appliedForceX.Magnitude)      // Fa > fmax
            {
                _kineticFriction.Direction = _appliedForceX.Direction.Negate();

                _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _kineticFriction.SignedMagnitude;
                _fNetX.Direction = _appliedForceX.Direction;
            }
        }

        // x, v, a
        double previousVelocity = Velocity;

        if (updateAPV)
        {
            Acceleration = _fNetX.SignedMagnitude / Mass;
            Position = Motion.DisplacementUsingAcceleration(InitialVelocity, CurrentTime, Acceleration);
            Velocity = Motion.FinalVelocity(InitialVelocity, Acceleration, CurrentTime);
        }    

        if (previousVelocity * Velocity < 0.0)
        {
            Velocity = 0.0;
            Acceleration = 0.0;

            _fNetX.Magnitude = 0.0;
            _kineticFriction.Magnitude = 0.0;
            _staticFriction.Magnitude = 0.0;

            _fNetX.Direction = DirectionXY.Xpositive;
            _kineticFriction.Direction = DirectionXY.Xpositive;
            _staticFriction.Direction = DirectionXY.Xpositive;

            if (_appliedForceX.Magnitude != 0.0)
            {
                _staticFriction.Magnitude = _appliedForceX.Magnitude;
                _staticFriction.Direction = _appliedForceX.Direction.Negate();               
            }
        }

    }


    public FlatSurfaceState GetCurrentState()
    {
        _currentState = new FlatSurfaceState(CurrentTime, 
                                            Mass, Position, Velocity, Acceleration,
                                            Normal, Weight, FrictionCoefficient,
                                            _staticFriction.SignedMagnitude ,_kineticFriction.SignedMagnitude,
                                            _appliedForceX.SignedMagnitude, _appliedForceY.SignedMagnitude,
                                            _fNetX.SignedMagnitude, _fNetY.SignedMagnitude);

        return _currentState;
    }
}
