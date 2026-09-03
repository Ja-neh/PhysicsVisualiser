using FluentAssertions;
using PhysicsSolver.Quantities;

namespace PhysicsSolver.Tests;

public class ForceTests
{

    #region Force (General / Mutable Direction)
    [Fact]
    public void Force_DefaultConstructor_CreatesZeroMagnitudeXPositive()
    {
        var force = new Force();

        force.Magnitude.Should().Be(0.0);
        force.Direction.Should().Be(DirectionXY.Xpositive);
        force.SignedMagnitude.Should().Be(0.0);
    }

    [Theory]
    [InlineData(15.0, "Xpositive", 15.0, 15.0)]
    [InlineData(15.0, "Xnegative", 15.0, -15.0)]
    [InlineData(25.5, "Ypositive", 25.5, 25.5)]
    [InlineData(25.5, "Ynegative", 25.5, -25.5)]
    [InlineData(0.0, "Xpositive", 0.0, 0.0)]
    [InlineData(0.0, "Ynegative", 0.0, 0.0)]
    [InlineData(-10.0, "Xpositive", 10.0, 10.0)] 
    [InlineData(-10.0, "Xnegative", 10.0, -10.0)]
    public void Force_ParameterizedConstructor_SetsMagnitudeAndDirection(double inputMagnitude, string directionName, 
                                                                         double expectedMagnitude, double expectedSignedMagnitude)
    {
        var direction = Enum.Parse<DirectionXY>(directionName);
        var force = new Force(inputMagnitude, direction);

        force.Direction.Should().Be(direction);
        force.Magnitude.Should().Be(expectedMagnitude);
        force.SignedMagnitude.Should().Be(expectedSignedMagnitude);
    }

    [Theory]
    [InlineData(20.0, 20.0)]
    [InlineData(-35.0, 35.0)]
    [InlineData(0.0, 0.0)]
    public void Force_MagnitudeSetter_AlwaysStoresAbsoluteValue(double setValue, double expectedMagnitude)
    {
        var force = new Force(5.0, DirectionXY.Xpositive);
        force.Magnitude = setValue;

        force.Magnitude.Should().Be(expectedMagnitude);
    }

    [Fact]
    public void Force_DirectionSetter_UpdatesDirectionAndSignedMagnitude()
    {
        var force = new Force(10.0, DirectionXY.Xpositive);
        force.SignedMagnitude.Should().Be(10.0);

        force.Direction = DirectionXY.Xnegative;
        force.Direction.Should().Be(DirectionXY.Xnegative);
        force.SignedMagnitude.Should().Be(-10.0);

        force.Direction = DirectionXY.Ypositive;
        force.Direction.Should().Be(DirectionXY.Ypositive);
        force.SignedMagnitude.Should().Be(10.0);

        force.Direction = DirectionXY.Ynegative;
        force.Direction.Should().Be(DirectionXY.Ynegative);
        force.SignedMagnitude.Should().Be(-10.0);
    }
    #endregion

    #region Locked Directional Forces
    [Theory]
    [InlineData(12.0, 12.0, 12.0)]
    [InlineData(-12.0, 12.0, 12.0)]
    [InlineData(0.0, 0.0, 0.0)]
    public void ForceXPositive_CreatesWithXPositiveDirection(double input, double expectedMag, double expectedSigned)
    {
        var force = new ForceXPositive(input);

        force.Direction.Should().Be(DirectionXY.Xpositive);
        force.Magnitude.Should().Be(expectedMag);
        force.SignedMagnitude.Should().Be(expectedSigned);
    }

    [Theory]
    [InlineData(12.0, 12.0, -12.0)]
    [InlineData(-12.0, 12.0, -12.0)]
    [InlineData(0.0, 0.0, 0.0)]
    public void ForceXNegative_CreatesWithXNegativeDirection(double input, double expectedMag, double expectedSigned)
    {
        var force = new ForceXNegative(input);

        force.Direction.Should().Be(DirectionXY.Xnegative);
        force.Magnitude.Should().Be(expectedMag);
        force.SignedMagnitude.Should().Be(expectedSigned);
    }

    [Theory]
    [InlineData(8.5, 8.5, 8.5)]
    [InlineData(-8.5, 8.5, 8.5)]
    [InlineData(0.0, 0.0, 0.0)]
    public void ForceYPositive_CreatesWithYPositiveDirection(double input, double expectedMag, double expectedSigned)
    {
        var force = new ForceYPositive(input);

        force.Direction.Should().Be(DirectionXY.Ypositive);
        force.Magnitude.Should().Be(expectedMag);
        force.SignedMagnitude.Should().Be(expectedSigned);
    }

    [Theory]
    [InlineData(8.5, 8.5, -8.5)]
    [InlineData(-8.5, 8.5, -8.5)]
    [InlineData(0.0, 0.0, 0.0)]
    public void ForceYNegative_CreatesWithYNegativeDirection(double input, double expectedMag, double expectedSigned)
    {
        var force = new ForceYNegative(input);

        force.Direction.Should().Be(DirectionXY.Ynegative);
        force.Magnitude.Should().Be(expectedMag);
        force.SignedMagnitude.Should().Be(expectedSigned);
    }

    [Fact]
    public void SpecializedForces_MagnitudeSetter_UpdatesMagnitudeAndSignedMagnitude()
    {
        var forceXPos = new ForceXPositive(10.0);
        var forceXNeg = new ForceXNegative(10.0);

        forceXPos.Magnitude = 25.0;
        forceXPos.Magnitude.Should().Be(25.0);
        forceXPos.SignedMagnitude.Should().Be(25.0);

        forceXNeg.Magnitude = -30.0;
        forceXNeg.Magnitude.Should().Be(30.0);
        forceXNeg.SignedMagnitude.Should().Be(-30.0);
    }
    #endregion

    #region DirectionXY Extensions
    [Theory]
    [InlineData("Xpositive", "Xnegative")]
    [InlineData("Xnegative", "Xpositive")]
    [InlineData("Ypositive", "Ynegative")]
    [InlineData("Ynegative", "Ypositive")]
    public void DirectionXYExtensions_Negate_FlipsDirectionCorrectly(string inputName, string expectedName)
    {
        var input = Enum.Parse<DirectionXY>(inputName);
        var expected = Enum.Parse<DirectionXY>(expectedName);

        input.Negate().Should().Be(expected);
    }

    [Fact]
    public void DirectionXYExtensions_DoubleNegate_ReturnsOriginalDirection()
    {
        DirectionXY.Xpositive.Negate().Negate().Should().Be(DirectionXY.Xpositive);
        DirectionXY.Xnegative.Negate().Negate().Should().Be(DirectionXY.Xnegative);
        DirectionXY.Ypositive.Negate().Negate().Should().Be(DirectionXY.Ypositive);
        DirectionXY.Ynegative.Negate().Negate().Should().Be(DirectionXY.Ynegative);
    }

    [Fact]
    public void DirectionXYExtensions_Negate_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        var invalid = (DirectionXY)999;
        var act = () => invalid.Negate();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
    #endregion
}
