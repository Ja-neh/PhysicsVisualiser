using System;
using PhysicsSolver.Bodies;
using PhysicsSolver.Formulas;
using PhysicsSolver.Quantities;

namespace PhysicsSolver.Scenarios;

public record FlatSurfaceState(
    double Time,
    double Mass,
    double Position,
    double Velocity,
    double Acceleration,
    double Normal,
    double Weight,
    double StaticFrictionCoefficient,
    double KineticFrictionCoefficient,
    double MaxStaticFriction,
    double StaticFriction,
    double KineticFriction,
    double AppliedForceX,
    double AppliedForceY,
    double FNetX,
    double FNetY,
    bool LiftOffWarning

) : ScenarioState();

public record FlatSurfaceSegment(
    double ElapsedTime,
    double StartPosition,
    double InitialVelocity,
    double FinalVelocity,
    double Acceleration
);

public class FlatSurface : Scenario
{
    private readonly Box box = new Box();


    #region PUBLIC PROPERTIES
    public List<FlatSurfaceSegment> Segments { get; private set; } = new();

    public double Mass
    {
        private get => box.Mass;
        set
        {
            if (value > 0)
            {
                box.Mass = value;
            }
        }
    }

    public double InitialVelocity
    {
        private get => box.InitialVelocityX;
        set
        {
            box.InitialVelocityX = value;
        }
    }


    private double _appliedForce;
    public double AppliedForce
    {
        private get => _appliedForce;
        set
        {
            _appliedForce = value;
            OnAppliedForceChanges();
        }
    }

    private double _appliedForceAngle;
    public double AppliedForceAngle
    {
        private get => _appliedForceAngle;
        set
        {
            _appliedForceAngle = value;
            OnAppliedForceChanges();
        }
    }

    private double _staticFrictionCoefficient;
    public double StaticFrictionCoefficient
    {
        private get => _staticFrictionCoefficient;
        set
        {
            if(value >= KineticFrictionCoefficient)
            {
                _staticFrictionCoefficient = value;
            }
        }
    }

    private double _kineticFrictionCoefficient;
    public double KineticFrictionCoefficient
    {
        private get => _kineticFrictionCoefficient;
        set
        {
            if(value <= StaticFrictionCoefficient)
            {
                _kineticFrictionCoefficient = value;
            }
        }
    }

    public double Gravity { private get; set; } = Constants.EarthGravitationalAcceleration;
    #endregion


