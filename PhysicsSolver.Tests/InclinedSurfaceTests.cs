using FluentAssertions;
using PhysicsSolver;
using PhysicsSolver.Formulas;
using PhysicsSolver.Scenarios;

namespace PhysicsSolver.Tests;

public class InclinedSurfaceTests
{
    private const double Precision = 1e-4;

    #region Initial State & Properties
    [Fact]
    public void GetCurrentState_InitialState_MatchesConfiguredValues()
    {
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            InitialVelocity = 12.0,
            AppliedForce = 20.0,
            AppliedForceAngle = 0.0,
            SurfaceInclination = Math.PI / 6.0,
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
        state.LiftOffWarning.Should().BeFalse();
        scenario.Segments.Should().BeEmpty();
    }

    [Fact]
    public void Mass_SetNonPositive_IgnoresValue()
    {
        var scenario = new InclinedSurface { Mass = 4.0 };
        scenario.Mass = -2.0;
        scenario.GetCurrentState().Mass.Should().Be(4.0);

        scenario.Mass = 0.0;
        scenario.GetCurrentState().Mass.Should().Be(4.0);
    }

    [Fact]
    public void FrictionCoefficients_EnforceStaticGreaterThanEqualToKinetic()
    {
        var scenario = new InclinedSurface();

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

    [Theory]
    [InlineData(Math.PI / 2.0)]
    [InlineData(-Math.PI / 2.0)]
    [InlineData(2.0)]
    [InlineData(-2.0)]
    public void SurfaceInclination_RejectsAnglesAtOrBeyondRightAngle(double invalidAngle)
    {
        var scenario = new InclinedSurface { SurfaceInclination = Math.PI / 6.0 };
        scenario.SurfaceInclination = invalidAngle;

        // WeightX depends on SurfaceInclination; if invalid angle is rejected,
        // WeightX should still correspond to Math.PI / 6.0
        scenario.Update(0.01);
        double expectedWeightParallel = 5.0 * Constants.EarthGravitationalAcceleration * Math.Sin(Math.PI / 6.0);
        scenario.GetCurrentState().WeightX.Should().BeApproximately(-expectedWeightParallel, Precision);
    }
    #endregion

    #region Normal Force and Weight Decomposition
    [Fact]
    public void Update_NormalAndWeight_DecomposedCorrectlyOnIncline()
    {
        double mass = 5.0;
        double angle = Math.PI / 6.0; // 30 degrees
        double g = Constants.EarthGravitationalAcceleration;

        var scenario = new InclinedSurface
        {
            Mass = mass,
            SurfaceInclination = angle,
            AppliedForce = 0.0
        };

        scenario.Update(0.01);
        var state = scenario.GetCurrentState();

        double expectedWeightParallel = mass * g * Math.Sin(angle); // 24.5 N
        double expectedWeightPerp = mass * g * Math.Cos(angle);     // ~42.4352 N

        // Inclination > 0 pulls downhill (Xnegative)
        state.WeightX.Should().BeApproximately(-expectedWeightParallel, Precision);
        state.WeightY.Should().BeApproximately(-expectedWeightPerp, Precision);
        state.Normal.Should().BeApproximately(expectedWeightPerp, Precision);
        state.FNetY.Should().BeApproximately(0.0, Precision);
    }
    #endregion

    #region At Rest On Incline
    [Fact]
    public void Update_AtRest_HighFriction_HoldsBoxStationary()
    {
        // tan(15 deg) ~ 0.2679. With mu_s = 0.5, static friction holds the box.
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            InitialVelocity = 0.0,
            SurfaceInclination = Math.PI / 12.0, // 15 deg
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.4,
            AppliedForce = 0.0
        };

        scenario.Update(0.1);
        var state = scenario.GetCurrentState();

        state.Acceleration.Should().Be(0.0);
        state.Velocity.Should().Be(0.0);
        state.Position.Should().Be(0.0);
        state.FNetX.Should().Be(0.0);
        // Static friction should equal downhill weight and point uphill (+X)
        state.StaticFriction.Should().BeApproximately(-state.WeightX, Precision);
        state.KineticFriction.Should().Be(0.0);
    }

