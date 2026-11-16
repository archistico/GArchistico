using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

/// <summary>Regressioni sul raster 2x, PNG/alpha, JSON e budget pre-allocazione.</summary>
public sealed class RasterQualityTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Recipe(string id, string? quality = null,
        int width = 128, int height = 96, RecipeAnimation? animation = null)
    {
        Dictionary<string, string>? p = quality is null ? null :
            new Dictionary<string, string> { ["render-quality"] = quality };
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), 37, p, animation);
    }

    private static byte[] Render(ArtworkRecipe recipe, int? frame = null)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output, frame);
        Assert.True(output.CanWrite);
        return output.ToArray();
    }

    [Theory]
    [InlineData("flow-brush")]
    [InlineData("strata-landscape")]
    [InlineData("guilloche")]
    [InlineData("tonal-engraving")]
    [InlineData("domain-warp")]
    [InlineData("lic-flow")]
    [InlineData("parametric-curves")]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("looping-flow")]
    [InlineData("plasma-noise")]
    [InlineData("sdf-orbits")]
    [InlineData("poisson-stippling")]
    [InlineData("voronoi-mosaic")]
    [InlineData("topographic-contours")]
    public void StandardModeIsDefaultAndFineModePreservesOutputDimensions(string id)
    {
        var animation = id == "looping-flow" ? new RecipeAnimation(4, 18) : null;
        int? frame = animation is null ? null : 0;
        ArtworkRecipe legacy = Recipe(id, animation: animation);
        ArtworkRecipe standard = Recipe(id, "standard", animation: animation);
        ArtworkRecipe fine = Recipe(id, "fine-2x", animation: animation);
        Assert.Equal("standard", legacy.Parameters["render-quality"]);
        Assert.Equal(Render(legacy, frame), Render(standard, frame));
        byte[] high = Render(fine, frame);
        Assert.Equal(high, Render(fine, frame));
        Assert.False(high.AsSpan().SequenceEqual(Render(standard, frame)));
        using SKBitmap decoded = SKBitmap.Decode(high) ?? throw new InvalidOperationException();
        Assert.Equal(128, decoded.Width);
        Assert.Equal(96, decoded.Height);
    }

    [Fact]
    public void FineModeSurvivesJsonAndLegacyJsonGetsStandard()
    {
        ArtworkRecipe fine = Recipe("plasma-noise", "fine-2x");
        byte[] json = ArtworkRecipeJson.Serialize(fine, Catalog);
        Assert.Equal("fine-2x", ArtworkRecipeJson.Deserialize(json, Catalog).Parameters["render-quality"]);
        // Compatibilità con JSON v1 emesso prima dell'introduzione del campionamento.
        const string legacy = """
            {"formatVersion":1,"artworkId":"plasma-noise","algorithmVersion":1,
             "seed":"37","width":128,"height":96,
             "parameters":{"frequency":0.016,"octaves":4,"palette":"deep"}}
            """;
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(legacy, Catalog);
        Assert.Equal("standard", loaded.Parameters["render-quality"]);
        Assert.Equal(Render(Recipe("plasma-noise")), Render(loaded));
        Assert.Throws<ArgumentException>(() => Recipe("plasma-noise", "fine-3x"));
    }

    [Fact]
    public void FullHd2xAllowedButExcessPixelsRejectedBeforeWriting()
    {
        Engine.Validate(Recipe("parametric-curves", "fine-2x", 1920, 1080));
        Engine.Validate(Recipe("parametric-curves", "fine-2x", 2048, 2048));
        using var output = new MemoryStream();
        var tooLarge = Recipe("parametric-curves", "fine-2x", 2049, 2048);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(tooLarge, output));
        Assert.Empty(output.ToArray());
        Engine.Validate(Recipe("parametric-curves", "standard", 4000, 4000));
    }

    [Fact]
    public void TransparencyAndColorArePreservedInPremultipliedDownsampling()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(64, 64), rasterScale: 2);
        surface.Clear(Color32.Transparent);
        surface.DrawCircle(32, 32, 12, new Color32(128, 240, 50, 35), fill: true);
        using var output = new MemoryStream();
        surface.WritePng(output);
        using SKBitmap bitmap = SKBitmap.Decode(output.ToArray()) ?? throw new InvalidOperationException();
        SKColor center = bitmap.GetPixel(32, 32);
        Assert.InRange(center.Alpha, (byte)127, (byte)129);
        Assert.InRange(center.Red, (byte)238, (byte)242);
        Assert.InRange(center.Green, (byte)48, (byte)52);
        Assert.Equal((byte)0, bitmap.GetPixel(0, 0).Alpha);
        // Gli edge pixel sono mediati in premultiplied alpha, non su RGB sporchi.
        Assert.Contains(Enumerable.Range(16, 32).SelectMany(y => Enumerable.Range(16, 32)
            .Select(x => bitmap.GetPixel(x, y).Alpha)), a => a > 0 && a < 128);
    }

    [Fact]
    public void PixelNativeBufferMustMatchPhysicalDimensions()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(8, 8), rasterScale: 2);
        using var output = new MemoryStream();
        Assert.Throws<ArgumentException>(() => surface.DrawPixels(
            new GenerativeArt.Core.Pixels.PixelBuffer(new CanvasSize(8, 8)), nativeResolution: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SkiaRasterSurface(new CanvasSize(8, 8), 3));
        surface.WritePng(output);
        Assert.NotEmpty(output.ToArray());
    }

    [Fact]
    public void LoopFineModeUsesSameFrameRulesAndDeterministicFrames()
    {
        var animation = new RecipeAnimation(4, 18);
        ArtworkRecipe fine = Recipe("looping-flow", "fine-2x", 128, 96, animation);
        Assert.Equal(Render(fine, 0), Render(fine, 0));
        Assert.False(Render(fine, 0).AsSpan().SequenceEqual(Render(fine, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(fine, 4));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        using var destination = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(fine, destination, 0, cancelled.Token));
        Assert.Equal(0, destination.Length);
    }
}
