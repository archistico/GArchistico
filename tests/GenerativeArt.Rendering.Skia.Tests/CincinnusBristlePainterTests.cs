using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class CincinnusBristlePainterTests
{
    private static CincinnusStrokePlan Plan()
    {
        var layout = CincinnusSimplePlacer.Place(VectorFields.Constant(Vector2.UnitX),
            128, 88, 31, new CincinnusSimpleOptions
            {
                Separation = 20, StepSize = 2, MinimumLength = 22, MaximumLength = 180,
                Margin = 0, MaximumLines = 64, MaximumPoints = 10_000,
                MaximumPendingSeeds = 10_000, MaximumCoverCells = 300
            });
        return CincinnusStrokePlanner.Build(layout, 31, new CincinnusStrokeOptions
        {
            FirstMinimumLength = 20, SubsequentMinimumLength = 30,
            MaximumStrokeLength = 70, MinimumOverlap = 9, MaximumOverlap = 13
        });
    }

    private static byte[] Render(CincinnusStrokePlan plan, long seed = 31,
        CincinnusPaintOptions? options = null)
    {
        using var surface = SKSurface.Create(new SKImageInfo(128, 88,
            SKColorType.Bgra8888, SKAlphaType.Premul))
            ?? throw new InvalidOperationException("Skia non disponibile.");
        CincinnusBristlePainter.Paint(surface.Canvas, plan.Strokes, 128, 88, seed,
            options ?? new CincinnusPaintOptions
            {
                BristlePitch = 9, WideShadowBlur = 2,
                TightShadowBlur = 1, DotCount = 0
            });
        using SKImage image = surface.Snapshot();
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Codifica PNG fallita.");
        return data.ToArray();
    }

    [Fact]
    public void PaintGeneratesValidImageWithVisibleStrokes()
    {
        CincinnusStrokePlan plan = Plan();
        Assert.NotEmpty(plan.Strokes);
        byte[] png = Render(plan);
        Assert.True(png.Length > 100);
        using SKBitmap bitmap = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("Decodifica PNG fallita.");
        Assert.Equal(128, bitmap.Width);
        Assert.Equal(88, bitmap.Height);
        Assert.Contains(Enumerable.Range(1, bitmap.Height - 2), y =>
            Enumerable.Range(1, bitmap.Width - 2).Any(x =>
                !bitmap.GetPixel(x, y).Equals(bitmap.GetPixel(0, 0))));
    }

    [Fact]
    public void SamePlanAndOptionsAreBitIdenticalWithinSkiaRuntime()
    {
        CincinnusStrokePlan plan = Plan();
        Assert.Equal(Render(plan), Render(plan));
    }

    [Fact]
    public void PaletteAndBristlePitchAffectPixels()
    {
        CincinnusStrokePlan plan = Plan();
        var defaults = new CincinnusPaintOptions
        {
            BristlePitch = 9, WideShadowBlur = 0,
            TightShadowBlur = 0, DotCount = 0
        };
        byte[] baseImage = Render(plan, options: defaults);
        byte[] bright = Render(plan, options: defaults with
        {
            Palette = new[] { SKColors.Blue, SKColors.Yellow }
        });
        byte[] fine = Render(plan, options: defaults with { BristlePitch = 3 });
        Assert.False(baseImage.AsSpan().SequenceEqual(bright));
        Assert.False(baseImage.AsSpan().SequenceEqual(fine));
    }

    [Fact]
    public void InvalidOptionsAndCancellationAreReported()
    {
        CincinnusStrokePlan plan = Plan();
        using var surface = SKSurface.Create(new SKImageInfo(128, 88))!;
        Assert.Throws<ArgumentOutOfRangeException>(() => CincinnusBristlePainter.Paint(
            surface.Canvas, plan.Strokes, 128, 88, options: new CincinnusPaintOptions
            { BristlePitch = 0 }));
        Assert.Throws<ArgumentException>(() => CincinnusBristlePainter.Paint(
            surface.Canvas, plan.Strokes, 128, 88, options: new CincinnusPaintOptions
            { Palette = new[] { SKColors.White } }));
        Assert.Throws<OperationCanceledException>(() => CincinnusBristlePainter.Paint(
            surface.Canvas, plan.Strokes, 128, 88, cancellationToken: new CancellationToken(true)));
        Assert.Throws<ArgumentNullException>(() => CincinnusBristlePainter.Paint(null!,
            plan.Strokes, 128, 88));
    }
}
