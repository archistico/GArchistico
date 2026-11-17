using GenerativeArt.Algorithms.Pattern;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class PatternGeometryTests
{
    [Fact]
    public void PhyllotaxisIsDeterministicAndSeedDependent()
    {
        var points = PhyllotaxisPattern.Generate(42, 128, 137.507764, .2, 7, "rosette").ToArray();
        Assert.Equal(points, PhyllotaxisPattern.Generate(42, 128, 137.507764, .2, 7, "rosette").ToArray());
        Assert.NotEqual(points, PhyllotaxisPattern.Generate(43, 128, 137.507764, .2, 7, "rosette").ToArray());
        Assert.All(points, p => { Assert.True(double.IsFinite(p.X)); Assert.True(double.IsFinite(p.Y)); });
        Assert.Equal(128, points.Length);
    }

    [Theory]
    [InlineData("sunflower")]
    [InlineData("rosette")]
    [InlineData("galaxy")]
    public void PhyllotaxisSupportsAllShapes(string shape) =>
        Assert.Equal(40, PhyllotaxisPattern.Generate(12, 40, 137.5, 0, 5, shape).Count());

    [Fact]
    public void ReactionDiffusionIsBoundedAndDeterministic()
    {
        var a = new ReactionDiffusionField2D(48, 36, 42, spots: 13, spotRadius: 3);
        var b = new ReactionDiffusionField2D(48, 36, 42, spots: 13, spotRadius: 3);
        a.Evolve(110, .035, .061, .16, .08);
        b.Evolve(110, .035, .061, .16, .08);
        double[] samples = [a.Sample(.2, .4), a.Sample(.5, .5), a.Sample(.88, .7)];
        Assert.Equal(samples, new[] { b.Sample(.2, .4), b.Sample(.5, .5), b.Sample(.88, .7) });
        Assert.All(samples, v => Assert.InRange(v, 0, 1));
        var c = new ReactionDiffusionField2D(48, 36, 43, spots: 13, spotRadius: 3);
        c.Evolve(110, .035, .061, .16, .08);
        Assert.NotEqual(samples, new[] { c.Sample(.2, .4), c.Sample(.5, .5), c.Sample(.88, .7) });
    }

    [Fact]
    public void ReactionDiffusionRejectsExcessiveMemoryAndCanBeCancelled()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReactionDiffusionField2D(2048, 2048, 42));
        var grid = new ReactionDiffusionField2D(300, 250, 42);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.Evolve(1200, .04, .06, .16, .08));
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Assert.Throws<OperationCanceledException>(() => grid.Evolve(2, .04, .06, .16, .08, canceled.Token));
    }
}
