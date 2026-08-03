namespace PhysicsEngine.Bodies;


public abstract class PhysicsBody
{
    public double Mass { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double AccelerationX { get; set; }
    public double AccelerationY { get; set; }

    public double WeightX { get; set; }
    public double WeightY { get; set; }
    public double Normal { get; set; }

    public PhysicsBody()
    {
        Mass = 5.0;
        PositionX = PositionY = 0.0;
        VelocityX = VelocityY = 0.0;
        AccelerationX = AccelerationY = 0.0;
    }
}
