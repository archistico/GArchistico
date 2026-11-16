using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class VoronoiTessellationTests
{
    [Fact]
    public void OneSiteOccupiesEntireRectangleAndHasExactCentroid()
    {
        VoronoiCell cell = Assert.Single(VoronoiTessellation.Build(
            [new Vector2(3, 7)], 10, 20));
        Assert.Equal(4, cell.Vertices.Count);
        Assert.Equal(200, cell.Area, 6);
        Assert.True(Vector2.Distance(new Vector2(5, 10), cell.Centroid) < 1e-5f);
        Assert.Equal(new Vector2(3, 7), cell.Site);
    }

    [Fact]
    public void TwoSitesSplitRectangleAtPerpendicularBisector()
    {
        IReadOnlyList<VoronoiCell> cells = VoronoiTessellation.Build(
            [new Vector2(2, 5), new Vector2(8, 5)], 10, 10);
        Assert.Equal(2, cells.Count);
        Assert.Equal(50, cells[0].Area, 5);
        Assert.Equal(50, cells[1].Area, 5);
        Assert.All(cells[0].Vertices, v => Assert.True(v.X <= 5.00001f));
        Assert.All(cells[1].Vertices, v => Assert.True(v.X >= 4.99999f));
        Assert.True(Vector2.Distance(new Vector2(2.5f, 5), cells[0].Centroid) < 1e-5f);
        Assert.True(Vector2.Distance(new Vector2(7.5f, 5), cells[1].Centroid) < 1e-5f);
    }

    [Fact]
    public void CellsPartitionRectangleAndVerticesRespectNearestSite()
    {
        Vector2[] sites =
        [
            new(2, 3), new(7, 5), new(15, 2), new(2, 12), new(12, 15), new(18, 12)
        ];
        IReadOnlyList<VoronoiCell> cells = VoronoiTessellation.Build(sites, 20, 18);
        Assert.Equal(sites.Length, cells.Count);
        Assert.True(Math.Abs(cells.Sum(c => c.Area) - 360) < .0001);
        for (int i = 0; i < cells.Count; i++)
        {
            VoronoiCell cell = cells[i];
            Assert.True(cell.Vertices.Count >= 3);
            Assert.True(cell.Area > 0);
            foreach (Vector2 p in cell.Vertices)
            {
                Assert.InRange(p.X, 0, 20);
                Assert.InRange(p.Y, 0, 18);
                double own = Vector2.DistanceSquared(p, sites[i]);
                for (int j = 0; j < sites.Length; j++)
                    Assert.True(own <= Vector2.DistanceSquared(p, sites[j]) + .0002,
                        $"Vertice della cella {i} più vicino al sito {j}.");
            }
        }
    }

    [Fact]
    public void RelaxationMovesSitesToTheirCentroidsAndIsReproducible()
    {
        Vector2[] sites = [new(2, 5), new(8, 5)];
        Vector2[] relaxed = VoronoiTessellation.Relax(sites, 10, 10, 1).ToArray();
        Assert.True(Vector2.Distance(new Vector2(2.5f, 5), relaxed[0]) < 1e-5f);
        Assert.True(Vector2.Distance(new Vector2(7.5f, 5), relaxed[1]) < 1e-5f);
        Assert.Equal(relaxed, VoronoiTessellation.Relax(sites, 10, 10, 1).ToArray());
        Assert.Equal(sites, VoronoiTessellation.Relax(sites, 10, 10, 0).ToArray());
        Assert.Equal(new Vector2(2, 5), sites[0]); // non muta l'input
    }

    [Fact]
    public void DegenerateSitesAndInvalidBudgetsFailBeforeAllocating()
    {
        Assert.Throws<ArgumentException>(() => VoronoiTessellation.Build(
            [new Vector2(4, 4), new Vector2(4, 4)], 10, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiTessellation.Build(
            [new Vector2(float.NaN, 1)], 10, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiTessellation.Build(
            [new Vector2(10, 1)], 10, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiTessellation.Build(
            Array.Empty<Vector2>(), 10, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiTessellation.Build(
            Enumerable.Range(0, 257).Select(i => new Vector2(i + .1f, 1)).ToArray(), 300, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => VoronoiTessellation.Relax(
            [new Vector2(1, 1)], 10, 10, 5));
    }

    [Fact]
    public void CancellationIsCheckedBeforeTessellationAndRelaxation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => VoronoiTessellation.Build(
            [new Vector2(2, 3)], 10, 10, cts.Token));
        Assert.Throws<OperationCanceledException>(() => VoronoiTessellation.Relax(
            [new Vector2(2, 3)], 10, 10, 1, cts.Token));
    }
}
