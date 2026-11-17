using System.Numerics;
using GenerativeArt.Algorithms.Brushes;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class GartBrushGeometryTests
{
    [Fact]
    public void BristleSpineKeepsUnitNormalsAndPointedEnds()
    {
        var spine = BristleSpine2D.Build(new Vector2(120, 90), new Vector2(90, 90),
            180, 2, 40, 1.1, .6, 1.7);
        Assert.Equal(180, spine.Length);
        Assert.All(spine, point =>
        {
            Assert.InRange(point.Normal.Length(), .999f, 1.001f);
            Assert.True(float.IsFinite(point.Position.X));
            Assert.True(float.IsFinite(point.Position.Y));
            Assert.True(point.HalfWidth > 0);
        });
        Assert.True(spine[90].HalfWidth > spine[0].HalfWidth * 5);
        Assert.True(spine[90].HalfWidth > spine[^1].HalfWidth * 5);
        Assert.Equal(spine, BristleSpine2D.Build(new Vector2(120, 90), new Vector2(90, 90),
            180, 2, 40, 1.1, .6, 1.7));
    }

    [Fact]
    public void SpiralStampIsDeterministicAndChangingPhaseChangesPosition()
    {
        var first = SpiralStamp2D.Points(new Vector2(30, 50), 18, 24, .5, 3);
        Assert.Equal(first, SpiralStamp2D.Points(new Vector2(30, 50), 18, 24, .5, 3));
        Assert.NotEqual(first[0], SpiralStamp2D.Points(new Vector2(30, 50), 18, 24, 1.5, 3)[0]);
        Assert.All(first, p => Assert.InRange(Vector2.Distance(p, new Vector2(30, 50)), 0f, 18f));
    }

    [Fact]
    public void InvalidBrushGeometryRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BristleSpine2D.Build(
            Vector2.Zero, Vector2.Zero, 1, 2, 10, .5, .5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpiralStamp2D.Points(
            Vector2.Zero, 0, 24, 0, 3));
    }
}
