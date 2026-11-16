using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class SignedDistance2DTests
{
    [Fact]
    public void CircleAndBoxHaveSignedDistancesAtKnownPoints()
    {
        Assert.Equal(-2f, SignedDistance2D.Circle(Vector2.Zero, 2f), 5);
        Assert.Equal(0f, SignedDistance2D.Circle(new Vector2(2, 0), 2f), 5);
        Assert.Equal(1f, SignedDistance2D.Circle(new Vector2(3, 0), 2f), 5);
        Assert.Equal(-1f, SignedDistance2D.Box(Vector2.Zero, new Vector2(2, 1)), 5);
        Assert.Equal(1f, SignedDistance2D.Box(new Vector2(3, 0), new Vector2(2, 1)), 5);
        Assert.Equal(MathF.Sqrt(2), SignedDistance2D.Box(new Vector2(3, 2), new Vector2(2, 1)), 5);
    }

    [Fact]
    public void RoundedBoxPreservesBorderAndRotation()
    {
        Assert.Equal(0f, SignedDistance2D.RoundedBox(new Vector2(2, 0), new Vector2(2, 1), .3f), 5);
        Vector2 rotated = SignedDistance2D.Rotate(Vector2.UnitX, MathF.PI / 2);
        Assert.True(Vector2.Distance(rotated, Vector2.UnitY) < 1e-5f);
        Assert.True(Vector2.Distance(SignedDistance2D.Rotate(rotated, -MathF.PI / 2), Vector2.UnitX) < 1e-5f);
    }

    [Fact]
    public void CsgUsesSignedDistanceConventionsAndSmoothUnion()
    {
        Assert.Equal(-2f, SignedDistance2D.Union(-2, 1));
        Assert.Equal(1f, SignedDistance2D.Intersection(-2, 1));
        Assert.Equal(1f, SignedDistance2D.Difference(-2, -1));
        Assert.Equal(-.05f, SignedDistance2D.SmoothUnion(0, 0, .2f), 5);
        Assert.Equal(-1f, SignedDistance2D.SmoothUnion(-1, 1, .2f), 5);
    }

    [Fact]
    public void InvalidPrimitiveDimensionsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SignedDistance2D.Circle(Vector2.Zero, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SignedDistance2D.Box(Vector2.Zero, new Vector2(0, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => SignedDistance2D.RoundedBox(Vector2.Zero, new Vector2(1, 1), 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SignedDistance2D.Rotate(Vector2.Zero, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => SignedDistance2D.SmoothUnion(0, 0, 0));
    }
}
