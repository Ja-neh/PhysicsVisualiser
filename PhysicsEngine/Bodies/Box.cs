namespace PhysicsEngine.Bodies;


internal class Box : PhysicsBody
{
    private double _initialVelocityX;
    private double _initialVelocityY;

    public double InitialVelocityX 
    {
        get => _initialVelocityX;
        set { _initialVelocityX = VelocityX = value; }  // update velocity when initial-velocity is changed
    }
    
    public double InitialVelocityY
    {
        get => _initialVelocityY;
        set { _initialVelocityY = VelocityY = value; }  // update velocity when initial-velocity is changed
    }

    public Box() : base() {}
}
