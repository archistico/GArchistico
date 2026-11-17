using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class VoronoiLayerPlanTests
{
    [Theory]
    [InlineData(VoronoiHolePattern.Random)]
    [InlineData(VoronoiHolePattern.Clustered)]
    public void HolesAreExactSeededAndIncreasingTheFractionPreservesEarlierHoles(VoronoiHolePattern pattern)
    {
        var a = VoronoiLayerPlan.Build(42, 64, 160, 100, missingFraction: .25, pattern: pattern);
        var b = VoronoiLayerPlan.Build(42, 64, 160, 100, missingFraction: .5, pattern: pattern);
        var copy = VoronoiLayerPlan.Build(42, 64, 160, 100, missingFraction: .25, pattern: pattern);
        Assert.Equal(16, a.Missing.Count(m => m)); Assert.Equal(32, b.Missing.Count(m => m));
        Assert.Equal(a.Missing, copy.Missing);
        for (int i = 0; i < a.Cells.Count; i++)
        {
            Assert.Equal(a.Cells[i].Vertices, b.Cells[i].Vertices);
            Assert.Equal(a.Cells[i].Vertices, copy.Cells[i].Vertices);
            if (a.Missing[i]) Assert.True(b.Missing[i]);
        }
        var changed = VoronoiLayerPlan.Build(43, 64, 160, 100, missingFraction: .25, pattern: pattern);
        Assert.False(a.Missing.SequenceEqual(changed.Missing));
    }

    [Fact]
    public void PatternAndHoleCountDoNotMoveTheSites()
    {
        var a = VoronoiLayerPlan.Build(42, 48, 160, 100, missingFraction: 0, pattern: VoronoiHolePattern.Random);
        var b = VoronoiLayerPlan.Build(42, 48, 160, 100, missingFraction: 1, pattern: VoronoiHolePattern.Clustered);
        Assert.DoesNotContain(a.Missing, missing => missing);
        Assert.All(b.Missing, missing => Assert.True(missing));
        Assert.Equal(a.Cells.Select(c => c.Site), b.Cells.Select(c => c.Site));
    }

    [Fact]
    public void ShiftedCellsStillPartitionCanvasAndGeometryScalesWithResolution()
    {
        var a = VoronoiLayerPlan.Build(11, 48, 160, 100, offset: new Vector2(30, -18));
        var b = VoronoiLayerPlan.Build(11, 48, 320, 200, offset: new Vector2(60, -36));
        Assert.InRange(a.Cells.Sum(c => c.Area), 15999.99, 16000.01);
        Assert.Equal(a.Missing, b.Missing);
        for (int i = 0; i < a.Cells.Count; i++)
            Assert.True(Vector2.Distance(a.Cells[i].Centroid * 2, b.Cells[i].Centroid) < .001f);
        Assert.All(a.Cells.SelectMany(c => c.Vertices), p =>
        { Assert.InRange(p.X, 0, 160); Assert.InRange(p.Y, 0, 100); });
    }

    [Fact]
    public void InvalidGeometryAndCancellationAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiLayerPlan.Build(1, 257, 100, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiLayerPlan.Build(1, 32, float.NaN, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiLayerPlan.Build(1, 32, 100, 100, missingFraction: 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiLayerPlan.Build(1, 32, 100, 100, offset: new Vector2(float.NaN, 0)));
        Assert.Throws<OperationCanceledException>(() => VoronoiLayerPlan.Build(1, 32, 100, 100,
            cancellationToken: new CancellationToken(true)));
    }
}
