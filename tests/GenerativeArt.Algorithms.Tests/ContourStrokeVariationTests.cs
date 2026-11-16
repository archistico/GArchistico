using System.Numerics;
using GenerativeArt.Algorithms.Contours;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class ContourStrokeVariationTests
{
    [Fact]
    public void PrecisePreservesVerticesAndHanddrawnIsSeeded()
    {
        Vector2[] points = [new(1, 1), new(4, 2), new(7, 5), new(10, 8)];
        Assert.Same(points, ContourStrokeVariation.Apply(points, false, "precise", 4, 1, 1, 42));
        Vector2[] first = ContourStrokeVariation.Apply(points, false, "handdrawn", 3, 1, 1, 42).ToArray();
        Vector2[] second = ContourStrokeVariation.Apply(points, false, "handdrawn", 3, 1, 1, 42).ToArray();
        Assert.Equal(first, second);
        Assert.False(points.SequenceEqual(first));
        Assert.False(first.SequenceEqual(ContourStrokeVariation.Apply(points, false, "handdrawn", 3, 1, 1, 2)));
        Assert.Same(points, ContourStrokeVariation.Apply(points, false, "handdrawn", 0, 1, 1, 42));
    }

    [Fact]
    public void SoftStrokeAddsVerticesAndKeepsOpenEndpointsAndClosedLoop()
    {
        Vector2[] open = [new(0, 0), new(1, 1), new(2, 0)];
        var smooth = ContourStrokeVariation.Apply(open, false, "soft", 0, 1, 1, 42);
        Assert.Equal(6, smooth.Count);
        Assert.Equal(open[0], smooth[0]);
        Assert.Equal(open[^1], smooth[^1]);
        Vector2[] closed = [new(0, 0), new(2, 0), new(2, 2), new(0, 0)];
        var loop = ContourStrokeVariation.Apply(closed, true, "soft", 1, 1, 1, 42);
        Assert.Equal(loop[0], loop[^1]);
    }

    [Fact]
    public void InvalidStrokeOptionsAreRejected()
    {
        Vector2[] points = [Vector2.Zero, Vector2.One];
        Assert.Throws<ArgumentException>(() => ContourStrokeVariation.Apply(points, false, "unknown", 1, 1, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ContourStrokeVariation.Apply(points, false, "soft", -1, 1, 1, 1));
    }
}
