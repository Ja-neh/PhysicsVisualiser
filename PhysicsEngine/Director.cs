using System;
using PhysicsEngine.Scenarios;

namespace PhysicsEngine;

public class Director
{
    public Scenario Scene { get; set; }

    public Director(Scenario scenario)
    {
        Scene = scenario;
        Scene.Initialize();
    }

    public void Step(double deltaTime)
    {
        Scene.Update(deltaTime);
    }

    public void SetScenario(Scenario scenario)
    {
        Scene = scenario;
        Scene.Initialize();
    }
}
