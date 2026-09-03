using PhysicsSolver;

namespace PhysicsSolver.Tests;

public class ConstantsTests
{
    [Fact]
    public void UniversalGravitationalConstant_IsCorrect()
    {
        Assert.Equal(6.67e-11, Constants.UniversalGravitationalConstant, precision: 13);
    }

    [Fact]
    public void EarthMass_IsCorrect()
    {
        Assert.Equal(5.98e24, Constants.EarthMass, precision: 0);
    }

    [Fact]
    public void EarthRadius_IsCorrect()
    {
        Assert.Equal(6.38e6, Constants.EarthRadius, precision: 0);
    }

    [Fact]
    public void EarthGravitationalAcceleration_IsCorrect()
    {
        Assert.Equal(9.8, Constants.EarthGravitationalAcceleration, precision: 1);
    }

    [Fact]
    public void MoonMass_IsCorrect()
    {
        Assert.Equal(7.35e22, Constants.MoonMass, precision: 0);
    }

    [Fact]
    public void MoonRadius_IsCorrect()
    {
        Assert.Equal(1.74e6, Constants.MoonRadius, precision: 0);
    }

    [Fact]
    public void MoonGravitationalAcceleration_IsCorrect()
    {
        Assert.Equal(1.62, Constants.MoonGravitationalAcceleration, precision: 2);
    }

    [Fact]
    public void MarsMass_IsCorrect()
    {
        Assert.Equal(6.42e23, Constants.MarsMass, precision: 0);
    }

    [Fact]
    public void MarsRadius_IsCorrect()
    {
        Assert.Equal(3.39e6, Constants.MarsRadius, precision: 0);
    }

    [Fact]
    public void MarsGravitationalAcceleration_IsCorrect()
    {
        Assert.Equal(3.71, Constants.MarsGravitationalAcceleration, precision: 2);
    }
}

