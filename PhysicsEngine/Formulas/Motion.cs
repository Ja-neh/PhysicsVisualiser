using System;

namespace PhysicsEngine.Formulas;


public static class Motion
{
    public static double FinalVelocity(double initialVelocity, double acceleration, double deltaTime)
    {
        return initialVelocity + acceleration * deltaTime;
    }

    public static double FinalVelocitySqured(double initialVelocity, double acceleration, double displacement)
    {
        return Math.Pow(initialVelocity, 2) + 2 * acceleration * displacement;
    }

    public static double DisplacementUsingAcceleration(double initialVelocity, double deltaTime, double acceleration)
    {
        return initialVelocity * deltaTime + 0.5 * acceleration * Math.Pow(deltaTime, 2);
    }

    public static double DisplacementUsingFinalVelocity(double initialVelocity, double fVelocity, double deltaTime)
    {
        return 0.5 * (initialVelocity + fVelocity) * deltaTime;
    }
}
