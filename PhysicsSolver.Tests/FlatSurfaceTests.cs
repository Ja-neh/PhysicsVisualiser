using FluentAssertions;
using PhysicsSolver;
using PhysicsSolver.Formulas;
using PhysicsSolver.Scenarios;

namespace PhysicsSolver.Tests;

public class FlatSurfaceTests
{
    private const double Precision = 1e-5;

    #region Initial State & Properties
    [Fact]
    public void GetCurrentState_InitialState_MatchesConfiguredValues()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            InitialVelocity = 12.0,
            AppliedForce = 20.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.4,
            KineticFrictionCoefficient = 0.3
        };

        var state = scenario.GetCurrentState();

        state.Time.Should().Be(0.0);
        state.Position.Should().Be(0.0);
        state.Velocity.Should().Be(12.0);
        state.Mass.Should().Be(5.0);
        state.StaticFrictionCoefficient.Should().Be(0.4);
        state.KineticFrictionCoefficient.Should().Be(0.3);
        scenario.Segments.Should().BeEmpty();
    }

    [Fact]
    public void Mass_SetNonPositive_IgnoresValue()
    {
        var scenario = new FlatSurface { Mass = 4.0 };
        scenario.Mass = -2.0;
        scenario.GetCurrentState().Mass.Should().Be(4.0);

        scenario.Mass = 0.0;
        scenario.GetCurrentState().Mass.Should().Be(4.0);
    }

    [Fact]
    public void FrictionCoefficients_EnforcesStaticGreaterThanOrEqualToKinetic()
    {
        var scenario = new FlatSurface();

        // Set static first, then kinetic lower -> accepted
        scenario.StaticFrictionCoefficient = 0.5;
        scenario.KineticFrictionCoefficient = 0.3;
        scenario.GetCurrentState().StaticFrictionCoefficient.Should().Be(0.5);
        scenario.GetCurrentState().KineticFrictionCoefficient.Should().Be(0.3);

        // Setting kinetic higher than static -> rejected
        scenario.KineticFrictionCoefficient = 0.8;
        scenario.GetCurrentState().KineticFrictionCoefficient.Should().Be(0.3);

        // Setting static lower than current kinetic -> rejected
        scenario.StaticFrictionCoefficient = 0.1;
        scenario.GetCurrentState().StaticFrictionCoefficient.Should().Be(0.5);
    }
    #endregion

    #region FNETX <= MAX STATIC FRICTION & FNETX > MAX STATIC FRICTION
    [Fact]
    public void Update_AppliedForceLessThanMaxStaticFriction_RemainsAtRest()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            InitialVelocity = 0.0,
            AppliedForce = 10.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.3
        };

        // Run for several steps
        double dt = 0.05;
        for (int i = 0; i < 20; i++)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();

        state.Position.Should().Be(0.0);
        state.Velocity.Should().Be(0.0);
        state.Acceleration.Should().Be(0.0);
        state.FNetX.Should().Be(0.0);
        state.StaticFriction.Should().Be(-10.0); // Exactly opposes the 10N applied force
        state.KineticFriction.Should().Be(0.0);
    }

    [Fact]
    public void Update_AppliedForceExceedsMaxStaticFriction_BeginsAccelerating()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            InitialVelocity = 0.0,
            AppliedForce = 30.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.4,
            KineticFrictionCoefficient = 0.2
        };

        double dt = 0.1;
        scenario.Update(dt);

        var state = scenario.GetCurrentState();

        double expectedFNetX = 30.0 - (0.2 * 5.0 * Constants.EarthGravitationalAcceleration);
        double expectedAcc = expectedFNetX / 5.0;

        state.FNetX.Should().BeApproximately(expectedFNetX, Precision);
        state.Acceleration.Should().BeApproximately(expectedAcc, Precision);
        state.Velocity.Should().BeGreaterThan(0.0);
        state.Position.Should().BeGreaterThan(0.0);
        state.StaticFriction.Should().Be(0.0);
        state.KineticFriction.Should().BeLessThan(0.0); // Opposes motion
    }
    #endregion

    #region Constant Velocity (Dynamic Equilibrium: Fa = fk)
    [Fact]
    public void Update_AppliedForceMatchesKineticFriction_MaintainsConstantVelocity()
    {
        double mass = 5.0;
        double v0 = 6.0;
        double mu = 0.2;
        double normal = mass * Constants.EarthGravitationalAcceleration;
        double fk = mu * normal; // 9.8 N

        var scenario = new FlatSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            AppliedForce = fk,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.4,
            KineticFrictionCoefficient = mu
        };

        double dt = 0.05;
        int steps = 60;
        double expectedTotalTime = steps * dt;

        for (int i = 0; i < steps; i++)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();

        state.Time.Should().BeApproximately(expectedTotalTime, Precision);
        state.FNetX.Should().BeApproximately(0.0, Precision);
        state.Acceleration.Should().BeApproximately(0.0, Precision);
        state.Velocity.Should().BeApproximately(v0, Precision);
        state.Position.Should().BeApproximately(v0 * expectedTotalTime, Precision);
        scenario.Segments.Should().BeEmpty(); // Never stops
    }
    #endregion

    #region Deceleration & Analytical Stopping
    [Fact]
    public void Update_SlidingUnderKineticFriction_DeceleratesAndStopsAtExactDistance()
    {
        double v0 = 10.0;
        double mu = 0.2;
        double mass = 5.0;
        double dt = 1.0 / 60.0;

        var scenario = new FlatSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            AppliedForce = 0.0,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu
        };

        // Fnet = fk = mu * N = mu * m * g   ->   a = Fnet / m = mu * g
        double theoreticalStopDistance = (v0 * v0) / (2.0 * mu * Constants.EarthGravitationalAcceleration);
        double theoreticalStopTime = v0 / (mu * Constants.EarthGravitationalAcceleration);

        // Run until past stopping time (6 seconds)
        for (double t = 0; t < 6.0; t += dt)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();

        state.Velocity.Should().Be(0.0);
        state.Acceleration.Should().Be(0.0);
        state.Position.Should().BeApproximately(theoreticalStopDistance, Precision);

        scenario.Segments.Should().HaveCount(1);
        var segment = scenario.Segments[0];
        segment.InitialVelocity.Should().Be(v0);
        segment.FinalVelocity.Should().Be(0.0);
        segment.ElapsedTime.Should().BeApproximately(theoreticalStopTime, Precision);
        segment.StartPosition.Should().BeApproximately(theoreticalStopDistance, Precision);
    }

    [Fact]
    public void Update_MovingLeft_DeceleratesAndStopsAtNegativeDistance()
    {
        double v0 = -8.0;
        double mu = 0.25;
        double dt = 1.0 / 60.0;

        var scenario = new FlatSurface
        {
            Mass = 4.0,
            InitialVelocity = v0,
            AppliedForce = 0.0,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu
        };

        double theoreticalStopDistance = -(v0 * v0) / (2.0 * mu * Constants.EarthGravitationalAcceleration);

        // Run until stopped
        for (double t = 0; t < 5.0; t += dt)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();

        state.Velocity.Should().Be(0.0);
        state.Acceleration.Should().Be(0.0);
        state.Position.Should().BeApproximately(theoreticalStopDistance, Precision);
        state.Position.Should().BeLessThan(0.0);
        scenario.Segments.Should().HaveCount(1);
    }

    [Fact]
    public void Update_AfterStopping_PositionRemainsFixedOverTime()
    {
        var scenario = new FlatSurface
        {
            Mass = 3.0,
            InitialVelocity = 4.0,
            AppliedForce = 0.0,
            StaticFrictionCoefficient = 0.2,
            KineticFrictionCoefficient = 0.2
        };

        // Run past stop time
        for (int i = 0; i < 200; i++)
        {
            scenario.Update(1.0 / 60.0);
        }

        double stoppedPosition = scenario.GetCurrentState().Position;
        stoppedPosition.Should().BeGreaterThan(0.0);

        // Run an additional 100 steps
        for (int i = 0; i < 100; i++)
        {
            scenario.Update(1.0 / 60.0);
        }

        var stateAfterWait = scenario.GetCurrentState();
        stateAfterWait.Position.Should().Be(stoppedPosition);
        stateAfterWait.Velocity.Should().Be(0.0);
    }
    #endregion

    #region Active Reversal (Deceleration then Direction Reversal)
    [Fact]
    public void Update_OpposingAppliedForceExceedingStaticFriction_ReversesDirectionAfterStopping()
    {
        // Phase 1 (v > 0 - moving right)
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            InitialVelocity = 10.0,
            AppliedForce = -30.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.3,
            KineticFrictionCoefficient = 0.2
        };

        double dt = 0.05;

        // until stop occurs (1.3s > 1.256s stop time)
        for (int i = 0; i < 26; i++) // 26 * 0.05 = 1.30s
        {
            scenario.Update(dt);
        }

        scenario.Segments.Should().HaveCount(1);
        var seg1 = scenario.Segments[0];
        seg1.InitialVelocity.Should().Be(10.0);
        seg1.FinalVelocity.Should().Be(0.0);
        seg1.Acceleration.Should().BeApproximately(-7.96, Precision);
        seg1.ElapsedTime.Should().BeApproximately(10.0 / 7.96, Precision);
        seg1.StartPosition.Should().BeApproximately(6.2814, Precision);

        // Phase 2 (v < 0 - moving left)
        for (int i = 0; i < 30; i++)
        {
            scenario.Update(dt);
        }

        var reversedState = scenario.GetCurrentState();

        reversedState.Velocity.Should().BeLessThan(0.0);
        reversedState.Position.Should().BeLessThan(6.2814); // Moved back from peak position
        reversedState.Acceleration.Should().BeApproximately(-4.04, Precision);
        reversedState.FNetX.Should().BeApproximately(-20.2, Precision);
        reversedState.KineticFriction.Should().BeApproximately(9.8, Precision);
    }
    #endregion

    #region Moon, Mars Gravity / Custom Gravities
    [Fact]
    public void Update_OnMoon_HasLowerWeightAndLongerStoppingDistance()
    {
        double v0 = 5.0;
        double mu = 0.2;
        double mass = 10.0;

        var earthScenario = new FlatSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            Gravity = Constants.EarthGravitationalAcceleration,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu
        };

        var moonScenario = new FlatSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            Gravity = Constants.MoonGravitationalAcceleration,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu
        };

        earthScenario.Update(0.1);
        moonScenario.Update(0.1);

        var earthState = earthScenario.GetCurrentState();
        var moonState = moonScenario.GetCurrentState();

        // Moon normal force and friction are smaller
        moonState.Normal.Should().BeLessThan(earthState.Normal);
        Math.Abs(moonState.KineticFriction).Should().BeLessThan(Math.Abs(earthState.KineticFriction));

        // Finish running both to a stop
        for (double t = 0.1; t < 20.0; t += 0.1)
        {
            earthScenario.Update(0.1);
            moonScenario.Update(0.1);
        }

        // Moon stopping distance must be greater than Earth stopping distance
        moonScenario.GetCurrentState().Position.Should().BeGreaterThan(earthScenario.GetCurrentState().Position);
    }
    #endregion

    #region Angled Force Effects on Normal Force & Friction
    [Fact]
    public void Update_AngledForceUpward_ReducesNormalForceAndFriction()
    {
        var flatForce = new FlatSurface
        {
            Mass = 10.0,
            AppliedForce = 40.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.3
        };

        var angledForce = new FlatSurface
        {
            Mass = 10.0,
            AppliedForce = 40.0,
            AppliedForceAngle = Conversions.DegreesToRadians(30.0),
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.3
        };

        flatForce.Update(0.01);
        angledForce.Update(0.01);

        var flatState = flatForce.GetCurrentState();
        var angledState = angledForce.GetCurrentState();

        // Angled upward force reduces normal force
        angledState.Normal.Should().BeLessThan(flatState.Normal);
        // And therefore reduces friction
        Math.Abs(angledState.MaxStaticFriction).Should().BeLessThan(Math.Abs(flatState.MaxStaticFriction));
    }

    [Fact]
    public void Update_AngledForceDownward_IncreasesNormalForceAndFriction()
    {
        var flatForce = new FlatSurface
        {
            Mass = 10.0,
            InitialVelocity = 5.0,
            AppliedForce = 40.0,
            AppliedForceAngle = 0.0,
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.3
        };

        var angledDownwardForce = new FlatSurface
        {
            Mass = 10.0,
            InitialVelocity = 5.0,
            AppliedForce = 40.0,
            AppliedForceAngle = Conversions.DegreesToRadians(-30.0),
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.3
        };

        flatForce.Update(0.01);
        angledDownwardForce.Update(0.01);

        var flatState = flatForce.GetCurrentState();
        var angledState = angledDownwardForce.GetCurrentState();

        // Downward angled force pushes into the surface, increasing normal force
        angledState.Normal.Should().BeGreaterThan(flatState.Normal);
        // And therefore increases friction
        Math.Abs(angledState.MaxStaticFriction).Should().BeGreaterThan(Math.Abs(flatState.MaxStaticFriction));
        Math.Abs(angledState.KineticFriction).Should().BeGreaterThan(Math.Abs(flatState.KineticFriction));
    }

    [Fact]
    public void Update_UpwardForceExceedingWeight_TriggersLiftOffWarning()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            AppliedForce = 120.0,
            AppliedForceAngle = Conversions.DegreesToRadians(30.0),
            StaticFrictionCoefficient = 0.3,
            KineticFrictionCoefficient = 0.2
        };

        // Run updates
        scenario.Update(0.01);
        scenario.Update(0.01);

        var state = scenario.GetCurrentState();
        state.LiftOffWarning.Should().BeTrue();
    }

    [Fact]
    public void Update_UpwardForceBelowWeight_DoesNotTriggerLiftOffWarning()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            AppliedForce = 40.0,
            AppliedForceAngle = Conversions.DegreesToRadians(30.0),
            StaticFrictionCoefficient = 0.3,
            KineticFrictionCoefficient = 0.2
        };

        scenario.Update(0.01);
        scenario.Update(0.01);

        var state = scenario.GetCurrentState();
        state.LiftOffWarning.Should().BeFalse();
    }
    #endregion

    #region Restart
    [Fact]
    public void Restart_ResetsPositionTimeAndRestoresInitialConditions()
    {
        var scenario = new FlatSurface
        {
            Mass = 5.0,
            InitialVelocity = 8.0,
            AppliedForce = 0.0,
            StaticFrictionCoefficient = 0.2,
            KineticFrictionCoefficient = 0.2
        };

        // until it moves and stops (t_stop = 8 / 1.96 = 4.08s)
        for (int i = 0; i < 300; i++)   // 300 steps = 5.0s
        {
            scenario.Update(1.0 / 60.0);
        }

        scenario.GetCurrentState().Position.Should().BeGreaterThan(0.0);
        scenario.Segments.Should().NotBeEmpty();

        scenario.Restart();

        var restartedState = scenario.GetCurrentState();
        restartedState.Position.Should().Be(0.0);
        restartedState.Velocity.Should().Be(8.0);
        restartedState.Acceleration.Should().Be(0.0);
        restartedState.FNetX.Should().Be(0.0);
    }
    #endregion
}
