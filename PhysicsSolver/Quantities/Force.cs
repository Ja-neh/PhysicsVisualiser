namespace PhysicsSolver.Quantities;


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


#region FORCE INTERFACES AND ABSTRACT CLASS
internal interface IMagnitude
{
    double Magnitude { get; set; }
    double SignedMagnitude { get; }
}
internal interface IMutableDirection
{
    DirectionXY Direction { get; set; }
}
internal interface IImmutableDirection
{
    DirectionXY Direction { get; }
}

internal abstract class ForceBase : IMagnitude
{
    private double _magnitude;
    public double Magnitude
    {
        get => _magnitude;
        set { _magnitude = Math.Abs(value); }
    }

    public abstract double SignedMagnitude { get; }

    protected ForceBase(double magnitude)
    {
        Magnitude = magnitude;
    }
}

#endregion


internal class Force : ForceBase, IMutableDirection
{
    public DirectionXY Direction { get; set; }

    public override double SignedMagnitude
    {
        get
        {
            if (Direction == DirectionXY.Xpositive || Direction == DirectionXY.Ypositive)
            {
                return Magnitude;
            }
            else
            {
                return - Magnitude;
            }
        }
    }


    public Force(double magnitude, DirectionXY direction) : base(magnitude)
    {
        Direction = direction;
    }

    public Force() : base(0.0)
    {
        Direction = DirectionXY.Xpositive;
    }
}


internal class ForceXPositive : ForceBase, IImmutableDirection
{
    public DirectionXY Direction => DirectionXY.Xpositive;
    public override double SignedMagnitude => Magnitude;

    public ForceXPositive(double magnitude) : base(magnitude) { }
}

internal class ForceXNegative : ForceBase, IImmutableDirection
{
    public DirectionXY Direction => DirectionXY.Xnegative;
    public override double SignedMagnitude => - Magnitude;

    public ForceXNegative(double magnitude) : base(magnitude) { }
}

internal class ForceYPositive : ForceBase, IImmutableDirection
{
    public DirectionXY Direction => DirectionXY.Ypositive;
    public override double SignedMagnitude => Magnitude;

    public ForceYPositive(double magnitude) : base(magnitude) { }
}

internal class ForceYNegative : ForceBase, IImmutableDirection
{
    public DirectionXY Direction => DirectionXY.Ynegative;
    public override double SignedMagnitude => - Magnitude;

    public ForceYNegative(double magnitude) : base(magnitude) { }
}
