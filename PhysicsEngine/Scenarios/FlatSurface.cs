using System;
using PhysicsEngine.Bodies;

namespace PhysicsEngine.Scenarios;


public class FlatSurface : Scenario
{
    public Box box;

    public double AppliedForce { get; set; }
    public double FrictionCoefficient { get; set; }
    public double Gravity { get; set; }

    private double _accumulatedTime;


    public FlatSurface()
    {
        box = new Box();
        AppliedForce = 0.0;
        FrictionCoefficient = 0.0;
        Gravity = 9.8;
        _accumulatedTime = 0.0;
    }

    public override void Update(double delta)
    {
        _accumulatedTime += delta;

        box.WeightY = box.Mass * Gravity;
        box.Normal = box.WeightY;

        double friction = FrictionCoefficient * box.Normal;
        double fnet = AppliedForce - friction;

        box.AccelerationX = fnet / box.Mass;

        box.PositionX = box.InitialVelocityX * _accumulatedTime + 0.5 * box.AccelerationX * Math.Pow(_accumulatedTime , 2);

        box.VelocityX = box.InitialVelocityX + box.AccelerationX * _accumulatedTime;


        Console.WriteLine("Weight : " + box.WeightY);
        Console.WriteLine("Normal : " + box.Normal);
        Console.WriteLine("Position : " + box.PositionX);
        Console.WriteLine("Velocity : " + box.VelocityX);
        Console.WriteLine("Acceleration : " + box.AccelerationX);
        Console.WriteLine("Friction : " + friction);
        Console.WriteLine("Fnet : " + fnet);
        Console.WriteLine("--------------------------------------------------------");
    }


}