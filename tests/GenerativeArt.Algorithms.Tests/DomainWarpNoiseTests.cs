using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class DomainWarpNoiseTests
{
    [Fact]
    public void ZeroWarpEqualsUndeformedFbm()
    {
        var warped = new DomainWarpNoise2D(72, 3, 0);
        var noise = new SeededSimplexNoise(72);
        double x = .43, y = -1.27;
        double expected = (noise.Sample2D(x, y) + .5 * noise.Sample2D(x * 2, y * 2)
            + .25 * noise.Sample2D(x * 4, y * 4)) / 1.75;
        Assert.Equal(expected, warped.Sample(x, y), 12);
    }

    [Fact]
    public void WarpIsDeterministicBoundedAndSensitiveToSeedAndLayers()
    {
        var a = new DomainWarpNoise2D(42);
        var b = new DomainWarpNoise2D(42);
        var c = new DomainWarpNoise2D(43);
        var oneLayer = new DomainWarpNoise2D(42, warpLayers: 1);
        var values = Enumerable.Range(0, 100).Select(i => a.Sample(i * .17, -i * .11)).ToArray();
        Assert.Equal(values, Enumerable.Range(0, 100).Select(i => b.Sample(i * .17, -i * .11)));
        Assert.All(values, v => Assert.InRange(v, -1, 1));
        Assert.NotEqual(a.Sample(.43, .72), c.Sample(.43, .72));
        Assert.NotEqual(a.Sample(.43, .72), oneLayer.Sample(.43, .72));
        Assert.True(Math.Abs(a.Sample(.43, .72) - a.Sample(.430001, .720001)) < .001);
    }

    [Fact]
    public void InvalidOptionsAndCoordinatesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DomainWarpNoise2D(1, octaves: 7));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DomainWarpNoise2D(1, warpLayers: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DomainWarpNoise2D(1, warpAmount: double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DomainWarpNoise2D(1, warpAmount: -1));
        var noise = new DomainWarpNoise2D(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => noise.Sample(double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => noise.Sample(0, double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => noise.Sample(1e8, 0));
    }
}
