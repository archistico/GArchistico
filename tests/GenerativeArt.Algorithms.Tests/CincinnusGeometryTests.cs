using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class CincinnusGeometryTests
{
    [Theory]
    [InlineData(2000, 2000)]
    [InlineData(1920, 1080)]
    [InlineData(7680, 4320)]
    public void GeneratedVorticesStayOutsideTheCanvasWithSourceRanges(int width, int height)
    {
        var field = CincinnusFlowField.Create(width, height, seed: 3);
        Assert.Equal(5, field.Vortices.Count);
        float side = Math.Min(width, height);
        foreach (CincinnusVortex v in field.Vortices)
        {
            double outside = Math.Max(Math.Max(-v.Center.X, (double)v.Center.X - width),
                Math.Max(-v.Center.Y, (double)v.Center.Y - height));
            Assert.True(outside > .08d * side);
            Assert.InRange(v.Center.X, -.4f * width - .001f, 1.4f * width + .001f);
            Assert.InRange(v.Center.Y, -.4f * height - .001f, 1.4f * height + .001f);
            Assert.InRange(v.Spin, -1.401f, -.599f);
            Assert.InRange(v.Reach, .209f * side, .391f * side);
        }
    }

    [Fact]
    public void SameSeedSameVorticesAndSamplesDifferentSeedChangesCenters()
    {
        var a = CincinnusFlowField.Create(2000, 2000, 3);
        var b = CincinnusFlowField.Create(2000, 2000, 3);
        var c = CincinnusFlowField.Create(2000, 2000, 4);
        Assert.Equal(a.Vortices.ToArray(), b.Vortices.ToArray());
        Assert.NotEqual(a.Vortices[0].Center, c.Vortices[0].Center);
        foreach (Vector2 p in new[] { new Vector2(0, 0), new Vector2(100, 500), new Vector2(1999, 1999) })
            Assert.Equal(a.Sample(p.X, p.Y), b.Sample(p.X, p.Y));
    }

    [Fact]
    public void ExplicitVortexMatchesExistingVectorFieldsPhysics()
    {
        var vortex = new CincinnusVortex(new Vector2(100, 100), -1f, 100f);
        var actual = CincinnusFlowField.FromVortices([vortex]);
        IVectorField2D reference = VectorFields.Vortex(vortex.Center, vortex.Spin, reach: vortex.Reach);
        foreach (Vector2 p in new[] { new Vector2(200, 100), new Vector2(100, 180), new Vector2(200, 200) })
        {
            Vector2 v = actual.Sample(p.X, p.Y);
            Assert.InRange(Vector2.Distance(v, reference.Sample(p.X, p.Y)), 0f, 0.000001f);
        }
        Vector2 sample = actual.Sample(200, 100);
        Assert.InRange(sample.X, -0.00001f, 0.00001f);
        Assert.InRange(sample.Y, -.50001f, -.49999f);
        Assert.Equal(Vector2.Zero, actual.Sample(100, 100));
    }

    [Fact]
    public void VortexContributionsAreSummedAndMayCancelWithoutInventedDrift()
    {
        var field = CincinnusFlowField.FromVortices([
            new CincinnusVortex(new Vector2(100, 100), -1f, 100f),
            new CincinnusVortex(new Vector2(100, 100), 1f, 100f)
        ]);
        Assert.Equal(Vector2.Zero, field.Sample(150, 110));
        Assert.Single(CincinnusPathGeometry.TraceForward(field, new Vector2(150, 110), 300, 300));
    }

    [Fact]
    public void FromVorticesDefensivelyCopiesTheInput()
    {
        var original = new List<CincinnusVortex> { new(new Vector2(0, 0), -1, 100) };
        var field = CincinnusFlowField.FromVortices(original);
        original[0] = new CincinnusVortex(new Vector2(100, 100), 1, 200);
        Assert.Equal(new Vector2(0, 0), field.Vortices[0].Center);
        Assert.IsAssignableFrom<IReadOnlyList<CincinnusVortex>>(field.Vortices);
        Assert.Throws<NotSupportedException>(() => ((IList<CincinnusVortex>)field.Vortices)[0] = original[0]);
    }

    [Fact]
    public void ForwardBackwardAndCombinedPathsAreOrientedAndBounded()
    {
        var options = new CincinnusTraceOptions { StepSize = 3f, MaximumLength = 90f, Margin = 5f };
        IVectorField2D field = VectorFields.Constant(Vector2.UnitX);
        Vector2 seed = new(10, 10);
        var forward = CincinnusPathGeometry.TraceForward(field, seed, 50, 50, options);
        var backward = CincinnusPathGeometry.TraceBackward(field, seed, 50, 50, options);
        var both = CincinnusPathGeometry.TraceBidirectional(field, seed, 50, 50, options);
        Assert.Equal(seed, forward[0]);
        Assert.Equal(seed, backward[0]);
        Assert.True(forward.Count > 3 && backward.Count > 3);
        Assert.Equal(backward.Count + forward.Count - 1, both.Count);
        Assert.Equal(1, both.Count(x => x == seed));
        Assert.All(both, p => Assert.True(p.X >= -5 && p.X < 55 && p.Y >= -5 && p.Y < 55));
        for (int i = 1; i < both.Count; i++)
            Assert.True(both[i].X > both[i - 1].X);
    }

    [Fact]
    public void Rk4PreservesApproximateCircularOrbit()
    {
        var field = CincinnusFlowField.FromVortices([
            new CincinnusVortex(new Vector2(100, 100), 1, 1000)
        ]);
        var points = CincinnusPathGeometry.TraceForward(field, new Vector2(150, 100),
            200, 200, new CincinnusTraceOptions { StepSize = 3, MaximumLength = 900, Margin = 100 });
        Assert.True(points.Count > 250);
        foreach (Vector2 p in points)
            Assert.InRange(Vector2.Distance(p, new Vector2(100, 100)), 49.5f, 50.5f);
    }

    [Fact]
    public void GeneratedFieldProducesReproducibleTraces()
    {
        var a = CincinnusFlowField.Create(2000, 2000, 3);
        var b = CincinnusFlowField.Create(2000, 2000, 3);
        var opt = new CincinnusTraceOptions { MaximumLength = 600 };
        var pa = CincinnusPathGeometry.TraceForward(a, new Vector2(1000, 1000), 2000, 2000, opt);
        var pb = CincinnusPathGeometry.TraceForward(b, new Vector2(1000, 1000), 2000, 2000, opt);
        Assert.True(pa.Count > 20);
        Assert.Equal(pa.ToArray(), pb.ToArray());
    }

    [Fact]
    public void ClosingLoopIsFoundOnlyAfterGoingFarEnough()
    {
        Vector2[] circle = Enumerable.Range(0, 129).Select(i =>
        {
            double theta = i * Math.PI * 2 / 128;
            return new Vector2((float)(20d - 20d * Math.Cos(theta)), (float)(20d * Math.Sin(theta)));
        }).ToArray();
        int index = CincinnusPathGeometry.FindFirstReturn(circle, 3);
        Assert.InRange(index, 110, 128);
        Assert.Equal(-1, CincinnusPathGeometry.FindFirstReturn(
            Enumerable.Range(0, 50).Select(i => new Vector2(i * 3, 0)).ToArray(), 3));
        Assert.Equal(-1, CincinnusPathGeometry.FindFirstReturn(circle, 3, minDistanceAway: 50));
    }

    [Fact]
    public void CurvatureEstimatorSeparatesLinesAndArcs()
    {
        Vector2[] straight = Enumerable.Range(0, 200).Select(i => new Vector2(i * 3, 50)).ToArray();
        Assert.All(CincinnusPathGeometry.EstimateBendRadii(straight), r =>
            Assert.Equal(CincinnusPathGeometry.StraightBendRadius, r));
        const float radius = 50f;
        const int segments = 200;
        float step = 2 * MathF.PI * radius / segments;
        Vector2[] arc = Enumerable.Range(0, segments + 1).Select(i =>
        {
            float a = 2f * MathF.PI * i / segments;
            return new Vector2(radius * MathF.Cos(a), radius * MathF.Sin(a));
        }).ToArray();
        float[] values = CincinnusPathGeometry.EstimateBendRadii(arc, step);
        Assert.InRange(values[100], 48f, 52f);
    }

    [Fact]
    public void InvalidBudgetsNaNsAndMalformedParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.Create(0, 200));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.Create(200, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.Create(200, 200, vortexCount: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.Create(200, 200, candidatesPerVortex: 65));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.FromVortices([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusFlowField.FromVortices([
            new CincinnusVortex(Vector2.Zero, -1, float.NaN)
        ]));
        var field = CincinnusFlowField.Create(200, 200);
        Assert.Throws<ArgumentOutOfRangeException>(() => field.Sample(float.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusPathGeometry.TraceForward(field,
            new Vector2(10, 10), 200, 200, new CincinnusTraceOptions { StepSize = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusPathGeometry.TraceForward(field,
            new Vector2(10, 10), 200, 200, new CincinnusTraceOptions { StepSize = 1, MaximumLength = 20_001 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusPathGeometry.TraceForward(field,
            new Vector2(10, 10), 200, 200, new CincinnusTraceOptions { Margin = float.PositiveInfinity }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusPathGeometry.TraceForward(field,
            new Vector2(10, 10), 200, 200, new CincinnusTraceOptions { MaximumLength = float.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusPathGeometry.FindFirstReturn([Vector2.Zero], 0));
        Assert.Throws<ArgumentException>(() => CincinnusPathGeometry.EstimateBendRadii([Vector2.Zero]));
        Assert.Throws<ArgumentException>(() => CincinnusPathGeometry.EstimateBendRadii([
            Vector2.Zero, new Vector2(float.NaN, 1)
        ]));
    }

    [Fact]
    public void CreationAndTracingRespectAlreadyCancelledToken()
    {
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => CincinnusFlowField.Create(
            2000, 2000, cancellationToken: cancel.Token));
        var field = CincinnusFlowField.Create(2000, 2000);
        Assert.Throws<OperationCanceledException>(() => CincinnusPathGeometry.TraceForward(
            field, new Vector2(1000, 1000), 2000, 2000, cancellationToken: cancel.Token));
        Assert.Throws<OperationCanceledException>(() => CincinnusPathGeometry.TraceBidirectional(
            field, new Vector2(1000, 1000), 2000, 2000, cancellationToken: cancel.Token));
    }
}