    [Fact]
    public void Update_AtRest_LowFriction_OvercomesStaticFrictionAndSlidesDownhill()
    {
        // 30 deg incline, mu_s = 0.2, mu_k = 0.2
        // W_parallel = 5 * 9.8 * sin(30) = 24.5 N downhill (-X)
        // Normal = 5 * 9.8 * cos(30) = 42.4352 N
        // f_max = 0.2 * 42.4352 = 8.487 N
        // Since W_parallel > f_max, box slides downhill (-X)
        // Friction fk = 8.487 N uphill (+X)
        // FnetX = -24.5 + 8.487 = -16.013 N
        // a = FnetX / m = -3.2026 m/s^2
        double mass = 5.0;
        double angle = Math.PI / 6.0;
        double mu = 0.2;

        var scenario = new InclinedSurface
        {
            Mass = mass,
            InitialVelocity = 0.0,
            SurfaceInclination = angle,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu,
            AppliedForce = 0.0
        };

        double expectedNormal = mass * Constants.EarthGravitationalAcceleration * Math.Cos(angle);
        double expectedFk = mu * expectedNormal;
        double expectedWeightX = -mass * Constants.EarthGravitationalAcceleration * Math.Sin(angle);
        double expectedFNetX = expectedWeightX + expectedFk;
        double expectedAcc = expectedFNetX / mass;

        scenario.Update(0.5);
        var state = scenario.GetCurrentState();

        state.FNetX.Should().BeApproximately(expectedFNetX, Precision);
        state.Acceleration.Should().BeApproximately(expectedAcc, Precision);
        state.Velocity.Should().BeLessThan(0.0);
        state.Position.Should().BeLessThan(0.0);
    }
    #endregion

    #region Moving On Incline
    [Fact]
    public void Update_MovingDownhill_AcceleratesDownhillCorrectly()
    {
        // Box moving downhill (v0 < 0)
        double mass = 5.0;
        double angle = Math.PI / 6.0;
        double mu = 0.2;
        double v0 = -4.0;
        double dt = 0.1;

        var scenario = new InclinedSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            SurfaceInclination = angle,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu,
            AppliedForce = 0.0
        };

        double expectedNormal = mass * Constants.EarthGravitationalAcceleration * Math.Cos(angle);
        double expectedFk = mu * expectedNormal;
        double expectedWeightX = -mass * Constants.EarthGravitationalAcceleration * Math.Sin(angle);
        double expectedFNetX = expectedWeightX + expectedFk; // fk is positive (opposes negative velocity)
        double expectedAcc = expectedFNetX / mass;

        scenario.Update(dt);
        var state = scenario.GetCurrentState();

