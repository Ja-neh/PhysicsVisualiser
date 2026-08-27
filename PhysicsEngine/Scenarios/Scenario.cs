namespace PhysicsEngine.Scenarios;

public abstract record ScenarioState();

public abstract class Scenario
{
    protected abstract void Initialize();
    public abstract void Update(double delta);
}
