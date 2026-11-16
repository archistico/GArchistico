using System.Numerics;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class BezierTests
{
    [Fact]
    public void QuadraticCurvePassesThroughEndpointsAndMidpoint()
    {
        Vector2 a = new(0, 0), b = new(5, 10), c = new(10, 0);
        Assert.Equal(a, Geometry2D.QuadraticBezier(a, b, c, 0));
        Assert.Equal(c, Geometry2D.QuadraticBezier(a, b, c, 1));
        Assert.Equal(new Vector2(5, 5), Geometry2D.QuadraticBezier(a, b, c, .5f));
    }

    [Fact]
    public void CubicCurveHasCorrectEndpointsAndTangents()
    {
        Vector2 a = new(0, 0), b = new(1, 2), c = new(2, 2), d = new(3, 0);
        Assert.Equal(a, Geometry2D.CubicBezier(a, b, c, d, 0f));
        Assert.Equal(d, Geometry2D.CubicBezier(a, b, c, d, 1f));
        Assert.Equal(new Vector2(1.5f, 1.5f), Geometry2D.CubicBezier(a, b, c, d, .5f));
        Assert.Equal(3f * (b - a), Geometry2D.CubicBezierTangent(a, b, c, d, 0f));
        Assert.Equal(3f * (d - c), Geometry2D.CubicBezierTangent(a, b, c, d, 1f));
    }

    [Fact]
    public void DegenerateSegmentUsesPointDistance()
    {
        Assert.Equal(5f, Geometry2D.DistanceToSegment(new Vector2(3, 4), Vector2.Zero, Vector2.Zero));
    }

    [Fact]
    public void SegmentClampsProjectionToEnds()
    {
        Assert.Equal(2f, Geometry2D.DistanceToSegment(new Vector2(-2, 0), Vector2.Zero, new Vector2(10, 0)));
        Assert.Equal(3f, Geometry2D.DistanceToSegment(new Vector2(5, 3), Vector2.Zero, new Vector2(10, 0)));
    }

    [Fact]
    public void InvalidControlPointsOrParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry2D.QuadraticBezier(Vector2.Zero, Vector2.Zero, Vector2.One, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry2D.CubicBezier(Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, -0.1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry2D.DistanceToSegment(new Vector2(float.PositiveInfinity, 0), Vector2.Zero, Vector2.Zero));
    }
}
