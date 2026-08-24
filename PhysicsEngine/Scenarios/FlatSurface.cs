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
        _appliedForceX.Magnitude = Forces.ForceAdjacent(AppliedForce, AppliedForceAngle);
        _appliedForceY.Magnitude = Forces.ForceOpposite(AppliedForce, AppliedForceAngle);

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
        Velocity = 0.0;
        Acceleration = 0.0;
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

        // friction
        _friction.Magnitude = Forces.Friction(FrictionCoefficient, Normal); 
        if (_appliedForceX.Magnitude <= _friction.Magnitude)
        {
            Acceleration = 0.0;
            Velocity = 0.0;
        }
        else
        {
            _friction.Direction = _appliedForceX.Direction.Negate();
            //if(_previousVelocity < 0.0001)
            //{
            //    Acceleration = 0.0;
            //    Velocity = 0.0;
            //}
        }

        // fnetX
        _fNetX.Magnitude = _appliedForceX.SignedMagnitude + _friction.SignedMagnitude;
        _fNetX.Direction = _appliedForceX.Direction;

        // fnetY
        double tempMagnitude = _appliedForceY.SignedMagnitude + Weight + Normal;
        if (tempMagnitude < 0)
        {
            _fNetY.Direction = DirectionXY.Ynegative;
        }
        _fNetY.Magnitude = Math.Abs(tempMagnitude);

        // x, v, a
        Acceleration = _fNetX.SignedMagnitude / Mass;
        Position = Motion.DisplacementUsingAcceleration(InitialVelocity, CurrentTime, Acceleration);
        Velocity = Motion.FinalVelocity(InitialVelocity, Acceleration, CurrentTime);
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
