using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class ClassicPerlin3DTests
{
    // Valori ottenuti compilando gli originali Perlin.kt e PerlinPermTable.kt dello ZIP.
    [Theory]
    [InlineData(0d, 0d, 0d, 0d)]
    [InlineData(0.1d, 0.2d, 0.3d, 0.35122924878110723d)]
    [InlineData(-0.1d, 0.2d, 0.3d, 0.2216273754669053d)]
    [InlineData(-3.75d, -2.125d, 8.5d, 0.12025996297597885d)]
    [InlineData(12.34d, -56.78d, 90.12d, 0.2436181581157053d)]
    [InlineData(256.125d, 15.5d, -4.25d, -0.0939309224486351d)]
    [InlineData(1d, 1d, 1d, 0d)]
    public void MatchesOriginalKotlinGoldenSamples(double x, double y, double z, double expected)
    {
        Assert.InRange(Math.Abs(ClassicPerlin3D.Sample(x, y, z) - expected), 0d, 1e-12d);
    }

    [Fact]
    public void SameCoordinatesYieldSameValue()
    {
        double x = ClassicPerlin3D.Sample(-7.1, 0.2, 3.8);
        Assert.Equal(x, ClassicPerlin3D.Sample(-7.1, 0.2, 3.8));
    }

    [Fact]
    public void RepeatsEvery256IntegerUnits()
    {
        double a = ClassicPerlin3D.Sample(0.25, -0.75, 1.1);
        double b = ClassicPerlin3D.Sample(256.25, -0.75, 1.1);
        Assert.InRange(Math.Abs(a - b), 0d, 1e-12d);
    }

    [Fact]
    public void RejectsNonFiniteInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassicPerlin3D.Sample(double.NaN, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassicPerlin3D.Sample(0, double.PositiveInfinity, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassicPerlin3D.Sample(0, 0, double.NegativeInfinity));
    }
}
