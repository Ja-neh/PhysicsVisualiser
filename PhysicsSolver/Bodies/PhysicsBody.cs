using PhysicsSolver.Quantities;

namespace PhysicsSolver.Bodies;


internal abstract class PhysicsBody
{
    public double Mass { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double AccelerationX { get; set; }
    public double AccelerationY { get; set; }

    public Force WeightX { get; set; }
    public Force WeightY { get; set; }
    public Force Normal { get; set; }

    public PhysicsBody()
    {
        WeightX = new Force(0, DirectionXY.Xnegative);
        WeightY = new Force(0, DirectionXY.Ynegative);
        Normal = new Force(0, DirectionXY.Ypositive);
    }
}
