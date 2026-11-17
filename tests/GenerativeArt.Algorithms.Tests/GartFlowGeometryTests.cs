using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class GartFlowGeometryTests
{
    [Fact]
    public void StableIntegrationFollowsStraightDrift()
    {
        var result = GartFlowGeometry2D.Trace(new Vector2(10, 25), 18, 2, 200, 100,
            p => GartFlowGeometry2D.Direction(p, 0, 1, [], 0));
        Assert.Equal(19, result.Count);
        Assert.All(result, p => Assert.Equal(25f, p.Y, 4));
        Assert.Equal(46f, result[^1].X, 4);
    }

    [Fact]
    public void WhirlContributesTangentialComponent()
    {
        Vector2 baseline = GartFlowGeometry2D.Direction(new Vector2(50, 40), 0, .3f, [], 0);
        Vector2 curved = GartFlowGeometry2D.Direction(new Vector2(50, 40), 0, .3f,
            [new GartFlowWhirl(new Vector2(40, 40), 2, 80)], 0);
        Assert.NotEqual(baseline, curved);
        Assert.InRange(curved.Length(), .999f, 1.001f);
    }

    [Fact]
    public void SpatialIndexRejectsClosePointsAndAcceptsDistantOnes()
    {
        var cells = new GartTrailOccupancy2D(12);
        cells.Deposit([new Vector2(10, 20), new Vector2(23, 20)]);
        Assert.True(cells.IsOccupied(new Vector2(12, 21), 6));
        Assert.True(cells.IsOccupied(new Vector2(23, 20), 12));
        Assert.False(cells.IsOccupied(new Vector2(80, 80), 12));
    }

    [Fact]
    public void IntegrationSupportsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => GartFlowGeometry2D.Trace(
            new Vector2(1, 1), 100, 1, 100, 100, _ => Vector2.UnitX, cts.Token));
    }
}
