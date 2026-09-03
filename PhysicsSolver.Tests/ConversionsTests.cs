using FluentAssertions;
using PhysicsSolver.Formulas;

namespace PhysicsSolver.Tests;

public class ConversionsTests
{
    private const double Precision = 1e-10;

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(30.0, Math.PI / 6.0)]
    [InlineData(45.0, Math.PI / 4.0)]
    [InlineData(90.0, Math.PI / 2.0)]
    [InlineData(180.0, Math.PI)]
    [InlineData(360.0, 2.0 * Math.PI)]
    public void DegreesToRadians_StandardAngles_ReturnsCorrectRadians(double degrees, double expectedRadians)
    {
        Conversions.DegreesToRadians(degrees).Should().BeApproximately(expectedRadians, Precision);
    }

    [Fact]
    public void DegreesToRadians_NegativeAngle_ReturnsNegativeRadians()
    {
        double result = Conversions.DegreesToRadians(-90.0);
        result.Should().BeApproximately(-Math.PI / 2.0, Precision);
    }

    [Fact]
    public void DegreesToRadians_FullRotationReturnsToSameSine()
    {
        // sin(x) should equal sin(x + 360)
        double angle = 37.0;
        double rad1 = Conversions.DegreesToRadians(angle);
        double rad2 = Conversions.DegreesToRadians(angle + 360.0);

        Math.Sin(rad1).Should().BeApproximately(Math.Sin(rad2), 1e-12);
    }

    [Fact]
    public void DegreesToRadians_LargeAngle_ReturnsCorrectResult()
    {
        // 720 = 4 PI
        Conversions.DegreesToRadians(720.0).Should().BeApproximately(4.0 * Math.PI, Precision);
    }
}
