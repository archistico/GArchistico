using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class SimplexNoiseTests
{
    // 18 vettori prodotti dal sorgente Gȧrt originale SimplexNoise.kt.
    // Sono costanti di test: nessuna toolchain Kotlin viene inclusa o richiesta.
    [Theory]
    [InlineData(0d, 0d, 0d)]
    [InlineData(0.1d, 0.2d, -0.294107569872242d)]
    [InlineData(-0.1d, 0.2d, 0.3033623593446643d)]
    [InlineData(-3.75d, -2.125d, 0.18735110642489908d)]
    [InlineData(12.34d, -56.78d, 0.06586518489189512d)]
    [InlineData(256.125d, 15.5d, -0.7880543472456225d)]
    [InlineData(1d, 1d, -0.44026771907549944d)]
    [InlineData(0.25d, 0.25d, -0.4987035860723359d)]
    [InlineData(-0.0001d, -0.00001d, 0.0004374999646499529d)]
    public void Classic2DMatchesOriginalGart(double x, double y, double expected)
    {
        Assert.InRange(Math.Abs(ClassicSimplexNoise.Sample2D(x, y) - expected), 0d, 1e-12d);
    }

    [Theory]
    [InlineData(0d, 0d, 0d, 0d)]
    [InlineData(0.1d, 0.2d, 0.3d, 0.6358903679999998d)]
    [InlineData(-0.1d, 0.2d, 0.3d, 0.10088756490534977d)]
    [InlineData(-3.75d, -2.125d, 8.5d, 0.15981115671793744d)]
    [InlineData(12.34d, -56.78d, 90.12d, -0.3075616506939727d)]
    [InlineData(256.125d, 15.5d, -4.25d, 0.11481115366619776d)]
    [InlineData(1d, 1d, 1d, 0d)]
    [InlineData(0.25d, 0.25d, 0.25d, 0.9265007812499998d)]
    [InlineData(-0.0001d, -0.00001d, -0.1d, -0.00042658673145847664d)]
    public void Classic3DMatchesOriginalGart(double x, double y, double z, double expected)
    {
        Assert.InRange(Math.Abs(ClassicSimplexNoise.Sample3D(x, y, z) - expected), 0d, 1e-12d);
    }

    // Risultati della stessa formulazione simplex con permutazione SplitMix64-v1.
    // NON sono vettori dell'OpenSimplex originale: caratterizzano il nostro algoritmo.
    [Theory]
    [InlineData(0UL, 0.1d, 0.2d, -0.2797572474996312d)]
    [InlineData(0UL, -3.75d, -2.125d, 0.7995152668950247d)]
    [InlineData(42UL, 0.1d, 0.2d, 0.2971559181809211d)]
    [InlineData(42UL, -3.75d, -2.125d, -0.36980708683366337d)]
    public void Seeded2DMatchesVersionedGolden(ulong seed, double x, double y, double expected)
    {
        Assert.InRange(Math.Abs(new SeededSimplexNoise(seed).Sample2D(x, y) - expected), 0d, 1e-12d);
    }

    [Theory]
    [InlineData(0UL, 0.1d, 0.2d, 0.3d, -0.683831573333333d)]
    [InlineData(0UL, -3.75d, -2.125d, 8.5d, -0.20419067624409953d)]
    [InlineData(42UL, 0.1d, 0.2d, 0.3d, -0.03416035199999995d)]
    [InlineData(42UL, -3.75d, -2.125d, 8.5d, 0.3157676979064962d)]
    public void Seeded3DMatchesVersionedGolden(ulong seed, double x, double y, double z, double expected)
    {
        Assert.InRange(Math.Abs(new SeededSimplexNoise(seed).Sample3D(x, y, z) - expected), 0d, 1e-12d);
    }

    [Fact]
    public void SameSeedAndCoordinatesAlwaysMatch()
    {
        var first = new SeededSimplexNoise(42UL);
        var second = new SeededSimplexNoise(42UL);
        Assert.Equal(42UL, first.Seed);
        Assert.Equal(first.Sample2D(-7.125d, 3.25d), second.Sample2D(-7.125d, 3.25d));
        Assert.Equal(first.Sample3D(0.1d, 0.2d, 0.3d), second.Sample3D(0.1d, 0.2d, 0.3d));
    }

    [Fact]
    public void DifferentSeedsAlterSamples()
    {
        var one = new SeededSimplexNoise(1UL);
        var other = new SeededSimplexNoise(99UL);
        Assert.NotEqual(one.Sample2D(0.31d, 0.27d), other.Sample2D(0.31d, 0.27d));
        Assert.NotEqual(one.Sample3D(-0.13d, 0.21d, 0.35d), other.Sample3D(-0.13d, 0.21d, 0.35d));
    }

    [Fact]
    public void FixedAndSeededAreIndependentAlgorithms()
    {
        var seeded = new SeededSimplexNoise(42UL);
        Assert.NotEqual(ClassicSimplexNoise.Sample2D(0.1d, 0.2d), seeded.Sample2D(0.1d, 0.2d));
        Assert.NotEqual(ClassicSimplexNoise.Sample3D(0.1d, 0.2d, 0.3d), seeded.Sample3D(0.1d, 0.2d, 0.3d));
    }

    [Fact]
    public void StatelessSamplingDoesNotDependOnCallOrder()
    {
        var noise = new SeededSimplexNoise(777UL);
        double sample = noise.Sample3D(0.1d, 0.2d, 0.3d);
        for (int i = 0; i < 100; i++)
            _ = noise.Sample2D(i * 0.17d, -i * 0.13d);
        Assert.Equal(sample, noise.Sample3D(0.1d, 0.2d, 0.3d));
    }

    [Fact]
    public void ConcurrentReadsMatchSequentialSampling()
    {
        var noise = new SeededSimplexNoise(42UL);
        var samples = new double[512];
        Parallel.For(0, samples.Length, i =>
            samples[i] = noise.Sample3D(i * 0.001d, i * -0.013d, 2.3d));
        for (int i = 0; i < samples.Length; i++)
            Assert.Equal(noise.Sample3D(i * 0.001d, i * -0.013d, 2.3d), samples[i]);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RejectsNonFiniteCoordinates(double coordinate)
    {
        var seeded = new SeededSimplexNoise(42UL);
        Assert.Throws<ArgumentOutOfRangeException>(() => seeded.Sample2D(coordinate, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => seeded.Sample2D(0d, coordinate));
        Assert.Throws<ArgumentOutOfRangeException>(() => seeded.Sample3D(0d, coordinate, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassicSimplexNoise.Sample3D(0d, 0d, coordinate));
    }

    [Fact]
    public void RejectsCoordinatesOutsideSupportedCells()
    {
        var seeded = new SeededSimplexNoise(42UL);
        Assert.Throws<ArgumentOutOfRangeException>(() => seeded.Sample2D(double.MaxValue, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => seeded.Sample3D(0d, 0d, double.MinValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassicSimplexNoise.Sample2D(1e15d, 2e15d));
    }

    [Fact]
    public void AllOriginSamplesAreZero()
    {
        Assert.Equal(0d, ClassicSimplexNoise.Sample2D(0d, 0d));
        Assert.Equal(0d, ClassicSimplexNoise.Sample3D(0d, 0d, 0d));
        Assert.Equal(0d, new SeededSimplexNoise(42UL).Sample2D(0d, 0d));
        Assert.Equal(0d, new SeededSimplexNoise(42UL).Sample3D(0d, 0d, 0d));
    }
}
