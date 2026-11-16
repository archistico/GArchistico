using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class PigmentSdfTests
{
    [Theory]
    [InlineData(PigmentSdfLayout.Cluster)]
    [InlineData(PigmentSdfLayout.Orbital)]
    [InlineData(PigmentSdfLayout.River)]
    public void FieldIsSeededWithNormalizedNonnegativeColorWeights(PigmentSdfLayout layout)
    {
        var options = new PigmentSdfOptions { Layout = layout };
        var a = new PigmentSdfField(42, new Vector2(1.6f, 1), options);
        var b = new PigmentSdfField(42, new Vector2(1.6f, 1), options);
        var c = new PigmentSdfField(43, new Vector2(1.6f, 1), options);
        bool seedChanges = false, mixed = false;
        for (int y = -8; y <= 8; y++)
            for (int x = -10; x <= 10; x++)
            {
                Vector2 p = new(x / 12f, y / 12f);
                var sample = a.Sample(p); Assert.Equal(sample, b.Sample(p));
                seedChanges |= sample != c.Sample(p);
                Assert.True(float.IsFinite(sample.Distance));
                Vector4 w = sample.ColorWeights;
                Assert.InRange(w.X, 0, 1); Assert.InRange(w.Y, 0, 1); Assert.InRange(w.Z, 0, 1); Assert.InRange(w.W, 0, 1);
                Assert.InRange(w.X + w.Y + w.Z + w.W, .99999f, 1.00001f);
                mixed |= w.X is > .05f and < .95f && w.Y > .05f;
            }
        Assert.True(seedChanges); Assert.True(mixed);
    }

    [Fact]
    public void ColorFusionDoesNotMoveGeometryAndLargerFusionExpandsTheUnion()
    {
        var options = new PigmentSdfOptions { Warp = 0 };
        var sharp = new PigmentSdfField(12, Vector2.One, options with { ColorMix = 0 });
        var mixed = new PigmentSdfField(12, Vector2.One, options with { ColorMix = 1 });
        var fused = new PigmentSdfField(12, Vector2.One, options with { Fusion = .2f, ColorMix = 1 });
        bool changedColors = false;
        for (int i = 0; i < 101; i++)
        {
            Vector2 p = new((i - 50) / 100f, .07f);
            var a = sharp.Sample(p); var b = mixed.Sample(p);
            Assert.Equal(a.Distance, b.Distance); changedColors |= a.ColorWeights != b.ColorWeights;
            Assert.True(fused.Sample(p).Distance <= b.Distance + 1e-6f);
        }
        Assert.True(changedColors);
    }

    [Fact]
    public void InvalidOptionsAndCoordinatesAreRejected()
    {
        PigmentSdfOptions[] invalid = [new() { Blobs = 41 }, new() { Radius = float.NaN },
            new() { Warp = float.PositiveInfinity }, new() { Elongation = .5f }, new() { Fusion = 0 },
            new() { ColorMix = -1 }, new() { Layout = (PigmentSdfLayout)99 }];
        foreach (var options in invalid)
            Assert.Throws<ArgumentOutOfRangeException>(() => new PigmentSdfField(1, Vector2.One, options));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PigmentSdfField(1, new Vector2(0, 1)));
        var field = new PigmentSdfField(1, Vector2.One);
        Assert.Throws<ArgumentOutOfRangeException>(() => field.Sample(new Vector2(float.NaN, 0)));
    }

    [Fact]
    public void EdgeAndSubpixelStrokeHaveKnownCoverage()
    {
        Assert.Equal(.5f, SdfPixelCoverage.Fill(0, 1));
        Assert.Equal(0, SdfPixelCoverage.Fill(1, 1));
        Assert.Equal(1, SdfPixelCoverage.Fill(-1, 1));
        Assert.Equal(.1f, SdfPixelCoverage.Stroke(0, .1f, 1), 6);
        Assert.Equal(.12f, SdfPixelCoverage.Stroke(.48f, .2f, 1), 6);
        Assert.Equal(0, SdfPixelCoverage.Stroke(0, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SdfPixelCoverage.Fill(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SdfPixelCoverage.Stroke(0, -1, 1));
    }

    [Fact]
    public void ThinStrokeConservesEnergyWhileItMovesAcrossPixels()
    {
        const float width = .37f;
        for (int shift = -49; shift <= 49; shift++)
        {
            float total = 0;
            for (int x = -3; x <= 3; x++) total += SdfPixelCoverage.Stroke(x - shift / 100f, width, 1);
            Assert.InRange(total, width - .00001f, width + .00001f);
        }
    }
}
