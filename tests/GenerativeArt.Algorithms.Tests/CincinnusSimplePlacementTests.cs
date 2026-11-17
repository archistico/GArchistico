using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class CincinnusSimplePlacementTests
{
    private static readonly CincinnusSimpleOptions Small = new()
    {
        Separation = 18, StepSize = 2, MinimumLength = 25,
        MaximumLength = 240, Margin = 0,
        MaximumLines = 64, MaximumPoints = 10_000,
        MaximumPendingSeeds = 20_000, MaximumCoverCells = 500
    };

    [Fact]
    public void StraightFieldCreatesSeparatedBidirectionalLines()
    {
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            180, 108, seed: 7, options: Small);

        Assert.True(layout.Lines.Count >= 3);
        Assert.False(layout.SafetyLimitReached);
        Assert.True(layout.AttemptedSeeds > 0);
        foreach (CincinnusStreamline line in layout.Lines)
        {
            Assert.True(line.Length >= Small.MinimumLength);
            Assert.Equal(line.Points.Count, line.Clearance.Count);
            Assert.True(line.Points.Count > 10);
            float referenceY = line.Points[0].Y;
            for (int i = 0; i < line.Points.Count; i++)
            {
                Vector2 p = line.Points[i];
                Assert.InRange(p.X, 0f, 180f);
                Assert.InRange(p.Y, 0f, 108f);
                Assert.InRange(p.Y, referenceY - .001f, referenceY + .001f);
                Assert.InRange(line.Clearance[i], Small.Separation * .5f - .01f, Small.Separation + .01f);
                Assert.InRange(line.Taper(i), 0f, 1f);
                if (i > 0) Assert.True(p.X > line.Points[i - 1].X);
            }
        }
    }

    [Fact]
    public void PlacementIsDeterministicAndChangesWithPlacementSeed()
    {
        IVectorField2D f = VectorFields.Constant(Vector2.UnitX);
        var first = CincinnusSimplePlacer.Place(f, 180, 108, 21, Small);
        var repeat = CincinnusSimplePlacer.Place(f, 180, 108, 21, Small);
        var different = CincinnusSimplePlacer.Place(f, 180, 108, 22, Small);

        Assert.Equal(first.Lines.Count, repeat.Lines.Count);
        Assert.Equal(first.AttemptedSeeds, repeat.AttemptedSeeds);
        Assert.Equal(first.SafetyLimitReached, repeat.SafetyLimitReached);
        for (int i = 0; i < first.Lines.Count; i++)
        {
            Assert.Equal(first.Lines[i].Points.ToArray(), repeat.Lines[i].Points.ToArray());
            Assert.Equal(first.Lines[i].Clearance.ToArray(), repeat.Lines[i].Clearance.ToArray());
        }
        bool identical = first.Lines.Count == different.Lines.Count;
        if (identical)
        {
            for (int i = 0; i < first.Lines.Count; i++)
                identical &= first.Lines[i].Points.SequenceEqual(different.Lines[i].Points);
        }
        Assert.False(identical);
    }

    [Fact]
    public void ShortRejectedLinesAreRolledBackWithoutHoldingSpace()
    {
        var options = Small with { MinimumLength = 200, MaximumLength = 200 };
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            90, 40, seed: 1, options: options);
        Assert.Empty(layout.Lines);
        Assert.True(layout.AttemptedSeeds > 1);
        Assert.False(layout.SafetyLimitReached);
    }

    [Fact]
    public void NullFieldProducesNoLinesAndTerminates()
    {
        IVectorField2D f = VectorFields.Constant(Vector2.Zero);
        var layout = CincinnusSimplePlacer.Place(f, 90, 60, 1, Small);
        Assert.Empty(layout.Lines);
        Assert.False(layout.SafetyLimitReached);
        Assert.True(layout.AttemptedSeeds > 1);
    }

    [Fact]
    public void SafetyLimitIsReportedInsteadOfSilentlyClaimingFullCoverage()
    {
        var limited = Small with { MaximumLines = 1 };
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            180, 108, 1, limited);
        Assert.Single(layout.Lines);
        Assert.True(layout.SafetyLimitReached);
    }

    [Fact]
    public void SequencesAreReadOnlyAndWidthsRemainInRange()
    {
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            180, 108, 1, Small);
        CincinnusStreamline line = layout.Lines[0];
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CincinnusStreamline>)layout.Lines)[0] = line);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Vector2>)line.Points)[0] = Vector2.Zero);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<float>)line.Clearance)[0] = 0f);
        Assert.Throws<ArgumentOutOfRangeException>(() => line.Taper(-1));
    }

    [Fact]
    public void RectangularGeneratedFieldIsRepeatableAndFinite()
    {
        var options = Small with
        {
            Separation = 25f, StepSize = 3f,
            MinimumLength = 20f, MaximumLength = 160f,
            Margin = 15f
        };
        var layout = CincinnusSimplePlacer.Generate(180, 80, 3, options);
        var repeat = CincinnusSimplePlacer.Generate(180, 80, 3, options);
        Assert.Equal(5, Assert.IsType<CincinnusFlowField>(layout.Field).Vortices.Count);
        Assert.Equal(layout.Lines.Count, repeat.Lines.Count);
        Assert.True(layout.Lines.Count > 0);
        for (int i = 0; i < layout.Lines.Count; i++)
        {
            Assert.Equal(layout.Lines[i].Points.ToArray(), repeat.Lines[i].Points.ToArray());
            Assert.All(layout.Lines[i].Points, p =>
            {
                Assert.True(float.IsFinite(p.X) && float.IsFinite(p.Y));
                Assert.InRange(p.X, -15f, 195f);
                Assert.InRange(p.Y, -15f, 95f);
            });
        }
    }

    [Fact]
    public void CircularFieldStopsOnSelfProximityInsteadOfLoopingForever()
    {
        var field = CincinnusFlowField.FromVortices([
            new CincinnusVortex(new Vector2(70, 70), -1, 500)
        ]);
        var options = Small with { Separation = 12, StepSize = 2,
            MaximumLength = 1000, MinimumLength = 20 };
        var layout = CincinnusSimplePlacer.Place(field, 140, 140, 3, options);
        Assert.NotEmpty(layout.Lines);
        Assert.All(layout.Lines, line =>
        {
            Assert.True(line.Length <= options.MaximumLength + .01f);
            Assert.True(line.Points.Count <= 650);
        });
    }

    [Fact]
    public void EqualSeparationAndTestDistanceKeepsTaperAtOne()
    {
        var options = Small with { TestDistance = Small.Separation };
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            180, 108, 1, options);
        Assert.NotEmpty(layout.Lines);
        Assert.All(layout.Lines, line =>
        {
            for (int i = 0; i < line.Points.Count; i++) Assert.Equal(1f, line.Taper(i));
        });
    }

    [Fact]
    public void PendingSeedBudgetProducesExplicitTruncationSignal()
    {
        var options = Small with { MaximumPendingSeeds = 1 };
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            180, 108, 1, options);
        Assert.NotEmpty(layout.Lines);
        Assert.True(layout.SafetyLimitReached);
    }

    [Fact]
    public void InvalidOptionsCannotCreateUnboundedGeometry()
    {
        IVectorField2D f = VectorFields.Constant(Vector2.UnitX);
        Assert.Throws<ArgumentNullException>(() => CincinnusSimplePlacer.Place(null!, 100, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { Separation = float.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { TestDistance = 100 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { TestDistance = 1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { MaximumLength = float.PositiveInfinity }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { Margin = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { MaximumPoints = 1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusSimplePlacer.Place(f, 100, 100,
            options: Small with { MaximumCoverCells = 1 }));
    }

    [Fact]
    public void CancelledTokenStopsBothPlacementAndGeneration()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => CincinnusSimplePlacer.Place(
            VectorFields.Constant(Vector2.UnitX), 200, 100,
            cancellationToken: cts.Token));
        Assert.Throws<OperationCanceledException>(() => CincinnusSimplePlacer.Generate(
            200, 100, cancellationToken: cts.Token));
    }
}
