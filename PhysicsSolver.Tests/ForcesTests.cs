using FluentAssertions;
using PhysicsSolver.Formulas;

namespace PhysicsSolver.Tests;

public class ForcesTests
{
    private const double Precision = 1e-10;

    #region FNet
    [Theory]
    [InlineData(0.0, 0.0, 0.0)]
    [InlineData(5.0, 2.0, 10.0)]
    [InlineData(5.0, 0.0, 0.0)]
    [InlineData(0.0, 5.0, 0.0)]
    [InlineData(5.0, -3.0, -15.0)]
    public void FNet_ReturnsCorrectProduct(double mass, double acceleration, double expected)
    {
        Forces.FNet(mass, acceleration).Should().BeApproximately(expected, Precision);
    }
    #endregion

    #region Momentum
    [Theory]
    [InlineData(10.0, 3.0, 30.0)]
    [InlineData(0.0, 5.0, 0.0)]
    [InlineData(5.0, 0.0, 0.0)]
    [InlineData(2.0, -4.0, -8.0)]
    public void Momentum_ReturnsCorrectProduct(double mass, double velocity, double expected)
    {
        Forces.Momentum(mass, velocity).Should().BeApproximately(expected, Precision);
    }
    #endregion

    #region Friction
    [Theory]
    [InlineData(0.2, 40.0, 8)]
    [InlineData(0.0, 40.0, 0.0)]
    [InlineData(0.2, 0.0, 0.0)]
    [InlineData(1.0, 50.0, 50.0)]
    public void Friction_ReturnsCorrectProduct(double coefficient, double normal, double expected)
    {
        Forces.Friction(coefficient, normal).Should().BeApproximately(expected, Precision);
    }
    #endregion

    #region Impulse (two overloads)
    [Theory]
    [InlineData(25.0, 2.0, 50.0)]
    [InlineData(0.0, 5.0, 0.0)]
    [InlineData(10.0, 0.0, 0.0)]
    [InlineData(-15.0, 3.0, -45.0)]
    public void Impulse_FNetDeltaTime_ReturnsCorrectProduct(double fnet, double deltaTime, double expected)
    {
        Forces.Impulse(fnet, deltaTime).Should().BeApproximately(expected, Precision);
    }

    [Theory]
    [InlineData(5.0, 2.0, 8.0, 30.0)]      // 5 * (8 - 2) = 30
    [InlineData(5.0, 10.0, 4.0, -30.0)]    // 5 * (4 - 10) = -30
    [InlineData(3.0, 5.0, 5.0, 0.0)]       // no change in velocity
    [InlineData(0.0, 3.0, 7.0, 0.0)]       // zero mass
    public void Impulse_MassVelocities_ReturnsCorrectResult(double mass, double vi, double vf, double expected)
    {
        Forces.Impulse(mass, vi, vf).Should().BeApproximately(expected, Precision);
    }
    #endregion

    #region GravitationalForce
    [Fact]
    public void GravitationalForce_EarthMoonApprox_ReturnsReasonableValue()
    {
        double m1 = Constants.EarthMass;   // Earth
        double m2 = 7.35e22;   // Moon
        double r = 3.844e8;    // Earth-Moon distance

        double result = Forces.GravitationalForce(m1, m2, r);

        // Expected is 1.98e20 N
        result.Should().BeApproximately(1.98e20, 0.02e20);
    }

    [Fact]
    public void GravitationalForce_EqualMassesUnitRadius_ReturnsG()
    {
        double result = Forces.GravitationalForce(1.0, 1.0, 1.0);
        result.Should().BeApproximately(Constants.UniversalGravitationalConstant, Precision);
    }

    [Fact]
    public void GravitationalForce_ZeroMass_ReturnsZero()
    {
        Forces.GravitationalForce(0.0, Constants.EarthMass, Constants.EarthRadius).Should().Be(0.0);
    }

    [Fact]
    public void GravitationalForce_DoubleRadius_QuartersForce()
    {
        double f1 = Forces.GravitationalForce(1e10, 1e10, 1000.0);
        double f2 = Forces.GravitationalForce(1e10, 1e10, 2000.0);

        f2.Should().BeApproximately(f1 / 4.0, Precision);
    }
    #endregion

    #region GravitationalAcceleration
    [Fact]
    public void GravitationalAcceleration_EarthSurface_ReturnsApprox9point8()
    {
        double result = Forces.GravitationalAcceleration(Constants.EarthMass, Constants.EarthRadius);
        result.Should().BeApproximately(Constants.EarthGravitationalAcceleration, 0.1);
    }

