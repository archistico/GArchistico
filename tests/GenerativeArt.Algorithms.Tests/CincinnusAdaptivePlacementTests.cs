using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Core.Randomness;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class CincinnusAdaptivePlacementTests
{
    private static readonly CincinnusAdaptiveOptions Small = new()
    {
        MainStrokeCount = 4, MaximumPlacementAttempts = 85, MaximumPatchAttempts = 24,
        CoverageCell = 12f, StepSize = 2f, MaximumTraceLength = 350f,
        Margin = 20f, MinimumRadius = 4f, MinimumAverageHalfWidth = 3f,
        MaximumHalfWidth = 20f, HalfWidthFactor = .3f,
        MaximumStrokeLength = 125f, MaximumSpinePoints = 30_000,
        MaximumTotalStrokes = 150, MaximumCoverageCells = 500
    };

    private static CincinnusAdaptiveLayout Sample(long seed = 7) =>
        CincinnusAdaptivePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            200, 128, seed, Small);

    [Fact]
    public void AdaptiveCreatesOrderedMainFillerAndPatchesWithFiniteSpines()
    {
        CincinnusAdaptiveLayout layout = Sample();
        Assert.NotEmpty(layout.Main);
        Assert.InRange(layout.Main.Count, 1, Small.MainStrokeCount);
        Assert.Equal(layout.Patches.Count + layout.Filler.Count + layout.Main.Count,
            layout.StrokesInPaintOrder.Count);
        Assert.Equal(layout.Main.Select(s => s.Seed),
            layout.StrokesInPaintOrder.TakeLast(layout.Main.Count).Select(s => s.Seed));
        Assert.True(layout.PlacementAttempts > 0);
        Assert.InRange(layout.PatchAttempts, 0, Small.MaximumPatchAttempts);
        Assert.True(layout.UncoveredCellsAfterPatches <= layout.UncoveredCellsBeforePatches);
        foreach (CincinnusBrushStroke s in layout.StrokesInPaintOrder)
        {
            Assert.True(s.Spine.Count >= 20);
            Assert.True(s.Spine.All(p => float.IsFinite(p.Position.X) && float.IsFinite(p.Position.Y)));
            Assert.True(s.Spine.All(p => p.HalfWidth > 0 && float.IsFinite(p.HalfWidth)));
            Assert.True(s.Length > 0);
        }
    }

    [Fact]
    public void SameSeedAndFieldReproduceGeometryAndDifferentSeedsChangeIt()
    {
        CincinnusAdaptiveLayout a = Sample(7);
        CincinnusAdaptiveLayout b = Sample(7);
        CincinnusAdaptiveLayout other = Sample(8);
        Assert.Equal(a.PlacementAttempts, b.PlacementAttempts);
        Assert.Equal(a.UncoveredCellsAfterPatches, b.UncoveredCellsAfterPatches);
        Assert.Equal(a.Main.Select(s => s.Seed), b.Main.Select(s => s.Seed));
        Assert.Equal(a.StrokesInPaintOrder.Count, b.StrokesInPaintOrder.Count);
        for (int i = 0; i < a.StrokesInPaintOrder.Count; i++)
            Assert.Equal(a.StrokesInPaintOrder[i].Spine.ToArray(), b.StrokesInPaintOrder[i].Spine.ToArray());
        Assert.True(a.StrokesInPaintOrder.Count != other.StrokesInPaintOrder.Count ||
            a.StrokesInPaintOrder.Zip(other.StrokesInPaintOrder, (x, y) =>
                x.Seed != y.Seed || x.Spine[0].Position != y.Spine[0].Position).Any(d => d));
    }

    [Fact]
    public void CoverageGridCountsEachStrokeOnlyOncePerCellAndPickTargetsLeastCovered()
    {
        var grid = new CincinnusCoverageGrid(200, 128, 12f);
        Assert.Equal(grid.CellCount, grid.UncoveredCellCount);
        CincinnusBrushStroke stroke = Sample().StrokesInPaintOrder.OrderByDescending(s => s.Length).First();
        grid.Add(stroke);
        int bare = grid.UncoveredCellCount;
        Assert.True(bare < grid.CellCount);
        grid.Add(stroke);
        Assert.Equal(bare, grid.UncoveredCellCount);
        var rnd = new SplitMix64Random(4);
        for (int i = 0; i < 50; i++)
        {
            Vector2 p = grid.Pick(rnd);
            Assert.InRange(p.X, 0, 200);
            Assert.InRange(p.Y, 0, 128);
        }
        grid.Mark(new Vector2(0, 0));
        Assert.True(grid.GetCount(0, 0) >= 1);
    }

    [Fact]
    public void ExhaustedBudgetIsReportedAndNoInfiniteLoopOnZeroField()
    {
        CincinnusAdaptiveLayout limited = CincinnusAdaptivePlacer.Place(
            VectorFields.Constant(Vector2.Zero), 200, 128, 3,
            Small with { MaximumPlacementAttempts = 2, MaximumPatchAttempts = 0 });
        Assert.Empty(limited.StrokesInPaintOrder);
        Assert.Equal(2, limited.PlacementAttempts);
        Assert.True(limited.SafetyLimitReached);
        Assert.Equal(limited.CoverageCells, limited.UncoveredCellsAfterPatches);
    }

    [Fact]
    public void RectangleAndExplicitVorticesAreSupported()
    {
        var flow = CincinnusFlowField.FromVortices(new[]
        {
            new CincinnusVortex(new Vector2(-80, 30), -1f, 120f),
            new CincinnusVortex(new Vector2(230, 150), -.8f, 110f)
        });
        CincinnusAdaptiveLayout result = CincinnusAdaptivePlacer.Place(flow, 200, 128,
            11, Small with { MaximumPlacementAttempts = 12, MaximumPatchAttempts = 5 });
        Assert.InRange(result.PlacementAttempts, 1, 12);
        Assert.True(result.CoverageCells > 0);
    }

    [Fact]
    public void InvalidParametersAndCancellationAreRejectedBeforeWork()
    {
        IVectorField2D f = VectorFields.Constant(Vector2.UnitX);
        Assert.Throws<ArgumentNullException>(() => CincinnusAdaptivePlacer.Place(null!, 100, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusAdaptivePlacer.Place(f, 100, 100,
            options: Small with { CoverageCell = float.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusAdaptivePlacer.Place(f, 100, 100,
            options: Small with { MaximumCoverageCells = 4 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusAdaptivePlacer.Place(f, 100, 100,
            options: Small with { StepSize = .01f }));
        Assert.Throws<OperationCanceledException>(() => CincinnusAdaptivePlacer.Place(f, 100, 100,
            options: Small, cancellationToken: new CancellationToken(true)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CincinnusCoverageGrid(100, 100, .01f, 100));
    }

    [Fact]
    public void OutputCollectionsCannotBeMutated()
    {
        CincinnusAdaptiveLayout l = Sample();
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CincinnusBrushStroke>)l.StrokesInPaintOrder)[0] = null!);
    }
}
