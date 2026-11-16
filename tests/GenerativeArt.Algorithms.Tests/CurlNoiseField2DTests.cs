using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class CurlNoiseField2DTests
{
    [Fact]
    public void SameSeedProducesSameVectorsAtVariousCoordinates()
    {
        var a = new CurlNoiseField2D(12345UL);
        var b = new CurlNoiseField2D(12345UL);
        foreach (Vector2 p in new[] { Vector2.Zero, new Vector2(73, 9), new Vector2(-400, 11) })
        {
            Vector2 x = a.Sample(p.X, p.Y);
            Assert.Equal(x, b.Sample(p.X, p.Y));
            Assert.True(float.IsFinite(x.X) && float.IsFinite(x.Y));
        }
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurlNoiseField2D(2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurlNoiseField2D(2, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurlNoiseField2D(2, derivativeStep: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurlNoiseField2D(2, derivativeStep: float.PositiveInfinity));
    }

    [Fact]
    public void NonFiniteCoordinatesReturnStationaryVector()
    {
        var field = new CurlNoiseField2D(42);
        Assert.Equal(Vector2.Zero, field.Sample(float.NaN, 0));
        Assert.Equal(Vector2.Zero, field.Sample(10, float.PositiveInfinity));
    }

    [Fact]
    public void CurlIsLocallyApproximatelyDivergenceFree()
    {
        var field = new CurlNoiseField2D(17, frequency: .005f, derivativeStep: .2f);
        const float h = .25f;
        Vector2 a = field.Sample(23 + h, 47), b = field.Sample(23 - h, 47);
        Vector2 c = field.Sample(23, 47 + h), d = field.Sample(23, 47 - h);
        double divergence = (a.X - b.X + c.Y - d.Y) / (2d * h);
        Assert.InRange(Math.Abs(divergence), 0d, .004d);
    }
}
