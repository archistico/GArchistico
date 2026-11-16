using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class VectorGrid2DTests
{
    [Fact]
    public void BilinearInterpolationAtCenterAndCorners()
    {
        var grid = new VectorGrid2D(new FlowBounds(0, 0, 10, 10), 2, 2,
            new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(0, 20), new Vector2(10, 20) });
        Assert.Equal(new Vector2(5, 10), grid.Sample(5, 5));
        Assert.Equal(new Vector2(0, 0), grid.Sample(0, 0));
        Assert.Equal(new Vector2(10, 20), grid.Sample(10, 10));
        Assert.Equal(new Vector2(10, 0), grid.Sample(10, 0));
    }

    [Fact]
    public void OutsideDomainOrNonFiniteInputReturnsZero()
    {
        var grid = VectorGrid2D.FromField(new FlowBounds(0, 0, 10, 10), 3, 3,
            VectorFields.Constant(Vector2.UnitX));
        Assert.Equal(Vector2.Zero, grid.Sample(-1, 4));
        Assert.Equal(Vector2.Zero, grid.Sample(11, 4));
        Assert.Equal(Vector2.Zero, grid.Sample(float.NaN, 2));
        Assert.Equal(Vector2.Zero, grid.Sample(2, float.PositiveInfinity));
    }

    [Fact]
    public void ConstructorCopiesValuesAndRejectsInvalidNodes()
    {
        var values = new[] { Vector2.UnitX, Vector2.UnitY, Vector2.Zero, Vector2.One };
        var grid = new VectorGrid2D(new FlowBounds(-1, -1, 1, 1), 2, 2, values);
        values[0] = new Vector2(400, 400);
        Assert.Equal(Vector2.UnitX, grid.Sample(-1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VectorGrid2D(
            new FlowBounds(0, 0, 10, 10), 2, 2,
            new[] { Vector2.UnitX, Vector2.UnitY, Vector2.Zero, new Vector2(float.NaN, 1) }));
    }

    [Fact]
    public void InvalidGridSizesAndCountsAreRejected()
    {
        var bounds = new FlowBounds(0, 0, 10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => new VectorGrid2D(bounds, 1, 2, Array.Empty<Vector2>()));
        Assert.Throws<ArgumentException>(() => new VectorGrid2D(bounds, 2, 2, Array.Empty<Vector2>()));
        Assert.Throws<ArgumentOutOfRangeException>(() => VectorGrid2D.FromField(bounds, 4000, 4000,
            VectorFields.Constant(Vector2.One)));
    }

    [Fact]
    public void FromFieldPreservesConstantAndLinearFields()
    {
        var bounds = new FlowBounds(0, 0, 100, 50);
        var field = VectorFields.Angles((x, y) => 0f);
        var grid = VectorGrid2D.FromField(bounds, 5, 4, field);
        Assert.InRange(grid.Sample(14, 27).X, .99999f, 1.00001f);
        Assert.InRange(grid.Sample(14, 27).Y, -.00001f, .00001f);
    }
}
