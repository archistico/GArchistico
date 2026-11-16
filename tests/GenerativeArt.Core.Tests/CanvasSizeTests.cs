using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class CanvasSizeTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    [InlineData(100_000, 100_000)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void RejectsInvalidOrOversizeDimensions(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CanvasSize(width, height));

    [Fact]
    public void ComputesAreaWithoutIntOverflow()
    {
        var size = new CanvasSize(8_192, 8_192);
        Assert.Equal(67_108_864L, size.PixelCount);
        Assert.Equal(1f, size.AspectRatio);
    }

    [Fact]
    public void RejectsNegativeFractionalAndNonFiniteCoordinates()
    {
        var size = new CanvasSize(4, 3);
        Assert.False(size.Contains(-0.1f, 1f));
        Assert.False(size.Contains(1f, -0.1f));
        Assert.False(size.Contains(float.NaN, 1f));
        Assert.False(size.Contains(float.PositiveInfinity, 1f));
        Assert.False(size.Contains(4f, 0f));
        Assert.False(size.Contains(0f, 3f));
        Assert.True(size.Contains(0f, 0f));
        Assert.True(size.Contains(3.999f, 2.999f));
    }

    [Fact]
    public void IntegralCoordinatesHaveTheSameEdges()
    {
        var size = new CanvasSize(3, 2);
        Assert.True(size.Contains(2, 1));
        Assert.False(size.Contains(-1, 1));
        Assert.False(size.Contains(3, 1));
    }
}
