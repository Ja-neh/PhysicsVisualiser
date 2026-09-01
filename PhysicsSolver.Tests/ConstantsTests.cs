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
}

