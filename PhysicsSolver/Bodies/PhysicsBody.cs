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

    public Force WeightX { get; set; } = new Force(0, DirectionXY.Xnegative);
    public ForceYNegative WeightY { get; set; } = new ForceYNegative(0);
    public ForceYPositive Normal { get; set; } = new ForceYPositive(0);

}
