using System;
using PhysicsEngine.Bodies;
using PhysicsEngine.Formulas;
using PhysicsEngine.Quantities;

namespace PhysicsEngine.Scenarios;


public class FlatSurface : Scenario
{
    public Box box;

    public double Mass { get; set; }
    public double InitialVelocityX { get; set; }
    public double AppliedForce { get; set; }
    public double AppliedForceAngle { get; set; }
    public double SurfaceInclination { get; set; }
    public double FrictionCoefficient { get; set; }
    public double Gravity { get; set; }

    // FIELDS
    private double _accumulatedTime;
    private Force AppliedForceX = new Force(0, DirectionXY.Xpositive);
    private Force AppliedForceY = new Force(0, DirectionXY.Ypositive);


    public FlatSurface()
    {
        box = new Box();
        Gravity = Constants.earthGravitationalAcceleration;

        _accumulatedTime = 0.0;
    }


    public override void Initialize()
    {
        if (Mass != 0.0)
        {
            box.Mass = this.Mass;
        }
        if (InitialVelocityX != 0.0)
        {
            box.InitialVelocityX = this.InitialVelocityX;
        }

        AppliedForceX.Magnitude = Forces.ForceAdjacent(AppliedForce, Math.Abs(AppliedForceAngle));
        if(AppliedForce < 0)
        {
            AppliedForceX.Direction = DirectionXY.Xnegative;
        }

        AppliedForceY.Magnitude = Forces.ForceOpposite(AppliedForce, Math.Abs(AppliedForceAngle));
        if(AppliedForceAngle < 0)
        {
            AppliedForceY.Direction = DirectionXY.Ynegative;
        }
    }


    public void ResetAccumulatedTime()
    {
        _accumulatedTime = 0.0;
    }


    public override void Update(double delta)
    {
        _accumulatedTime += delta;

        // weight & normal
        box.WeightX.Magnitude = Forces.WeightParallel(box.Mass, SurfaceInclination);
        box.WeightY.Magnitude = Forces.WeightPerpendicular(box.Mass, SurfaceInclination);
        box.Normal.Magnitude = box.WeightY.SignedMagnitude + AppliedForceY.SignedMagnitude;

        // friction
        Force friction = new Force(0, DirectionXY.Xpositive);
        friction.Magnitude = Forces.Friction(FrictionCoefficient, box.Normal.Magnitude);  
        if (box.VelocityX < 0)
        {
            friction.Direction = DirectionXY.Xpositive;
        }
        else
        {
            friction.Direction = DirectionXY.Xnegative;
        }

        // fnet
        Force fNetX = new Force(0, DirectionXY.Xpositive);
        double tempMagnitude = AppliedForceX.SignedMagnitude + box.WeightX.SignedMagnitude + friction.SignedMagnitude;
        if(tempMagnitude < 0)
        {
            fNetX.Direction = DirectionXY.Xnegative;
        }
        fNetX.Magnitude = Math.Abs(tempMagnitude);

        Force fNetY = new Force(0, DirectionXY.Ypositive);
        tempMagnitude = AppliedForceY.SignedMagnitude + box.WeightY.SignedMagnitude + box.Normal.SignedMagnitude;
        if (tempMagnitude < 0)
        {
            fNetY.Direction = DirectionXY.Ynegative;
        }
        fNetY.Magnitude = Math.Abs(tempMagnitude);

        // x, v, a
        box.AccelerationX = fNetX.SignedMagnitude / box.Mass;
        box.AccelerationY = fNetY.SignedMagnitude / box.Mass;

        box.PositionX = box.InitialVelocityX * _accumulatedTime + 0.5 * box.AccelerationX * Math.Pow(_accumulatedTime , 2);

        box.VelocityX = box.InitialVelocityX + box.AccelerationX * _accumulatedTime;


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