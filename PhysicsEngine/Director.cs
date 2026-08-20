using System;
using PhysicsEngine.Scenarios;

namespace PhysicsEngine;

public class Director
{
    private Scenario _scenario;

    public Director(Scenario scenario)
    {
        _scenario = scenario;
        _scenario.Initialize();
    }

}
