using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class LineworkAlgorithmTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RationalGearsCloseAndRemainInsideAnalyticEnvelope(bool outside)
    {
        var curve = new GuillocheCurve(64, 17, .48, outside, 7, .12, .4);
        Assert.Equal(17, curve.PeriodTurns);
        Assert.True(Vector2.Distance(curve.Sample(0), curve.Sample(Math.Tau * curve.PeriodTurns)) < .0001f);
        Vector2[] points = curve.CreatePolyline(512);
        Assert.Equal(Math.Max(512, curve.MinimumSamples) + 1, points.Length);
        Assert.Equal(points[0], points[^1]);
        Assert.All(points, p => Assert.True(p.Length() <= curve.EnvelopeRadius + .0001));
        Assert.Equal(points, curve.CreatePolyline(512));
    }

    [Theory]
    [InlineData(false, .75)]
    [InlineData(true, 1.25)]
    public void ZeroPenAndZeroModulationReduceToCircle(bool outside, double radius)
    {
        var curve = new GuillocheCurve(64, 16, 0, outside, waveAmount: 0);
        Assert.Equal(1, curve.PeriodTurns);
        foreach (double t in new[] { 0d, .15, 1.4, 4.7, Math.Tau })
            Assert.Equal(radius, curve.Sample(t).Length(), 5);
    }

    [Fact]
    public void InvalidGearsAndPrecisionAreRejectedAndCurveGenerationCanBeCancelled()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GuillocheCurve(16, 17));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GuillocheCurve(waveAmount: double.NaN));
        var curve = new GuillocheCurve();
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.CreatePolyline(100));
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => curve.CreatePolyline(48000, cancel.Token));
    }

    [Fact]
    public void HatchingIsPeriodicAndDoesNotInkWhiteTone()
    {
        var hatch = new TonalHatching(6, .8, 0, 60, 1);
        Assert.Equal(new HatchCoverage(0, 0), hatch.Sample(10, 10, 0));
        Assert.Equal(hatch.Sample(3, .2, .8).Primary, hatch.Sample(3, 6.2, .8).Primary, 12);
        Assert.True(hatch.Sample(0, 0, 1).Primary > hatch.Sample(0, 3, 1).Primary);
        Assert.Equal(0, hatch.Sample(0, 0, 1).Cross);
    }

    [Fact]
    public void TonalCoverageIsMonotoneAndCrossingAppearsInDarkerTones()
    {
        var hatch = new TonalHatching();
        for (int y = 0; y < 20; y++)
            for (int x = 0; x < 20; x++)
            {
                double before = 0;
                for (int i = 0; i <= 10; i++)
                {
                    HatchCoverage sample = hatch.Sample(x, y, i / 10d);
                    double coverage = 1 - (1 - sample.Primary) * (1 - sample.Cross);
                    Assert.InRange(coverage, 0, 1);
                    Assert.True(coverage + 1e-12 >= before);
                    before = coverage;
                }
            }
        Assert.Equal(0, hatch.Sample(0, 0, .2).Cross);
        Assert.Contains(Enumerable.Range(0, 20).Select(x => hatch.Sample(x, 0, 1)), p => p.Cross > 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => new TonalHatching(2, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => hatch.Sample(0, 0, double.NaN));
    }

    [Fact]
    public void ExtendedFractalControlsKeepDefaultsButChangeTheFieldWhenAdjusted()
    {
        var original = new DomainWarpNoise2D(42);
        Assert.Equal(original.Sample(.17, .43), new DomainWarpNoise2D(42,
            lacunarity: 2, persistence: .5, warpFrequency: 1).Sample(.17, .43));
        Assert.NotEqual(original.Sample(.17, .43), new DomainWarpNoise2D(42, lacunarity: 2.4).Sample(.17, .43));
        Assert.NotEqual(original.Sample(.17, .43), new DomainWarpNoise2D(42, persistence: .7).Sample(.17, .43));
        Assert.NotEqual(original.Sample(.17, .43), new DomainWarpNoise2D(42, warpFrequency: .5).Sample(.17, .43));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DomainWarpNoise2D(42, persistence: 1));
    }
}
