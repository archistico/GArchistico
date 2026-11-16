using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class EvenStreamlineTracerTests
{
    [Fact]
    public void DenseFullHdFieldExceedsFiveHundredLinesAndReachesAllQuadrants()
    {
        var lines = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.UnitX), new EvenStreamlineOptions
        {
            Bounds = new FlowBounds(0, 0, 1920, 1080), Separation = 4, StepSize = 2,
            MaxStepsPerDirection = 35, MaxLines = 10000, MaxCandidates = 200000
        });
        Assert.True(lines.Count > 500);
        foreach (var (left, top) in new[] { (true, true), (true, false), (false, true), (false, false) })
            Assert.Contains(lines, line => line.Any(p => (p.X < 960) == left && (p.Y < 540) == top));
    }

    [Fact]
    public void LimitedSeedBudgetIsDistributedAcrossWholeCanvas()
    {
        var lines = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.UnitX), new EvenStreamlineOptions
        {
            Bounds = new FlowBounds(0, 0, 1920, 1080), Separation = 2, StepSize = 2,
            MaxStepsPerDirection = 8, MaxLines = 1000, MaxCandidates = 200
        });
        Assert.Contains(lines, line => line.Any(p => p.X > 1800 && p.Y > 950));
    }

    [Fact]
    public void DenseTracingCanBeCancelledDuringIntegration()
    {
        using var cancellation = new CancellationTokenSource();
        int sampled = 0;
        var field = VectorFields.FromFunction((_, _) =>
        {
            if (++sampled == 30) cancellation.Cancel();
            return Vector2.UnitX;
        });
        Assert.Throws<OperationCanceledException>(() =>
            EvenStreamlineTracer.Trace(field, Defaults, cancellation.Token));
        Assert.InRange(sampled, 30, 150);
    }

    private static EvenStreamlineOptions Defaults => new()
    {
        Bounds = new FlowBounds(0, 0, 110, 70),
        Separation = 12f,
        StepSize = 3f,
        Margin = 2f,
        MaxStepsPerDirection = 45,
        MaxLines = 30,
        MaxCandidates = 500
    };

    [Fact]
    public void ConstantFieldProducesBoundedTraces()
    {
        var options = Defaults;
        var lines = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.UnitX), options);
        Assert.NotEmpty(lines);
        Assert.True(lines.Count <= options.MaxLines);
        foreach (IReadOnlyList<Vector2> line in lines)
        {
            Assert.True(line.Count >= options.MinimumPoints);
            foreach (Vector2 p in line)
                Assert.True(options.Bounds.Contains(p));
        }
    }

    [Fact]
    public void ZeroFieldProducesNoTraces()
    {
        var lines = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.Zero), Defaults);
        Assert.Empty(lines);
    }

    [Fact]
    public void SameInputsProduceIdenticalTraces()
    {
        var field = new CurlNoiseField2D(42UL);
        var opts = Defaults with { MaxLines = 8 };
        var a = EvenStreamlineTracer.Trace(field, opts);
        var b = EvenStreamlineTracer.Trace(field, opts);
        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
            Assert.Equal(a[i].ToArray(), b[i].ToArray());
    }

    [Fact]
    public void DifferentTracesMaintainApproximateVertexSeparation()
    {
        var opts = Defaults with { MaxLines = 12 };
        var lines = EvenStreamlineTracer.Trace(VectorFields.Constant(Vector2.UnitX), opts);
        for (int a = 0; a < lines.Count; a++)
            for (int b = a + 1; b < lines.Count; b++)
                foreach (Vector2 p in lines[a])
                    foreach (Vector2 q in lines[b])
                        Assert.True(Vector2.Distance(p, q) >= opts.Separation * opts.TestDistanceFactor - .1f);
    }

    [Fact]
    public void RejectsNonFiniteAndExcessiveBudgets()
    {
        var field = VectorFields.Constant(Vector2.One);
        Assert.Throws<ArgumentOutOfRangeException>(() => EvenStreamlineTracer.Trace(field,
            Defaults with { Separation = 0f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => EvenStreamlineTracer.Trace(field,
            Defaults with { MaxCandidates = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => EvenStreamlineTracer.Trace(field,
            Defaults with { MaxLines = 10_001 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => EvenStreamlineTracer.Trace(field,
            Defaults with { Margin = 100f }));
        Assert.Throws<ArgumentNullException>(() => EvenStreamlineTracer.Trace(null!, Defaults));
    }
}
