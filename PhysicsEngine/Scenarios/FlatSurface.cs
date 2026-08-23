using System;
using PhysicsEngine.Bodies;
using PhysicsEngine.Formulas;
using PhysicsEngine.Quantities;

namespace PhysicsEngine.Scenarios;


public class FlatSurface : Scenario
{
    private readonly Box box = new Box();

    #region PROPERTIES
    public double CurrentTime { get; set; }

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
    #endregion

    // FIELDS
    private readonly Force _appliedForceX = new Force(0, DirectionXY.Xpositive);
    private readonly Force _appliedForceY = new Force(0, DirectionXY.Ypositive);


    public FlatSurface() {}


    public override void Initialize()   // To components 
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
        Force friction = new Force(0, DirectionXY.Xpositive);
        friction.Magnitude = Forces.Friction(FrictionCoefficient, Normal);  
        if (Velocity < 0)
        {
            friction.Direction = DirectionXY.Xpositive;
        }
        else
        {
            friction.Direction = DirectionXY.Xnegative;
        }

        // fnet
        Force fNetX = new Force(0, DirectionXY.Xpositive);
        double tempMagnitude = _appliedForceX.SignedMagnitude + friction.SignedMagnitude;
        if(tempMagnitude < 0)
        {
            fNetX.Direction = DirectionXY.Xnegative;
        }
        fNetX.Magnitude = Math.Abs(tempMagnitude);

        Force fNetY = new Force(0, DirectionXY.Ypositive);
        tempMagnitude = _appliedForceY.SignedMagnitude + Weight + Normal;
        if (tempMagnitude < 0)
        {
            fNetY.Direction = DirectionXY.Ynegative;
        }
        fNetY.Magnitude = Math.Abs(tempMagnitude);

        // x, v, a
        Acceleration = fNetX.SignedMagnitude / Mass;

        Position = Motion.DisplacementUsingAcceleration(InitialVelocity, CurrentTime, Acceleration);

        Velocity = Motion.FinalVelocity(InitialVelocity, Acceleration, CurrentTime);


        Console.WriteLine("WeightX : " + box.WeightX.SignedMagnitude);
        Console.WriteLine("WeightY : " + box.WeightY.SignedMagnitude);
        Console.WriteLine("Normal : " + box.Normal.SignedMagnitude);
        Console.WriteLine("Position : " + box.PositionX);
        Console.WriteLine("Velocity : " + box.VelocityX);
        Console.WriteLine("Acceleration : " + box.AccelerationX);
        Console.WriteLine("Friction : " + friction.SignedMagnitude);
        Console.WriteLine("FnetX : " + fNetX.SignedMagnitude);
        Console.WriteLine("FnetY : " + fNetY.SignedMagnitude);
        Console.WriteLine("--------------------------------------------------------");
    }


}