    [Fact]
    public void GravitationalAcceleration_UnitValues_ReturnsG()
    {
        double result = Forces.GravitationalAcceleration(1.0, 1.0);
        result.Should().BeApproximately(Constants.UniversalGravitationalConstant, Precision);
    }

    [Fact]
    public void GravitationalAcceleration_ZeroMass_ReturnsZero()
    {
        Forces.GravitationalAcceleration(0.0, Constants.EarthRadius).Should().Be(0.0);
    }
    #endregion

    #region WeightParallel / WeightPerpendicular
    [Fact]
    public void WeightParallel_DefaultsToEarthGravitationalAcceleration()
    {
        double mass = 6.0;
        double angle = Math.PI / 6.0;

        double withDefault = Forces.WeightParallel(mass, angle);
        double withExplicitEarth = Forces.WeightParallel(mass, angle, Constants.EarthGravitationalAcceleration);

        withDefault.Should().BeApproximately(withExplicitEarth, Precision);
    }

    [Fact]
    public void WeightPerpendicular_DefaultsToEarthGravitationalAcceleration()
    {
        double mass = 6.0;
        double angle = Math.PI / 6.0;

        double withDefault = Forces.WeightPerpendicular(mass, angle);
        double withExplicitEarth = Forces.WeightPerpendicular(mass, angle, Constants.EarthGravitationalAcceleration);

        withDefault.Should().BeApproximately(withExplicitEarth, Precision);
    }

