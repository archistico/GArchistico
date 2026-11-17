using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Pattern;
using GenerativeArt.Algorithms.Sampling;
using GenerativeArt.Core.Rendering;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class ProductionParallelTests
{
    [Fact]
    public void ReactionParallelismPreservesAllCellsAcrossIterations()
    {
        var serial = new ReactionDiffusionField2D(64, 48, 19);
        var parallel = new ReactionDiffusionField2D(64, 48, 19, production: true);
        serial.Evolve(40, .0367, .0649, .16, .08);
        parallel.Evolve(40, .0367, .0649, .16, .08);
        for (int y = 0; y < 48; y++)
        for (int x = 0; x < 64; x++)
            Assert.Equal(serial.Sample((x + .5) / 64, (y + .5) / 48), parallel.Sample((x + .5) / 64, (y + .5) / 48));
    }

    [Fact]
    public void LicParallelismPreservesEverySampleIncludingEdges()
    {
        float[] texture = Enumerable.Range(0, 64 * 48).Select(i => (float)(i % 31) / 30).ToArray();
        var field = new UniformField();
        Assert.Equal(LineIntegralConvolution.Apply(64, 48, texture, field),
            LineIntegralConvolution.Apply(64, 48, texture, field, production: true));
    }

    [Fact]
    public void ParallelRowsHonorCancellation()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => RasterRows.For(0, 1000, true, cancelled.Token,
            _ => throw new InvalidOperationException("No work allowed after cancellation.")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndexedLoopDetectionPreservesEveryStreamlinePoint(bool vortex)
    {
        var options = new EvenStreamlineOptions
        {
            Bounds = new FlowBounds(0, 0, 240, 180), Separation = 3, StepSize = .5f,
            MaxStepsPerDirection = 800, MaxLines = 120, MaxCandidates = 8000,
            DistributeSeedBudget = false
        };
        var field = VectorFields.FromFunction((x, y) => vortex ? new Vector2(90 - y, x - 120) : new Vector2(1, .3f));
        var serial = EvenStreamlineTracer.Trace(field, options);
        var indexed = EvenStreamlineTracer.Trace(field, options with { Production = true });
        Assert.Equal(serial.Count, indexed.Count);
        for (int i = 0; i < serial.Count; i++) Assert.Equal(serial[i], indexed[i]);
    }

    [Fact]
    public void ProductionSeedPrefixReachesEveryRegionBeforeLineLimit()
    {
        var options = new EvenStreamlineOptions
        {
            Bounds = new FlowBounds(0, 0, 1920, 1080), Separation = 1, StepSize = .25f,
            MaxStepsPerDirection = 40, MaxLines = 100, MaxCandidates = 200000, Production = true
        };
        var paths = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.UnitX), options);
        Assert.Equal(100, paths.Count);
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
            Assert.Contains(paths, line => line.Any(p => (int)(p.X / 480) == col && (int)(p.Y / 360) == row));
    }

    private sealed class UniformField : IVectorField2D
    {
        public Vector2 Sample(float x, float y) => new(1, .3f);
    }

    [Fact]
    public void ProductionPoissonDistributesLimitedPointsAndKeepsMinimumDistance()
    {
        var points = PoissonDiskSampler.Sample(1920, 1080, 8, 19, 300, production: true);
        Assert.Equal(300, points.Count);
        Assert.Equal(points, PoissonDiskSampler.Sample(1920, 1080, 8, 19, 300, production: true));
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
            Assert.Contains(points, p => (int)(p.X / 480) == col && (int)(p.Y / 360) == row);
        for (int i = 0; i < points.Count; i++)
        for (int j = 0; j < i; j++) Assert.True(Vector2.DistanceSquared(points[i], points[j]) >= 64);
    }

    [Fact]
    public void HigherPoissonCountsRequireExplicitProduction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(3840, 2160, 2.16f, 500000));
        PoissonDiskSampler.Validate(3840, 2160, 2.16f, 500000, production: true);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => PoissonDiskSampler.Sample(3840, 2160, 2.16f, 19, 500000,
            cancellationToken: cancelled.Token, production: true));
    }
}
