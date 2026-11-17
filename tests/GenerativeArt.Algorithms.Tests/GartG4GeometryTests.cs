using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class GartG4GeometryTests
{
    [Fact]
    public void OcellusPositionsAndDepthsAreSeededAndBounded()
    {
        var eyes = OcellusField.Generate(42, 512, 512, 13, 4, 1.6, .08, .02, .12);
        Assert.NotEmpty(eyes);
        Assert.Equal(eyes, OcellusField.Generate(42, 512, 512, 13, 4, 1.6, .08, .02, .12));
        Assert.False(eyes.SequenceEqual(OcellusField.Generate(43, 512, 512, 13, 4, 1.6, .08, .02, .12)));
        Assert.All(eyes, e => Assert.InRange(e.Depth, 4, 13));
        var radii = OcellusField.Radii(eyes, 512, 13, .011, 2.5);
        Assert.True(radii[0, 1] > radii[0, eyes[0].Depth]);
    }

    [Fact]
    public void OcellusSmoothUnionNeverExceedsHardMinimum()
    {
        Assert.Equal(-3, OcellusField.SmoothMin(-3, 7, 0));
        Assert.Equal(4, OcellusField.SmoothMin(4, 9, .5));
        Assert.True(OcellusField.SmoothMin(-2, -2, 1) < -2);
    }

    [Fact]
    public void OcellusPoreOnlyAppearsNearThreeOverlappingOuterBoundaries()
    {
        OcellusField.Eye[] eyes =
        [
            new(0, 0, 10, 13), new(30, 0, 10, 13), new(15, 26, 10, 13)
        ];
        var radii = new double[3, 14];
        for (int i = 0; i < 3; i++) radii[i, 1] = 10;
        Assert.Equal(1d, OcellusField.JunctionPore(eyes, radii, [10d, 10d, 10d], 8), 12);
        Assert.Equal(0, OcellusField.JunctionPore(eyes, radii, [10d, 10d, 50d], 8));
        Assert.Equal(0, OcellusField.JunctionPore(eyes, radii, [50d, 50d, 50d], 8));
        Assert.Equal(0, OcellusField.JunctionPore(eyes, radii, [10d, 10d, 10d], 0));
    }

    [Fact]
    public void OcellusSignedDistanceAndCoverageRemainEquivalent()
    {
        var eyes = OcellusField.Generate(42, 320, 240, 8, 3, 1.7, .08, 0, .1);
        var radii = OcellusField.Radii(eyes, 240, 13, .012, 2.5);
        var distances = eyes.Select(e => Math.Sqrt((170 - e.X) * (170 - e.X) +
            (120 - e.Y) * (120 - e.Y))).ToArray();
        for (int level = 1; level <= 13; level++)
        {
            double sdf = OcellusField.SignedDistance(eyes, radii, distances, level, 8);
            Assert.Equal(Math.Clamp(.5 - sdf, 0, 1),
                OcellusField.Coverage(eyes, radii, distances, level, 8), 12);
        }
    }

    [Fact]
    public void ReteNearestSurfaceMatchesBruteForceOnBucketBoundaries()
    {
        var field = ReteFoamField.Create(42, 320, 240, 130,
            .13, .004, .68, .3, 4, .08, .3, .85, .06);
        Assert.True(field.Cells.Count >= 2);
        foreach (int y in new[] { 0, 35, 79, 151, 239 })
        foreach (int x in new[] { 0, 19, 75, 173, 319 })
        {
            var expected = field.Cells.Select(c => Math.Sqrt((x - c.X) * (x - c.X) + (y - c.Y) * (y - c.Y)) - c.Radius)
                .OrderBy(d => d).Take(2).ToArray();
            var actual = field.Probe(x, y);
            Assert.Equal(expected[0], actual.First, 8);
            Assert.Equal(expected[1], actual.Second, 8);
        }
    }

    [Fact]
    public void RetePackingHasStableSeedsAndCanBeCancelled()
    {
        var a = ReteFoamField.Create(20, 192, 128, 80, .13, .004, .7, .3, 5, .08, .3, .85, .06);
        var b = ReteFoamField.Create(20, 192, 128, 80, .13, .004, .7, .3, 5, .08, .3, .85, .06);
        Assert.Equal(a.Cells.ToArray(), b.Cells.ToArray());
        var c = ReteFoamField.Create(21, 192, 128, 80, .13, .004, .7, .3, 5, .08, .3, .85, .06);
        Assert.NotEqual(a.Cells[0], c.Cells[0]);
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => ReteFoamField.Create(20, 192, 128, 80,
            .13, .004, .7, .3, 5, .08, .3, .85, .06, cancel.Token));
    }

    [Fact]
    public void ReteNodeIsStrongOnlyWhereThreeCellsMeet()
    {
        var triple = new ReteFoamField.NearestPair(-2, -2, -2, 10, 10);
        var farThird = new ReteFoamField.NearestPair(-2, -2, 40, 10, 10);
        Assert.Equal(1, ReteFoamField.JunctionIntensity(triple, 4));
        Assert.InRange(ReteFoamField.JunctionIntensity(farThird, 4), 0, 0.000001);
        Assert.Throws<ArgumentOutOfRangeException>(() => ReteFoamField.JunctionIntensity(triple, 0));
    }
}
