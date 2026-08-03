namespace PhysicsEngine.Bodies;


public class Box : PhysicsBody
{
    private double _initialVelocityX;
    private double _initialVelocityY;

    public double InitialVelocityX 
    {
        get { return _initialVelocityX; }
        // update velocity when initial-velocity is changed
        set { _initialVelocityX = VelocityX = value; }
    }
    
    public double InitialVelocityY
    {
        get { return _initialVelocityY; }
        // update velocity when initial-velocity is changed
        set { _initialVelocityY = VelocityY = value; }
    }

    public Box() : base()
    {
        InitialVelocityX = VelocityX;
        InitialVelocityY = VelocityY;
    }
}
