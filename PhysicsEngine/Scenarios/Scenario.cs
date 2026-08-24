namespace PhysicsEngine.Scenarios;


public abstract class Scenario
{
    protected abstract void Initialize();
    public abstract void Update(double delta);
}
