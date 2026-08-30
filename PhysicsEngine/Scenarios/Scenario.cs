namespace PhysicsEngine.Scenarios;

public abstract record ScenarioState();

public abstract class Scenario
{
    public abstract void Update(double delta);
}
