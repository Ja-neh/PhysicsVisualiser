namespace PhysicsSolver.Formulas;

public static class Conversions
{
    public static double DegreesToRadians(double angleInDegrees)
    {
        return angleInDegrees * (Math.PI / 180.0);
    }

    public static double RadiansToDegrees(double angleInRadians)
    {
        return angleInRadians * (180.0 / Math.PI);
    }
}
