using FluentAssertions;
using PhysicsSolver.Formulas;

namespace PhysicsSolver.Tests;

public class MotionTests
{
    private const double Precision = 1e-10;

    // FinalVelocity
    [Theory]
    [InlineData(0.0, 0.0, 0.0, 0.0)]        // everything zero
    [InlineData(0.0, 0.0, 5.0, 0.0)]        // no initial velocity, no acceleration
    [InlineData(2.0, 2.0, 0.0, 2.0)]        // no time elapsed
    [InlineData(0.0, 2.0, 5.0, 10.0)]       // from rest
    [InlineData(2.0, 0.0, 5.0, 2.0)]        // no acceleration
    [InlineData(10.0, -2.0, 5.0, 0.0)]      // deceleration to zero
    [InlineData(2.0, -2.0, 5.0, -8.0)]      // deceleration past zero (reversal)
    [InlineData(-2.0, 2.0, 5.0, 8.0)]       // negative initial velocity + positive acceleration
    public void FinalVelocity_ReturnsCorrectResult(double initialV, double a, double t, double expected)
    {
        Motion.FinalVelocity(initialV, a, t).Should().BeApproximately(expected, Precision);
    }


    // FinalVelocitySquared
    [Theory]
    [InlineData(0.0, 0.0, 0.0, 0.0)]        // all zeros
    [InlineData(2.0, 2.0, 0.0, 4.0)]        // no displacement
    [InlineData(2.0, 0.0, 2.0, 4.0)]        // no acceleration
    [InlineData(0.0, 2.0, 5.0, 20.0)]       // from rest
    [InlineData(4.0, -2, 4.0, 0.0)]         // decelerating to rest
    public void FinalVelocitySquared_ReturnsCorrectResult(double initialV, double a, double x, double expected)
    {
        Motion.FinalVelocitySquared(initialV, a, x).Should().BeApproximately(expected, Precision);
    }


    // DisplacementUsingAcceleration
    [Theory]
    [InlineData(0.0, 0.0, 0.0, 0.0)]         // all zeros
    [InlineData(0.0, 5.0, 2.0, 25.0)]        // zero initial velocity
    [InlineData(2.0, 0.0, 5.0, 0.0)]         // zero time
    [InlineData(2.0, 5.0, 0.0, 10.0)]        // no acceleration
    [InlineData(2.0, 5.0, 3.0, 47.5)]
    public void DisplacementUsingAcceleration_ReturnsCorrectResult(double initialV, double t, double a, double expected)
    {
        Motion.DisplacementUsingAcceleration(initialV, t, a).Should().BeApproximately(expected, Precision);
    }



    // DisplacementUsingFinalVelocity
    [Theory]
    [InlineData(0.0, 0.0, 0.0, 0.0)]         // all zeros
    [InlineData(2.0, 10.0, 0.0, 0.0)]        // zero time
    [InlineData(0.0, 0.0, 10.0, 0.0)]        // no velocity at all
    [InlineData(2.0, 2.0, 5.0, 10.0)]        // constant velocity
    [InlineData(2.0, 10.0, 5.0, 30.0)]
    [InlineData(10.0, 2.0, 5.0, 30.0)]
    public void DisplacementUsingFinalVelocity_ReturnsCorrectResult(double initialV, double finalV, double t, double expected)
    {
        Motion.DisplacementUsingFinalVelocity(initialV, finalV, t).Should().BeApproximately(expected, Precision);
    }

}
