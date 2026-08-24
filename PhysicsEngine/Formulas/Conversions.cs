namespace PhysicsEngine.Formulas;

public static class Conversions
{
    public static double DegreesToRadians(double angleInDegrees)
    {
        return angleInDegrees * (Math.PI / 180.0);
    }
}
