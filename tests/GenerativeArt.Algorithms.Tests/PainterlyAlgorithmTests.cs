using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class PainterlyAlgorithmTests
{
    private static Vector2[] Straight => Enumerable.Range(0, 21).Select(i => new Vector2(i, 0)).ToArray();

    [Fact]
    public void StraightBrushHasPredictableWidthAndTaperedEnds()
    {
        var constant = OrganicStroke.Build(Straight, 3, 10, 0, 0, 42).ToArray();
        Assert.Equal(3, constant.Length);
        Assert.All(constant[0].Points, p => Assert.Equal(-5, p.Y));
        Assert.All(constant[2].Points, p => Assert.Equal(5, p.Y));
        Assert.All(constant[1].Points, p => Assert.Equal(0, p.Y));
        var tapered = OrganicStroke.Build(Straight, 3, 10, 1, 0, 42).ToArray();
        Assert.True(Math.Abs(tapered[0].Points[0].Y) < Math.Abs(tapered[0].Points[10].Y));
        Assert.Equal(21, tapered[0].Points.Length);
    }

    [Fact]
    public void SetolesAreSeededFiniteAndCanBeCancelledDuringEnumeration()
    {
        var a = OrganicStroke.Build(Straight, 6, 20, 1, .5, 17).ToArray();
        var b = OrganicStroke.Build(Straight, 6, 20, 1, .5, 17).ToArray();
        var c = OrganicStroke.Build(Straight, 6, 20, 1, .5, 18).ToArray();
        Assert.Equal(a[0].Points, b[0].Points);
        Assert.False(a[0].Points.AsSpan().SequenceEqual(c[0].Points));
        Assert.All(a.SelectMany(p => p.Points), p => Assert.True(float.IsFinite(p.X) && float.IsFinite(p.Y)));
        using var cancellation = new CancellationTokenSource();
        using var strokes = OrganicStroke.Build(Straight, 6, 20, 1, .5, 17, cancellation.Token).GetEnumerator();
        Assert.True(strokes.MoveNext());
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => strokes.MoveNext());
        Assert.Throws<ArgumentOutOfRangeException>(() => OrganicStroke.Build(Straight, 41, 20, 1, .5, 17));
    }

    [Fact]
    public void FractalProfilesAreContinuousBoundedAndRespondToSeedAndDetail()
    {
        var a = new FractalProfile1D(42);
        var b = new FractalProfile1D(42);
        double[] values = Enumerable.Range(0, 100).Select(i => a.Sample(i * .04, .7)).ToArray();
        Assert.Equal(values, Enumerable.Range(0, 100).Select(i => b.Sample(i * .04, .7)));
        Assert.All(values, v => Assert.InRange(v, -1, 1));
        Assert.NotEqual(a.Sample(.43, .7), new FractalProfile1D(43).Sample(.43, .7));
        Assert.NotEqual(a.Sample(.43, .7), new FractalProfile1D(42, persistence: .8).Sample(.43, .7));
        Assert.True(Math.Abs(a.Sample(.43, .7) - a.Sample(.430001, .7)) < .001);
        Assert.Throws<ArgumentOutOfRangeException>(() => a.Sample(double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FractalProfile1D(1, octaves: 8));
    }

    [Fact]
    public void SingleFlowTraceCanBeCancelledWhileSampling()
    {
        using var cancellation = new CancellationTokenSource();
        int sampled = 0;
        var field = VectorFields.FromFunction((_, _) =>
        {
            if (++sampled == 17) cancellation.Cancel();
            return Vector2.UnitX;
        });
        Assert.Throws<OperationCanceledException>(() => FlowTracer.Trace(field, Vector2.Zero,
            2000, cancellationToken: cancellation.Token));
        Assert.InRange(sampled, 17, 80);
    }
}
