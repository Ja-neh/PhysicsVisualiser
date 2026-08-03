using System;

namespace PhysicsEngine.Formulas;


public static class Motion
{
    public static double FinalVelocity(double iVelocity, double acceleration, double deltaTime)
    {
        return iVelocity + acceleration * deltaTime;
    }

    public static double FinalVelocitySqured(double iVelocity, double acceleration, double displacement)
    {
        return Math.Pow(iVelocity, 2) + 2 * acceleration * displacement;
    }

    public static double DisplacementUsingAcceleration(double iVelocity, double deltaTime, double acceleration)
    {
        return iVelocity * deltaTime + 0.5 * acceleration * Math.Pow(deltaTime, 2);
    }

    public static double DisplacementUsingFinalVelocity(double iVelocity, double fVelocity, double deltaTime)
    {
        return 0.5 * (iVelocity + fVelocity) * deltaTime;
    }
}
