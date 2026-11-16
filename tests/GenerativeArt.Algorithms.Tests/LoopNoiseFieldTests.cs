using System.Numerics;
using GenerativeArt.Algorithms.Animation;
using GenerativeArt.Core.Rendering;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class LoopNoiseFieldTests
{
    [Fact]
    public void PhaseZeroAndOneHaveSameFieldWithinFloatTolerance()
    {
        var start = new LoopNoiseField2D(42, 0);
        var end = new LoopNoiseField2D(42, 1);
        foreach (Vector2 p in new[] { new Vector2(0, 0), new Vector2(130, 270), new Vector2(-40, 6) })
            Assert.InRange(Vector2.Distance(start.Sample(p.X, p.Y), end.Sample(p.X, p.Y)), 0, 1e-5f);
    }

    [Fact]
    public void SameSeedAndPhaseProduceSameField()
    {
        double phase = LoopFrame.At(9, 24, 24).Phase;
        var a = new LoopNoiseField2D(722, phase);
        var b = new LoopNoiseField2D(722, phase);
        Assert.Equal(a.Sample(13, 42), b.Sample(13, 42));
    }

    [Fact]
    public void DifferentPhasesChangeFieldAndRemainFinite()
    {
        var a = new LoopNoiseField2D(77, 0);
        var b = new LoopNoiseField2D(77, .25);
        Vector2 va = a.Sample(85, 37), vb = b.Sample(85, 37);
        bool changes = Vector2.Distance(va, vb) > 1e-4f
            || Vector2.Distance(a.Sample(152, 47), b.Sample(152, 47)) > 1e-4f;
        Assert.True(changes);
        Assert.True(float.IsFinite(va.X) && float.IsFinite(vb.Y));
        Assert.Equal(Vector2.Zero, a.Sample(float.NaN, 23));
    }

    [Fact]
    public void RejectsInvalidPhaseOrFrequency()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoopNoiseField2D(1, -.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoopNoiseField2D(1, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoopNoiseField2D(1, 0, frequency: 0));
    }
}
