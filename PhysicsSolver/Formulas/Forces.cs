using System;

namespace PhysicsSolver.Formulas;


public static class Forces
{
    public static double FNet(double mass, double acceleration)
    {
        return mass * acceleration;
    }

    public static double Momentum(double mass, double velocity)
    {
        return mass * velocity;
    }

    public static double Friction(double frictionCoefficient, double normal)
    {
        return frictionCoefficient * normal;
    }

    public static double Impulse(double fnet, double deltaTime)
    {
        return fnet * deltaTime;
    }

    public static double Impulse(double mass, double initialVelocity, double finalVelocity)
    {
        return mass * (finalVelocity - initialVelocity);
    }

    public static double GravitationalForce(double massOne, double massTwo, double radius)
    {
        return (Constants.universalGravitationalConstant * massOne * massTwo) * ( 1 / Math.Pow(radius, 2) );
    }

    public static double GravitationalAcceleration(double mass, double radius)
    {
        return (Constants.universalGravitationalConstant * mass) * ( 1 / Math.Pow(radius, 2) );
    }

    public static double WeightParallel(double mass, double angle)
    {
        return mass * Constants.earthGravitationalAcceleration * Math.Sin(angle);
    }

    public static double WeightPerpendicular(double mass, double angle)
    {
        return mass * Constants.earthGravitationalAcceleration * Math.Cos(angle);
    }

    public static double ForceAdjacent(double force, double angle)
    {
        return force * Math.Cos(angle);
    }

    public static double ForceOpposite(double force, double angle)
    {
        return force * Math.Sin(angle);
    }

}
