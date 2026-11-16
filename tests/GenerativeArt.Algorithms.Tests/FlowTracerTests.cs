using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class FlowTracerTests
{
    [Fact]
    public void TraceIncludesStartAndStopsBeforeLeavingRightEdge()
    {
        var path = FlowTracer.Trace(VectorFields.Constant(Vector2.UnitX),
            Vector2.Zero, 100, step: 1, bounds: new FlowBounds(0, 0, 3, 4));
        Assert.Equal(3, path.Count);
        Assert.Equal(Vector2.Zero, path[0]);
        Assert.Equal(new Vector2(2, 0), path[2]);
    }

    [Fact]
    public void ZeroStepsReturnsStartOnly()
    {
        var path = FlowTracer.Trace(VectorFields.Constant(Vector2.One), Vector2.Zero, 0);
        Assert.Single(path);
        Assert.Equal(Vector2.Zero, path[0]);
    }

    [Fact]
    public void TraceStopsAtStationaryField()
    {
        var path = FlowTracer.Trace(VectorFields.Constant(Vector2.Zero), new Vector2(4, 5), 12);
        Assert.Single(path);
    }

    [Fact]
    public void TracesFromSameFieldAndSeedAreIdentical()
    {
        var field = VectorFields.Angles((x, y) => x * .01f + y * .02f);
        var a = FlowTracer.Trace(field, new Vector2(20, 30), 40);
        var b = FlowTracer.Trace(field, new Vector2(20, 30), 40);
        Assert.Equal(a.ToArray(), b.ToArray());
        Assert.True(a.Count > 20);
    }

    [Fact]
    public void BoundsUseExclusiveRightAndBottomEdges()
    {
        var bounds = new FlowBounds(-1, 0, 5, 8);
        Assert.True(bounds.Contains(new Vector2(-1, 0)));
        Assert.False(bounds.Contains(new Vector2(5, 7)));
        Assert.False(bounds.Contains(new Vector2(1, 8)));
        Assert.False(bounds.Contains(new Vector2(float.NaN, 1)));
    }

    [Fact]
    public void InvalidTraceRequestsFailBeforeSampling()
    {
        var field = VectorFields.Constant(Vector2.UnitX);
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowTracer.Trace(field, Vector2.Zero, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowTracer.Trace(field, Vector2.Zero, FlowTracer.MaxSteps + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowTracer.Trace(field, Vector2.Zero, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowTracer.Trace(field, new Vector2(-1, 0), 10,
            bounds: new FlowBounds(0, 0, 3, 4)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlowBounds(1, 1, 1, 4));
    }
}
