using System;
using PhysicsEngine.Bodies;
using PhysicsEngine.Formulas;
using PhysicsEngine.Quantities;

namespace PhysicsEngine.Scenarios;


public class FlatSurface : Scenario
{
    private readonly Box box = new Box();

    #region PROPERTIES
    // GET and SET
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

    public double Position
    {
        get => box.PositionX;
        set
        {
            box.PositionX = value;
        }
    }

    public double Velocity
    {
        get => box.VelocityX;
        set
        {
            box.VelocityX = value;
        }        
    }

    public double Acceleration
    {
        get => box.AccelerationX;
        set
        {
            box.AccelerationX = value;
        }
    }

    public double Weight
    {
        get => box.WeightX.SignedMagnitude;
        set
        {
            box.WeightX.Magnitude = value;
        }
    }

    public double Normal
    {
        get => box.Normal.SignedMagnitude;
        set
        {
            box.Normal.Magnitude = value;
        }
    }

    public double AppliedForce { get; set; }
    public double AppliedForceAngle { get; set; }

    public double SurfaceInclination { get; set; }
    public double FrictionCoefficient { get; set; }

    public double Gravity { get; set; } = Constants.earthGravitationalAcceleration;

    // GET only to outside
    public double CurrentTime { get; private set; }

    public double AppliedForceX
    {
        get => _appliedForceX.SignedMagnitude;
    }

    public double AppliedForceY
    {
        get => _appliedForceY.SignedMagnitude;
    }

    public double Friction
    {
        get => _friction.SignedMagnitude;
    }

    public double FNetX
    {
        get => _fNetX.SignedMagnitude;
    }

    public double FNetY
    {
        get => _fNetY.SignedMagnitude;
    }
    #endregion

    // FIELDS
    private readonly Force _appliedForceX = new Force(0, DirectionXY.Xpositive);
    private readonly Force _appliedForceY = new Force(0, DirectionXY.Ypositive);

    private readonly Force _friction = new Force(0, DirectionXY.Xpositive);

    private readonly Force _fNetX = new Force(0, DirectionXY.Xpositive);
    private readonly Force _fNetY = new Force(0, DirectionXY.Xpositive);

    private double _previousVelocity;


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
    {                                               // to avoid double inaccuracy
        if (CurrentTime == default) Initialize();

        CurrentTime += delta;

        // weight & normal
        Weight = Forces.WeightPerpendicular(Mass, SurfaceInclination);
        Normal = Weight + _appliedForceY.SignedMagnitude;

        // fnetY
        double tempMagnitude = _appliedForceY.SignedMagnitude + Weight + Normal;
        if (tempMagnitude < 0)
        {
            _fNetY.Direction = DirectionXY.Ynegative;
        }
        _fNetY.Magnitude = Math.Abs(tempMagnitude);

        // friction magnitude
        _friction.Magnitude = Forces.Friction(FrictionCoefficient, Normal);

        // fnetX & friction direction
        bool updatePosVelAcc = true;
        if (_appliedForceX.Magnitude > _friction.Magnitude)
        {
            _fNetX.Direction = _appliedForceX.Direction;
            _friction.Direction = _appliedForceX.Direction.Negate();

            _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _friction.SignedMagnitude;
        }
        else if(_appliedForceX.Magnitude < _friction.Magnitude)
        {
            if (Velocity == 0.0)        // no movement, no kinetic friction, no net force
            {
                _friction.Magnitude = 0.0;
                _friction.Direction = DirectionXY.Xpositive;
                _fNetX.Magnitude = 0.0;
            }
            else if (Velocity > 0.0)        // friction works against direction of motion
            {
                _friction.Direction = DirectionXY.Xnegative;
                _fNetX.Direction = _friction.Direction;

                _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _friction.SignedMagnitude;
            }
            else if(Velocity < 0.0)
            {
                _friction.Direction = DirectionXY.Xpositive;
                _fNetX.Direction = _friction.Direction;

                _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _friction.SignedMagnitude;
            }

            if(Math.Abs(Velocity) < 0.01)
            {
                _friction.Magnitude = 0.0;
                _friction.Direction = DirectionXY.Xpositive;
                _fNetX.Magnitude = 0.0;
                Acceleration = 0.0;
                Velocity = 0.0;
                updatePosVelAcc = false;
            }
        }
        else
        {
            _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _friction.SignedMagnitude;
            _fNetX.Direction = DirectionXY.Xpositive;
            _friction.Direction = _appliedForceX.Direction.Negate();
        }


        // x, v, a
        if (updatePosVelAcc)
        {
            Acceleration = _fNetX.SignedMagnitude / Mass;
            Position = Motion.DisplacementUsingAcceleration(InitialVelocity, CurrentTime, Acceleration);
            Velocity = Motion.FinalVelocity(InitialVelocity, Acceleration, CurrentTime);
        }
        
        _previousVelocity = Velocity;


        Console.WriteLine("WeightX : " + box.WeightX.SignedMagnitude);
        Console.WriteLine("WeightY : " + box.WeightY.SignedMagnitude);
        Console.WriteLine("Normal : " + box.Normal.SignedMagnitude);
        Console.WriteLine("Position : " + box.PositionX);
        Console.WriteLine("Velocity : " + box.VelocityX);
        Console.WriteLine("Acceleration : " + box.AccelerationX);
        Console.WriteLine("Friction : " + _friction.SignedMagnitude);
        Console.WriteLine("FnetX : " + _fNetX.SignedMagnitude);
        Console.WriteLine("FnetY : " + _fNetY.SignedMagnitude);
        Console.WriteLine("--------------------------------------------------------");
    }


}