        state.FNetX.Should().BeApproximately(expectedFNetX, Precision);
        state.Acceleration.Should().BeApproximately(expectedAcc, Precision);
        state.Velocity.Should().BeApproximately(v0 + expectedAcc * dt, Precision);
    }

    [Fact]
    public void Update_MovingUphill_DeceleratesDueToGravityAndFriction()
    {
        // Box moving uphill (v0 > 0)
        // Both gravity and friction act downhill (-X)
        double mass = 5.0;
        double angle = Math.PI / 6.0;
        double mu = 0.2;
        double v0 = 10.0;
        double dt = 0.1;

        var scenario = new InclinedSurface
        {
            Mass = mass,
            InitialVelocity = v0,
            SurfaceInclination = angle,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu,
            AppliedForce = 0.0
        };

        double expectedNormal = mass * Constants.EarthGravitationalAcceleration * Math.Cos(angle);
        double expectedFk = mu * expectedNormal;
        double expectedWeightX = -mass * Constants.EarthGravitationalAcceleration * Math.Sin(angle);
        double expectedFNetX = expectedWeightX - expectedFk; // friction opposes uphill velocity -> -X
        double expectedAcc = expectedFNetX / mass;

        scenario.Update(dt);
        var state = scenario.GetCurrentState();

        state.FNetX.Should().BeApproximately(expectedFNetX, Precision);
        state.Acceleration.Should().BeApproximately(expectedAcc, Precision);
        state.Velocity.Should().BeApproximately(v0 + expectedAcc * dt, Precision);
    }
    #endregion

    #region Applied Force and LiftOff
    [Fact]
    public void Update_AppliedForceUphill_OvercomesGravityAndFriction()
    {
        // Mass = 5kg, angle = 30 deg
        // Gravity downhill = 24.5 N
        // fk = 8.487 N downhill
        // Applied force = 50 N uphill (+X)
        // Net force = 50 - 24.5 - 8.487 = 17.013 N
        // a = 17.013 / 5 = 3.4026 m/s^2
        double mass = 5.0;
        double angle = Math.PI / 6.0;
        double mu = 0.2;

        var scenario = new InclinedSurface
        {
            Mass = mass,
            InitialVelocity = 1.0,
            SurfaceInclination = angle,
            StaticFrictionCoefficient = mu,
            KineticFrictionCoefficient = mu,
            AppliedForce = 50.0,
            AppliedForceAngle = 0.0
        };

        scenario.Update(0.1);
        var state = scenario.GetCurrentState();

        double expectedNormal = mass * Constants.EarthGravitationalAcceleration * Math.Cos(angle);
        double expectedFk = mu * expectedNormal;
        double expectedWeightX = -mass * Constants.EarthGravitationalAcceleration * Math.Sin(angle);
        double expectedFNetX = 50.0 + expectedWeightX - expectedFk;
        double expectedAcc = expectedFNetX / mass;

        state.FNetX.Should().BeApproximately(expectedFNetX, Precision);
        state.Acceleration.Should().BeApproximately(expectedAcc, Precision);
    }

    [Fact]
    public void Update_UpwardAppliedForceExceedingWeightPerpendicular_TriggersLiftOff()
    {
        // Weight perpendicular = 5 * 9.8 * cos(30) = 42.4352 N
        // Applied force pulling up away from surface at 90 deg = 50 N
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            SurfaceInclination = Math.PI / 6.0,
            AppliedForce = 50.0,
            AppliedForceAngle = Math.PI / 2.0
        };

        scenario.Update(0.1);
        var state = scenario.GetCurrentState();

        state.LiftOffWarning.Should().BeTrue();
    }
    #endregion

    #region Restart
    [Fact]
    public void Restart_ResetsSimulationState()
    {
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            InitialVelocity = -5.0,
            SurfaceInclination = Math.PI / 6.0,
            StaticFrictionCoefficient = 0.2,
            KineticFrictionCoefficient = 0.2
        };

        scenario.Update(0.5);
        scenario.Restart();

        var state = scenario.GetCurrentState();
        state.Time.Should().Be(0.0);
        state.Position.Should().Be(0.0);
        state.Velocity.Should().Be(-5.0);
        state.Acceleration.Should().Be(0.0);
        scenario.Segments.Should().BeEmpty();
    }
    #endregion

    #region Segment Boundary and Stopping / Reversal
    [Fact]
    public void Update_MovingUphill_StopsAndRemainsAtRest_WhenStaticFrictionSufficient()
    {
        // Mass = 5, angle = 15 deg (Math.PI / 12)
        // W_parallel = 5 * 9.8 * sin(15) ~ 12.68 N
        // N = 5 * 9.8 * cos(15) ~ 47.33 N
        // mu_s = 0.5 -> f_smax = 0.5 * 47.33 ~ 23.66 N > 12.68 N (holds!)
        // mu_k = 0.4 -> f_k = 0.4 * 47.33 ~ 18.93 N
        // Initial velocity = 2.0 m/s (uphill)
        // Deceleration uphill = (12.68 + 18.93) / 5 ~ 6.32 m/s^2
        // Stops in ~ 2.0 / 6.32 ~ 0.316 s
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            InitialVelocity = 2.0,
            SurfaceInclination = Math.PI / 12.0,
            StaticFrictionCoefficient = 0.5,
            KineticFrictionCoefficient = 0.4,
            AppliedForce = 0.0
        };

        // Run past stopping time
        double dt = 1.0 / 60.0;
        for (int i = 0; i < 60; i++)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();
        state.Velocity.Should().Be(0.0);
        state.Acceleration.Should().Be(0.0);
        state.Position.Should().BeGreaterThan(0.0);
        scenario.Segments.Should().HaveCount(1);
    }

    [Fact]
    public void Update_MovingUphill_StopsAndReversesDownhill_WhenStaticFrictionInsufficient()
    {
        // Mass = 5, angle = 30 deg (Math.PI / 6)
        // W_parallel = 24.5 N downhill
        // N = 42.4352 N
        // mu_s = 0.1 -> f_smax = 4.24 N < 24.5 N (cannot hold, will reverse!)
        // mu_k = 0.1 -> f_k = 4.24 N
        // Initial velocity = 4.0 m/s uphill
        // Stops and reverses downhill
        var scenario = new InclinedSurface
        {
            Mass = 5.0,
            InitialVelocity = 4.0,
            SurfaceInclination = Math.PI / 6.0,
            StaticFrictionCoefficient = 0.1,
            KineticFrictionCoefficient = 0.1,
            AppliedForce = 0.0
        };

        // Run past stop time into downhill slide
        double dt = 1.0 / 60.0;
        for (int i = 0; i < 120; i++)
        {
            scenario.Update(dt);
        }

        var state = scenario.GetCurrentState();
        scenario.Segments.Should().HaveCountGreaterThanOrEqualTo(1);
        // After reversing, velocity should be negative (sliding downhill)
        state.Velocity.Should().BeLessThan(0.0);
        state.Acceleration.Should().BeLessThan(0.0);
    }
    #endregion
}