    [Fact]
    public void WeightParallel_ZeroAngle_ReturnsZero()
    {
        // sin(0) = 0   - no parallel component on flat surface
        Forces.WeightParallel(10.0, 0.0).Should().BeApproximately(0.0, Precision);
        Forces.WeightParallel(10.0, 0.0, Constants.MoonGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
        Forces.WeightParallel(10.0, 0.0, Constants.MarsGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
    }

    [Fact]
    public void WeightPerpendicular_ZeroAngle_ReturnsFullWeight()
    {
        // cos(0) = 1   - full weight perpendicular on flat surface
        double mass = 10.0;
        double expectedEarth = mass * Constants.EarthGravitationalAcceleration;
        double expectedMoon = mass * Constants.MoonGravitationalAcceleration;
        double expectedMars = mass * Constants.MarsGravitationalAcceleration;

        Forces.WeightPerpendicular(mass, 0.0).Should().BeApproximately(expectedEarth, Precision);
        Forces.WeightPerpendicular(mass, 0.0, Constants.MoonGravitationalAcceleration).Should().BeApproximately(expectedMoon, Precision);
        Forces.WeightPerpendicular(mass, 0.0, Constants.MarsGravitationalAcceleration).Should().BeApproximately(expectedMars, Precision);
    }

    [Fact]
    public void WeightParallel_90Degrees_ReturnsFullWeight()
    {
        // sin(PI/2) = 1    - vertical surface, all weight is parallel
        double mass = 5.0;
        double angle = Math.PI / 2.0;

        Forces.WeightParallel(mass, angle).Should().BeApproximately(mass * Constants.EarthGravitationalAcceleration, Precision);
        Forces.WeightParallel(mass, angle, Constants.MoonGravitationalAcceleration).Should().BeApproximately(mass * Constants.MoonGravitationalAcceleration, Precision);
        Forces.WeightParallel(mass, angle, Constants.MarsGravitationalAcceleration).Should().BeApproximately(mass * Constants.MarsGravitationalAcceleration, Precision);
    }

    [Fact]
    public void WeightPerpendicular_90Degrees_ReturnsZero()
    {
        // cos(PI/2) = 0
        double angle = Math.PI / 2.0;

        Forces.WeightPerpendicular(5.0, angle).Should().BeApproximately(0.0, Precision);
        Forces.WeightPerpendicular(5.0, angle, Constants.MoonGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
        Forces.WeightPerpendicular(5.0, angle, Constants.MarsGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
    }

    [Theory]
    [InlineData(Constants.EarthGravitationalAcceleration)]
    [InlineData(Constants.MoonGravitationalAcceleration)]
    [InlineData(Constants.MarsGravitationalAcceleration)]
    [InlineData(5.0)]   // Custom
    public void WeightComponents_45Degrees_AreEqualForAnyGravitationalAcceleration(double g)
    {
        // At 45 deg, sin = cos, so parallel and perpendicular components should always be equal
        double angle = Math.PI / 4.0;
        double mass = 8.0;

        double parallel = Forces.WeightParallel(mass, angle, g);
        double perpendicular = Forces.WeightPerpendicular(mass, angle, g);

        parallel.Should().BeApproximately(perpendicular, Precision);
    }

    [Theory]
    [InlineData(Constants.EarthGravitationalAcceleration)]
    [InlineData(Constants.MoonGravitationalAcceleration)]
    [InlineData(Constants.MarsGravitationalAcceleration)]
    [InlineData(3.0)]   // Custom
    public void WeightComponents_Pythagorean_SumOfSquaresEqualsWeightSquaredForAnyGravity(double g)
    {
        // W_parallel^2 + W_perp^2 = (m*g)^2
        double mass = 12.0;
        double angle = Math.PI / 6.0;

        double parallel = Forces.WeightParallel(mass, angle, g);
        double perpendicular = Forces.WeightPerpendicular(mass, angle, g);
        double totalWeight = mass * g;

        double sumOfSquares = parallel * parallel + perpendicular * perpendicular;
        sumOfSquares.Should().BeApproximately(totalWeight * totalWeight, Precision);
    }

    [Theory]
    [InlineData(10.0, Math.PI / 6.0, Constants.EarthGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, Constants.MoonGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, Constants.MarsGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, 0.0)]   // Zero gravity
    public void WeightParallel_ReturnsExpectedValueAcrossGravitationalAccelerations(double mass, double angle, double g)
    {
        double expected = mass * g * Math.Sin(angle);
        Forces.WeightParallel(mass, angle, g).Should().BeApproximately(expected, Precision);
    }

    [Theory]
    [InlineData(10.0, Math.PI / 6.0, Constants.EarthGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, Constants.MoonGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, Constants.MarsGravitationalAcceleration)]
    [InlineData(10.0, Math.PI / 6.0, 0.0)]   // Zero gravity
    public void WeightPerpendicular_ReturnsExpectedValueAcrossGravitationalAccelerations(double mass, double angle, double g)
    {
        double expected = mass * g * Math.Cos(angle);
        Forces.WeightPerpendicular(mass, angle, g).Should().BeApproximately(expected, Precision);
    }


    [Fact]
    public void WeightParallel_ZeroMass_ReturnsZero()
    {
        Forces.WeightParallel(0.0, Math.PI / 4.0).Should().BeApproximately(0.0, Precision);
        Forces.WeightParallel(0.0, Math.PI / 4.0, Constants.MoonGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
        Forces.WeightParallel(0.0, Math.PI / 4.0, Constants.MarsGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
    }

    [Fact]
    public void WeightPerpendicular_ZeroMass_ReturnsZero()
    {
        Forces.WeightPerpendicular(0.0, Math.PI / 4.0).Should().BeApproximately(0.0, Precision);
        Forces.WeightPerpendicular(0.0, Math.PI / 4.0, Constants.MoonGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
        Forces.WeightPerpendicular(0.0, Math.PI / 4.0, Constants.MarsGravitationalAcceleration).Should().BeApproximately(0.0, Precision);
    }
    #endregion

    #region ForceAdjacent / ForceOpposite
    [Theory]
    [InlineData(100.0, 0.0, 100.0)]           // cos(0) = 1     - full force
    [InlineData(100.0, Math.PI / 2.0, 0.0)]   // cos(90) = 0
    public void ForceAdjacent_ReturnsCorrectComponent(double force, double angle, double expected)
    {
        Forces.ForceAdjacent(force, angle).Should().BeApproximately(expected, Precision);
    }

    [Theory]
    [InlineData(100.0, 0.0, 0.0)]             // sin(0) = 0
    [InlineData(100.0, Math.PI / 2.0, 100.0)] // sin(90) = 1   - full force
    public void ForceOpposite_ReturnsCorrectComponent(double force, double angle, double expected)
    {
        Forces.ForceOpposite(force, angle).Should().BeApproximately(expected, Precision);
    }

    [Fact]
    public void ForceComponents_Pythagorean_SumOfSquaresEqualsForceSquared()
    {
        double force = 75.0;
        double angle = Math.PI / 6.0;

        double adjacent = Forces.ForceAdjacent(force, angle);
        double opposite = Forces.ForceOpposite(force, angle);

        double sumOfSquares = adjacent * adjacent + opposite * opposite;
        sumOfSquares.Should().BeApproximately(force * force, Precision);
    }

    [Fact]
    public void ForceAdjacent_ZeroForce_ReturnsZero()
    {
        Forces.ForceAdjacent(0.0, Math.PI / 4.0).Should().BeApproximately(0.0, Precision);
    }

    [Fact]
    public void ForceOpposite_ZeroForce_ReturnsZero()
    {
        Forces.ForceOpposite(0.0, Math.PI / 4.0).Should().BeApproximately(0.0, Precision);
    }
    #endregion
}
