namespace PhysicsEngine.Quantities;


public enum DirectionXY
{
    Xpositive,
    Xnegative,
    Ypositive,
    Ynegative
}

public class Force
{
    public DirectionXY Direction { get; set; }

    private double _magnitude;
    public double Magnitude
    {
        get { return _magnitude; }
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
}
