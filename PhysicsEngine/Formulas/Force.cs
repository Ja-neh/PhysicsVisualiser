using System;

namespace PhysicsEngine.Formulas;


public static class Force
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

    public static double Impulse(double mass, double iVelocity, double fVelocity)
    {
        return mass * (fVelocity - iVelocity);
    }

    public static double GravitationalForce(double massOne, double massTwo, double radius)
    {
        return (Constants.universalGravitationalConstant * massOne * massTwo) * ( 1 / Math.Pow(radius, 2) );
    }

    public static double GravitationalAcceleration(double mass, double radius)
    {
        return (Constants.universalGravitationalConstant * mass) * ( 1 / Math.Pow(radius, 2) );
    }
}
