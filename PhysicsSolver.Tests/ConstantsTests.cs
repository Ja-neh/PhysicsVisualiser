using FluentAssertions;
using PhysicsSolver;

namespace PhysicsSolver.Tests;

public class ConstantsTests
{
    [Fact]
    public void UniversalGravitationalConstant_IsCorrect()
    {
        Constants.UniversalGravitationalConstant.Should().Be(6.67e-11);
    }

    [Fact]
    public void EarthMass_IsCorrect()
    {
        Constants.EarthMass.Should().Be(5.98e24);
    }

    [Fact]
    public void EarthRadius_IsCorrect()
    {
        Constants.EarthRadius.Should().Be(6.38e6);
    }

    [Fact]
    public void EarthGravitationalAcceleration_IsCorrect()
    {
        Constants.EarthGravitationalAcceleration.Should().Be(9.8);
    }

    [Fact]
    public void MoonMass_IsCorrect()
    {
        Constants.MoonMass.Should().Be(7.35e22);
    }

    [Fact]
    public void MoonRadius_IsCorrect()
    {
        Constants.MoonRadius.Should().Be(1.74e6);
    }

    [Fact]
    public void MoonGravitationalAcceleration_IsCorrect()
    {
        Constants.MoonGravitationalAcceleration.Should().Be(1.62);
    }

    [Fact]
    public void MarsMass_IsCorrect()
    {
        Constants.MarsMass.Should().Be(6.42e23);
    }

    [Fact]
    public void MarsRadius_IsCorrect()
    {
        Constants.MarsRadius.Should().Be(3.39e6);
    }

    [Fact]
    public void MarsGravitationalAcceleration_IsCorrect()
    {
        Constants.MarsGravitationalAcceleration.Should().Be(3.71);
    }
}

