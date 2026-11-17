using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class CincinnusStrokePlannerTests
{
    private static CincinnusSimpleLayout Sample(long seed = 5) => CincinnusSimplePlacer.Place(
        VectorFields.Constant(Vector2.UnitX), 160, 100, seed, new CincinnusSimpleOptions
        {
            Separation = 20, StepSize = 2, MinimumLength = 26, MaximumLength = 320,
            Margin = 0, MaximumLines = 128, MaximumPoints = 25_000,
            MaximumPendingSeeds = 25_000, MaximumCoverCells = 1_000
        });

    private static CincinnusStrokeOptions Short => new()
    {
        FirstMinimumLength = 20, SubsequentMinimumLength = 35,
        MaximumStrokeLength = 75, MinimumOverlap = 10, MaximumOverlap = 18,
        MaximumStrokes = 1_000, MaximumSpinePoints = 50_000
    };

    [Fact]
    public void SimpleStreamlinesBecomeFiniteStrokesSortedByCurvature()
    {
        CincinnusStrokePlan result = CincinnusStrokePlanner.Build(Sample(), 17, Short);
        Assert.NotEmpty(result.Strokes);
        Assert.False(result.SafetyLimitReached);
        Assert.Equal(result.Strokes.Sum(x => x.Spine.Count), result.SpinePointCount);
        float lastBend = -1;
        foreach (CincinnusBrushStroke stroke in result.Strokes)
        {
            Assert.True(stroke.Length > 0);
            Assert.InRange(stroke.Bend, 0f, 1f);
            Assert.True(stroke.Bend >= lastBend);
            lastBend = stroke.Bend;
            Assert.Equal(1f, stroke.Load(0));
            Assert.Equal(0f, stroke.Load(1));
            Assert.InRange(stroke.Load(.5f), .01f, 1f);
            Assert.True(stroke.Spine.Count >= 16);
            Assert.InRange(stroke.LandingOffset(-1), 0f, 160f);
            Assert.InRange(stroke.LandingOffset(1), 0f, 160f);
            for (int k = 0; k < stroke.Spine.Count; k++)
            {
                CincinnusSpineSample s = stroke.Spine[k];
                Assert.True(float.IsFinite(s.Position.X) && float.IsFinite(s.Position.Y));
                Assert.True(float.IsFinite(s.Normal.X) && float.IsFinite(s.Normal.Y));
                Assert.InRange(s.Normal.Length(), .99f, 1.01f);
                Assert.True(float.IsFinite(s.Arc));
                Assert.InRange(s.HalfWidth, .05f, 20_000f);
                if (k > 0) Assert.True(s.Arc > stroke.Spine[k - 1].Arc);
            }
        }
    }

    [Fact]
    public void SameSeedCreatesIdenticalSamplesHeadProfilesAndOrder()
    {
        CincinnusSimpleLayout lines = Sample();
        CincinnusStrokePlan first = CincinnusStrokePlanner.Build(lines, 21, Short);
        CincinnusStrokePlan again = CincinnusStrokePlanner.Build(lines, 21, Short);
        CincinnusStrokePlan other = CincinnusStrokePlanner.Build(lines, 22, Short);
        Assert.Equal(first.Strokes.Count, again.Strokes.Count);
        for (int i = 0; i < first.Strokes.Count; i++)
        {
            Assert.Equal(first.Strokes[i].Seed, again.Strokes[i].Seed);
            Assert.Equal(first.Strokes[i].Spine.ToArray(), again.Strokes[i].Spine.ToArray());
            Assert.Equal(first.Strokes[i].LandingOffset(-.7f), again.Strokes[i].LandingOffset(-.7f));
            Assert.Equal(first.Strokes[i].LandingOffset(.35f), again.Strokes[i].LandingOffset(.35f));
        }
        Assert.True(first.Strokes.Count != other.Strokes.Count ||
            first.Strokes.Zip(other.Strokes, (a, b) => a.Seed != b.Seed).Any(x => x));
    }

    [Fact]
    public void TestatesDifferBySeedAndAreNonNegative()
    {
        CincinnusBrushStroke a = CincinnusStrokePlanner.Build(Sample(), 3, Short).Strokes[0];
        CincinnusBrushStroke b = CincinnusStrokePlanner.Build(Sample(), 4, Short).Strokes[0];
        for (int i = 0; i <= 128; i++)
        {
            float t = -1f + 2f * i / 128;
            Assert.True(a.LandingOffset(t) >= 0f);
        }
        Assert.NotEqual(a.Seed, b.Seed);
        Assert.Throws<ArgumentOutOfRangeException>(() => a.LandingOffset(float.NaN));
    }

    [Fact]
    public void BudgetIsReportedWithoutClaimingCompletePlan()
    {
        CincinnusStrokePlan limited = CincinnusStrokePlanner.Build(Sample(), 1,
            Short with { MaximumStrokes = 1 });
        Assert.Single(limited.Strokes);
        Assert.True(limited.SafetyLimitReached);
    }

    [Fact]
    public void InvalidOptionsAreRejectedAndCancelledWorkDoesNotStart()
    {
        CincinnusSimpleLayout lines = Sample();
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusStrokePlanner.Build(lines,
            options: Short with { HalfWidthFactor = float.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusStrokePlanner.Build(lines,
            options: Short with { MaximumOverlap = 75 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusStrokePlanner.Build(lines,
            options: Short with { DryExponent = 0 }));
        Assert.Throws<OperationCanceledException>(() => CincinnusStrokePlanner.Build(lines,
            cancellationToken: new CancellationToken(true)));
        Assert.Throws<ArgumentNullException>(() => CincinnusStrokePlanner.Build((CincinnusSimpleLayout)null!));
    }

    [Fact]
    public void SpineSamplesAreNotMutableThroughPublicApi()
    {
        CincinnusBrushStroke stroke = CincinnusStrokePlanner.Build(Sample(), options: Short).Strokes[0];
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CincinnusSpineSample>)stroke.Spine)[0] = default);
    }
}

