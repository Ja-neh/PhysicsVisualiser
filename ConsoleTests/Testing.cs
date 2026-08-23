using System;

using PhysicsEngine;
using PhysicsEngine.Scenarios;
using PhysicsEngine.Bodies;


namespace ConsoleTests;

class Program
{
    static void Main(string[] args)
    {
        FlatSurface scenario = new FlatSurface();

        scenario.Mass = 3;
        scenario.AppliedForce = 25;
        scenario.FrictionCoefficient = 0.2;

        double TotalTime = 2;
        double delta = 0.2;

        scenario.Initialize();

        for (double i = 0; i < TotalTime; i = i + delta)
        {
            scenario.Update(delta);
        }
    }
}
