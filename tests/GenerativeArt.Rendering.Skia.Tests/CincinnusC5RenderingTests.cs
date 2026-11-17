using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class CincinnusC5RenderingTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(ulong seed = 3, int width = 256, int height = 256,
        IReadOnlyDictionary<string, string>? overrides = null) =>
        ArtworkRecipe.Create(Catalog.Get("cincinnus-bristles"),
            new CanvasSize(width, height), seed, overrides);

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output);
        return output.ToArray();
    }

    [Fact]
    public void SimpleRendersDeterministicPngWithRealStrokes()
    {
        ArtworkRecipe recipe = Make();
        byte[] first = Png(recipe);
        Assert.Equal(first, Png(recipe));
        using SKBitmap bitmap = SKBitmap.Decode(first) ?? throw new InvalidOperationException();
        Assert.Equal(256, bitmap.Width);
        Assert.Equal(256, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 4);
    }

    [Fact]
    public void CustomPigmentsAndSeedChangeRaster()
    {
        ArtworkRecipe baseline = Make();
        byte[] standard = Png(baseline);
        byte[] custom = Png(Make(overrides: new Dictionary<string, string>
        {
            ["pigment-4"] = "#FFFF0088", ["background-color"] = "#FF102030",
            ["pigment-8"] = "#FF00FFFF"
        }));
        Assert.False(standard.AsSpan().SequenceEqual(custom));
        Assert.False(standard.AsSpan().SequenceEqual(Png(Make(seed: 5))));
    }

    [Fact]
    public void AdaptiveModeAcceptsJsonAndCanCreatePng()
    {
        ArtworkRecipe recipe = Make(seed: 4, overrides: new Dictionary<string, string>
        {
            ["mode"] = "adaptive", ["placement-attempts"] = "70",
            ["patch-attempts"] = "20", ["main-strokes"] = "10"
        });
        byte[] png = Png(recipe);
        using SKBitmap bitmap = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(256, bitmap.Width);
        Assert.Equal(256, bitmap.Height);
    }

    [Fact]
    public void RejectsOverBudgetBeforeAnyOutputAndHonorsCancellation()
    {
        ArtworkRecipe tooWide = Make(width: 2049, height: 256);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(tooWide));
        ArtworkRecipe tooManyPixels = Make(width: 2048, height: 2048);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(tooManyPixels));
        ArtworkRecipe fine = Make(width: 1024, height: 1024,
            overrides: new Dictionary<string, string> { ["render-quality"] = "fine-2x" });
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(fine));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(), output,
            cancellationToken: cancelled.Token));
        Assert.Equal(0L, output.Length);
    }

    [Theory]
    [InlineData("crimson", "#1F040A")]
    [InlineData("ocean", "#081724")]
    [InlineData("moss", "#142016")]
    public void AutoBackgroundPreviewMatchesThePainter(string palette, string expected)
    {
        SKColor[] pigments = Enumerable.Range(1, 8).Select(i => SKColor.Parse(
            ArtworkRolePalettePreview.SuggestHex("cincinnus-bristles", palette, $"pigment-{i}"))).ToArray();
        string background = ArtworkRolePalettePreview.SuggestHex("cincinnus-bristles", palette, "background-color");
        Assert.Equal(expected, background);
        using var surface = SKSurface.Create(new SKImageInfo(16, 16))!;
        CincinnusBristlePainter.Paint(surface.Canvas,
            Array.Empty<GenerativeArt.Algorithms.Cincinnus.CincinnusBrushStroke>(), 16, 16,
            options: new CincinnusPaintOptions { Palette = pigments });
        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        Assert.Equal(SKColor.Parse(background), bitmap.GetPixel(0, 0));
    }

    [Fact]
    public void AutoPreviewRejectsNonexistentPigmentsAndNonColorParameters()
    {
        foreach (string role in new[] { "pigment-0", "pigment-9", "pigment-01", "pigment-granulation" })
            Assert.Throws<ArgumentException>(() => ArtworkRolePalettePreview.SuggestHex("cincinnus-bristles", "crimson", role));
    }
}