    #region ON APPLIED FORCE CHANGES
    private void OnAppliedForceChanges()   // To components 
    {
        _appliedForceX.Magnitude = Forces.ForceAdjacent(AppliedForce, Math.Abs(AppliedForceAngle));
        if (AppliedForce < 0)
        {
            _appliedForceX.Direction = DirectionXY.Xnegative;
        }
        else
        {
            _appliedForceX.Direction = DirectionXY.Xpositive;
        }

        _appliedForceY.Magnitude = Forces.ForceOpposite(AppliedForce, Math.Abs(AppliedForceAngle));
        if (AppliedForceAngle < 0)
        {
            _appliedForceY.Direction = DirectionXY.Ynegative;
        }
        else
        {
            _appliedForceY.Direction = DirectionXY.Ypositive;
        }
    }
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
        get => box.WeightY.SignedMagnitude;
        set
        {
            box.WeightY.Magnitude = value;
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
    private readonly Force _appliedForceX = new Force();
    private readonly Force _appliedForceY = new Force();

    private readonly Force _maxStaticFriction = new Force();
    private readonly Force _staticFriction = new Force();
    private readonly Force _kineticFriction = new Force();

    private readonly Force _fNetX = new Force();
    private readonly Force _fNetY = new Force();

    private FlatSurfaceState? _currentState;

    private const double _surfaceInclination = 0.0;

    private double _segmentElapsedTime;
    private double _totalElapsedTime;

    private double _firstInitialVelocityForRun;
    private double _segmentStartPosition;
    private bool _hasliftOffWarning;
    #endregion


    public FlatSurface()
    {
        Mass = 5.0;     // default to non zero mass
    }


    public void Restart()
    {
        _segmentElapsedTime = default;
        _totalElapsedTime = default;

        Position = 0.0;
        _segmentStartPosition = Position;

        if(InitialVelocity == _firstInitialVelocityForRun)
        {
            InitialVelocity = _firstInitialVelocityForRun;
        }
        else if(Segments.Count != 0)
        {
            InitialVelocity = _firstInitialVelocityForRun;
            Segments.Clear();
        }
        else
        {
            Velocity = InitialVelocity;
        }

        Acceleration = 0.0;
        _fNetX.Magnitude = 0.0;
        _fNetX.Direction = DirectionXY.Xpositive;
        _fNetY.Magnitude = 0.0;
        _fNetY.Direction = DirectionXY.Ypositive;
    }

    public void Reset() // not used
    {
        _segmentElapsedTime = default;

        Mass = 0.0;

        double temp = InitialVelocity;
        Velocity = temp;

        Position = 0.0;
        Velocity = 0.0;
        Acceleration = 0.0;
        Weight = 0.0;
        Normal = 0.0;
    }


    public override void Update(double delta)   // using TotalTime instead of small deltas in calculations
    {                                               // to avoid double inaccuracy compounding over time

        if (Math.Abs(Weight) < _appliedForceY.Magnitude && Weight * _appliedForceY.SignedMagnitude < 0)
        {
            _hasliftOffWarning = true;
        }
        else
        {
            _hasliftOffWarning = false;
        }


        if(_totalElapsedTime == 0.0)
        {
            _firstInitialVelocityForRun = InitialVelocity;
        }

        _segmentElapsedTime += delta;
        _totalElapsedTime += delta;

        // weight & normal
        Weight = Forces.WeightPerpendicular(Mass, _surfaceInclination, Gravity);
        Normal = Weight + _appliedForceY.SignedMagnitude;

        // fnetY
        double tempMagnitude = _appliedForceY.SignedMagnitude + Weight + Normal;
        if (tempMagnitude < 0)
        {
            _fNetY.Direction = DirectionXY.Ynegative;
        }
        _fNetY.Magnitude = Math.Abs(tempMagnitude);

        // friction magnitude
        _kineticFriction.Magnitude = Forces.Friction(KineticFrictionCoefficient, Normal);
        _maxStaticFriction.Magnitude = Forces.Friction(StaticFrictionCoefficient, Normal);


        // fNetX, frictions
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
                _staticFriction.Magnitude = 0.0;
                _staticFriction.Direction = DirectionXY.Xpositive;

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
            Position = _segmentStartPosition + Motion.DisplacementUsingAcceleration(InitialVelocity, _segmentElapsedTime, Acceleration);
            Velocity = Motion.FinalVelocity(InitialVelocity, Acceleration, _segmentElapsedTime);
        }    


        if (previousVelocity * Velocity <= 0.0 && previousVelocity != 0.0)      // segment change
        {

            _kineticFriction.Magnitude = 0.0;
            _kineticFriction.Direction = DirectionXY.Xpositive;

            _kineticFriction.Magnitude = 0.0;
            _kineticFriction.Direction = DirectionXY.Xpositive;

            if (_maxStaticFriction.Magnitude >= _appliedForceX.Magnitude)
            {
                _staticFriction.Magnitude = _appliedForceX.Magnitude;
                _staticFriction.Direction = _appliedForceX.Direction.Negate();

                _fNetX.Magnitude = 0.0;
                _fNetX.Direction = DirectionXY.Xpositive;
            }
            else
            {
                _kineticFriction.Magnitude = Forces.Friction(KineticFrictionCoefficient, Normal);
                _kineticFriction.Direction = _appliedForceX.Direction.Negate();

                _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _kineticFriction.SignedMagnitude;
                _fNetX.Direction = _appliedForceX.Direction;
            }

            // saving current segment and preparing next
            double exactStopTime = - InitialVelocity / Acceleration;
            Position = _segmentStartPosition + Motion.DisplacementUsingAcceleration(InitialVelocity, exactStopTime, Acceleration);
            double finalVelocity = 0.0;

            FlatSurfaceSegment segment = new FlatSurfaceSegment(exactStopTime, Position, InitialVelocity, finalVelocity, Acceleration);
            Segments.Add(segment);

            if(_fNetX.Magnitude == 0.0)
            {
                Acceleration = 0.0;
            }

            _segmentElapsedTime = default;
            _segmentStartPosition = Position;
            InitialVelocity = 0.0;
        }
    }


    public FlatSurfaceState GetCurrentState()
    {
        _currentState = new FlatSurfaceState(_totalElapsedTime, 
                                            Mass, Position, Velocity, Acceleration,
                                            Normal, Weight, StaticFrictionCoefficient, KineticFrictionCoefficient,
                                            _maxStaticFriction.SignedMagnitude, _staticFriction.SignedMagnitude ,_kineticFriction.SignedMagnitude,
                                            _appliedForceX.SignedMagnitude, _appliedForceY.SignedMagnitude,
                                            _fNetX.SignedMagnitude, _fNetY.SignedMagnitude,
                                            _hasliftOffWarning );

        return _currentState;
    }
}
