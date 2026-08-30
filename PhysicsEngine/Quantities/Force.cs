namespace PhysicsEngine.Quantities;


internal enum DirectionXY
{
    Xpositive,
    Xnegative,
    Ypositive,
    Ynegative
}

internal static class DirectionXYExtensions
{
    public static DirectionXY Negate(this DirectionXY direction) => direction switch
    {
        DirectionXY.Xpositive => DirectionXY.Xnegative,
        DirectionXY.Xnegative => DirectionXY.Xpositive,
        DirectionXY.Ypositive => DirectionXY.Ynegative,
        DirectionXY.Ynegative => DirectionXY.Ypositive,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Not expected direction value: {direction}"), 
    };
}

internal class Force
{
    public DirectionXY Direction { get; set; }

    private double _magnitude;
    public double Magnitude
    {
        get => _magnitude;
        set { _magnitude = Math.Abs(value); }
    }

    public double SignedMagnitude
    {
        get
        {
            if (Direction == DirectionXY.Xpositive || Direction == DirectionXY.Ypositive)
            {
                return _magnitude;
            }
            else
            {
                return -(_magnitude);
            }
        }
    }


    public Force(double magnitude, DirectionXY direction){
        Direction = direction;
        Magnitude = magnitude;
    }

    public Force()  // Don't forget to set your values later
    {
        Direction = DirectionXY.Xpositive;
        Magnitude = 1.0;
    }
}
