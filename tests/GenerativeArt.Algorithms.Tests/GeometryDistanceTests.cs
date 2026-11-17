using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class GeometryDistanceTests
{
    [Theory]
    [InlineData(0d, 0d, 0d)]
    [InlineData(3d, 4d, 5d)]
    [InlineData(-5d, 12d, 13d)]
    [InlineData(0d, -7d, 7d)]
    public void HypotenuseMatchesEuclideanDistance(double x, double y, double expected)
    {
        Assert.Equal(expected, GeometryDistance.Hypotenuse(x, y), 12);
        Assert.Equal(expected, GeometryDistance.Hypotenuse(y, x), 12);
    }

    [Fact]
    public void HypotenuseAvoidsIntermediateOverflowAndUnderflow()
    {
        Assert.Equal(1e200, GeometryDistance.Hypotenuse(1e200, 0d));
        Assert.Equal(1e200, GeometryDistance.Hypotenuse(1e200, 1e-200));
        double tiny = GeometryDistance.Hypotenuse(3e-200, 4e-200);
        Assert.True(double.IsFinite(tiny));
        Assert.InRange(tiny, 4.99e-200, 5.01e-200);
    }

    [Fact]
    public void HypotenuseHandlesNonFiniteInputsExplicitly()
    {
        Assert.True(double.IsNaN(GeometryDistance.Hypotenuse(double.NaN, 3d)));
        Assert.Equal(double.PositiveInfinity,
            GeometryDistance.Hypotenuse(double.PositiveInfinity, 3d));
        Assert.Equal(double.PositiveInfinity,
            GeometryDistance.Hypotenuse(double.NaN, double.PositiveInfinity));
    }
}
