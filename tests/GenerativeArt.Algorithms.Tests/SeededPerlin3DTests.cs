using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class SeededPerlin3DTests
{
    // Golden generati da oracle Python indipendente (NON Gȧrt Kotlin).
    [Theory]
    [InlineData(0UL, 0d, 0d, 0d, 0d)]
    [InlineData(0UL, 0.1d, 0.2d, 0.3d, -0.5838644028562432d)]
    [InlineData(0UL, -3.75d, -2.125d, 8.5d, 0.20191115140914917d)]
    [InlineData(0UL, 12.34d, -56.78d, 90.12d, 0.014110914155159521d)]
    [InlineData(0UL, 256.125d, 15.5d, -4.25d, -0.4280899167060852d)]
    [InlineData(1UL, 0d, 0d, 0d, 0d)]
    [InlineData(1UL, 0.1d, 0.2d, 0.3d, 0.031565249173094403d)]
    [InlineData(1UL, -3.75d, -2.125d, 8.5d, -0.09533326327800751d)]
    [InlineData(1UL, 12.34d, -56.78d, 90.12d, 0.05433313838805755d)]
    [InlineData(1UL, 256.125d, 15.5d, -4.25d, -0.15720873326063156d)]
    [InlineData(42UL, 0d, 0d, 0d, 0d)]
    [InlineData(42UL, 0.1d, 0.2d, 0.3d, 0.14940977103513597d)]
    [InlineData(42UL, -3.75d, -2.125d, 8.5d, 0.32486849278211594d)]
    [InlineData(42UL, 12.34d, -56.78d, 90.12d, 0.048209546760052575d)]
    [InlineData(42UL, 256.125d, 15.5d, -4.25d, 0.002368845045566559d)]
    [InlineData(18446744073709551615UL, 0d, 0d, 0d, 0d)]
    [InlineData(18446744073709551615UL, 0.1d, 0.2d, 0.3d, -0.40008368417259516d)]
    [InlineData(18446744073709551615UL, -3.75d, -2.125d, 8.5d, 0.31243235617876053d)]
    [InlineData(18446744073709551615UL, 12.34d, -56.78d, 90.12d, -0.02342922987134683d)]
    [InlineData(18446744073709551615UL, 256.125d, 15.5d, -4.25d, -0.543611578643322d)]
    public void MatchesIndependentSeededOracle(ulong seed, double x, double y, double z, double expected)
    {
        var perlin = new SeededPerlin3D(seed);
        Assert.InRange(Math.Abs(perlin.Sample(x, y, z) - expected), 0d, 1e-12d);
    }

    [Fact]
    public void SameSeedAlwaysProducesSameSamples()
    {
        var first = new SeededPerlin3D(42);
        var second = new SeededPerlin3D(42);
        Assert.Equal(first.Sample(-.2, 4.3, 1.7), second.Sample(-.2, 4.3, 1.7));
        Assert.NotEqual(new SeededPerlin3D(11).Sample(.1, .2, .3), first.Sample(.1, .2, .3));
    }

    [Fact]
    public void SeededNoiseIsPeriodicEvery256Units()
    {
        var noise = new SeededPerlin3D(42);
        Assert.InRange(Math.Abs(noise.Sample(.25, -.75, 1.125) -
            noise.Sample(256.25, -.75, 1.125)), 0d, 1e-12d);
    }

    [Fact]
    public void FbmWithOneOctaveMatchesBaseNoise()
    {
        var noise = new SeededPerlin3D(42);
        Assert.Equal(noise.Sample(.1, .2, .3),
            FractalNoise3D.Sample(noise, .1, .2, .3, octaves: 1));
    }

    [Fact]
    public void FbmParameterValidationRejectsInvalidValues()
    {
        var noise = new SeededPerlin3D(42);
        Assert.Throws<ArgumentOutOfRangeException>(() => FractalNoise3D.Sample(noise, 0, 0, 0, octaves: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FractalNoise3D.Sample(noise, 0, 0, 0, lacunarity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FractalNoise3D.Sample(noise, 0, 0, 0, persistence: 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => noise.Sample(0, double.NaN, 0));
    }
}
