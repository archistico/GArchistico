using System.Numerics;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class GeometryTests
{
    [Fact]
    public void RotationByQuarterTurn()
    {
        var result = Geometry2D.Rotate(Vector2.UnitX, MathF.PI / 2);
        Assert.InRange(MathF.Abs(result.X), 0f, 0.000001f);
        Assert.InRange(MathF.Abs(result.Y - 1), 0f, 0.000001f);
    }

    [Fact]
    public void PointOnCircleHasRequestedRadius()
    {
        Vector2 center = new(3, 4);
        Vector2 p = Geometry2D.PointOnCircle(center, 10, 0.5f);
        Assert.InRange(MathF.Abs(Vector2.Distance(center, p) - 10), 0f, 0.00001f);
    }

    [Fact]
    public void InvalidAngleAndRadiusAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry2D.Rotate(Vector2.Zero, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry2D.PointOnCircle(Vector2.Zero, -1, 0));
    }
}
