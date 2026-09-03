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
internal interface IForce
{
    DirectionXY Direction { get; }
    double Magnitude { get; set; }
    double SignedMagnitude { get; }
}

internal interface IMutableDirectionForce : IForce
{
    new DirectionXY Direction { get; set; }
}

internal abstract class ForceBase : IForce
{
    private double _magnitude;
    public double Magnitude
    {
        get => _magnitude;
        set { _magnitude = Math.Abs(value); }
    }

    public abstract DirectionXY Direction { get; }
    public abstract double SignedMagnitude { get; }

    public ForceBase(double magnitude)
    {
        Magnitude = magnitude;
    }
}

#endregion


internal class Force : IMutableDirectionForce
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
                return Magnitude;
            }
            else
            {
                return - Magnitude;
            }
        }
    }


    public Force(double magnitude, DirectionXY direction){
        Direction = direction;
        Magnitude = magnitude;
    }

    public Force()
    {
        Direction = DirectionXY.Xpositive;
        Magnitude = 0.0;
    }
}


internal class ForceXPositive : ForceBase
{
    public override DirectionXY Direction => DirectionXY.Xpositive;
    public override double SignedMagnitude => Magnitude;

    public ForceXPositive(double magnitude) : base(magnitude) { }
}

internal class ForceXNegative : ForceBase
{
    public override DirectionXY Direction => DirectionXY.Xnegative;
    public override double SignedMagnitude => - Magnitude;

    public ForceXNegative(double magnitude) : base(magnitude) { }
}

internal class ForceYPositive : ForceBase
{
    public override DirectionXY Direction => DirectionXY.Ypositive;
    public override double SignedMagnitude => Magnitude;

    public ForceYPositive(double magnitude) : base(magnitude) { }
}

internal class ForceYNegative : ForceBase
{
    public override DirectionXY Direction => DirectionXY.Ynegative;
    public override double SignedMagnitude => - Magnitude;

    public ForceYNegative(double magnitude) : base(magnitude) { }
}
