using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ArtisticPostProcessorTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static PixelBuffer Gradient(int width = 32, int height = 24)
    {
        var pixels = new PixelBuffer(new CanvasSize(width, height));
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels.SetPixel(x, y, new Color32(128, (byte)(x * 255 / (width - 1)),
                    (byte)(y * 255 / (height - 1)), 133));
        return pixels;
    }

    private static uint[] Pixels(PixelBuffer pixels) =>
        Enumerable.Range(0, pixels.Height)
            .SelectMany(y => pixels.GetPackedArgbRow(y).ToArray()).ToArray();

    [Fact]
    public void DisabledEffectsArePixelIdenticalAndDoNotChangeAlpha()
    {
        PixelBuffer pixels = Gradient();
        uint[] before = Pixels(pixels);
        var options = new ArtisticFinishOptions(0, 1, "none", 8);
        ArtisticPostProcessor.Apply(pixels, options, 42);
        Assert.Equal(before, Pixels(pixels));
        Assert.Throws<ArgumentOutOfRangeException>(() => (options with { GrainAmount = double.NaN }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => (options with { GrainSize = 0 }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => (options with { DitherLevels = 1 }).Validate());
        Assert.Throws<ArgumentException>(() => (options with { DitherMode = "random" }).Validate());
    }

    [Fact]
    public void GrainUsesSeedAndCoordinatesAndIsRepeatable()
    {
        var options = new ArtisticFinishOptions(.14, 3, "none", 8);
        PixelBuffer first = Gradient(48, 36);
        PixelBuffer second = Gradient(48, 36);
        PixelBuffer changedSeed = Gradient(48, 36);
        ArtisticPostProcessor.Apply(first, options, 1234);
        ArtisticPostProcessor.Apply(second, options, 1234);
        ArtisticPostProcessor.Apply(changedSeed, options, 1235);
        Assert.Equal(Pixels(first), Pixels(second));
        Assert.False(Pixels(first).AsSpan().SequenceEqual(Pixels(changedSeed)));
        Assert.False(Pixels(Gradient(48, 36)).AsSpan().SequenceEqual(Pixels(first)));
        // Le 3x3 celle della grana condividono un valore: stesso input -> stesso output.
        var uniform = new PixelBuffer(new CanvasSize(9, 9));
        uniform.Clear(Color32.Opaque(128, 128, 128));
        ArtisticPostProcessor.Apply(uniform, options, 5);
        Assert.Equal(uniform.GetPixel(0, 0), uniform.GetPixel(2, 2));
        Assert.Equal(uniform.GetPixel(3, 3), uniform.GetPixel(5, 5));
    }

    [Theory]
    [InlineData("bayer4", 4)]
    [InlineData("bayer8", 8)]
    public void OrderedDitherHasFiniteLevelsAndLeavesTransparencyUntouched(string mode, int period)
    {
        PixelBuffer pixels = Gradient(64, 48);
        pixels.SetPixel(0, 0, new Color32(0, 250, 18, 77));
        var options = new ArtisticFinishOptions(0, 1, mode, 2);
        ArtisticPostProcessor.Apply(pixels, options, 42);
        Assert.Equal(new Color32(0, 250, 18, 77), pixels.GetPixel(0, 0));
        for (int y = 0; y < pixels.Height; y++)
            for (int x = 0; x < pixels.Width; x++)
            {
                Color32 c = pixels.GetPixel(x, y);
                if (c.A == 0) continue;
                Assert.Equal((byte)128, c.A);
                Assert.Contains(c.R, new byte[] { 0, 255 });
                Assert.Contains(c.G, new byte[] { 0, 255 });
                Assert.Contains(c.B, new byte[] { 0, 255 });
            }
        // Il pattern spaziale si ripete al periodo previsto.
        var uniform = new PixelBuffer(new CanvasSize(period * 2, period * 2));
        uniform.Clear(Color32.Opaque(128, 128, 128));
        ArtisticPostProcessor.Apply(uniform, options, 999);
        Assert.Equal(uniform.GetPixel(0, 0), uniform.GetPixel(period, period));
    }

    [Fact]
    public void CancelledPostprocessDoesNotTouchPixelData()
    {
        PixelBuffer pixels = Gradient();
        uint[] before = Pixels(pixels);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => ArtisticPostProcessor.Apply(
            pixels, new ArtisticFinishOptions(.1, 1, "bayer4", 6), 42, cancelled.Token));
        Assert.Equal(before, Pixels(pixels));
    }

    [Fact]
    public void OptionalFinishingParametersRoundTripWithoutSchemaBump()
    {
        var id = "plasma-noise";
        ArtworkRecipe changed = ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(128, 96), 7,
            new Dictionary<string, string> { ["grain-amount"] = "0.12", ["grain-size"] = "3",
                ["dither-mode"] = "bayer8", ["dither-levels"] = "5", ["render-quality"] = "fine-2x" });
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(changed, Catalog), Catalog);
        Assert.Equal(1, loaded.FormatVersion);
        Assert.Equal(changed.Parameters.OrderBy(p => p.Key), loaded.Parameters.OrderBy(p => p.Key));
        Assert.Equal(new ArtisticFinishOptions(.12, 3, "bayer8", 5),
            ArtisticFinishOptions.FromRecipe(loaded));
        const string oldRecipe = """
            {"formatVersion":1,"artworkId":"plasma-noise","algorithmVersion":1,
             "seed":"7","width":128,"height":96,
             "parameters":{"frequency":0.016,"octaves":4,"palette":"deep"}}
            """;
        ArtworkRecipe legacy = ArtworkRecipeJson.Deserialize(oldRecipe, Catalog);
        Assert.False(ArtisticFinishOptions.FromRecipe(legacy).IsEnabled);
        Assert.Equal("none", legacy.Parameters["dither-mode"]);
        Assert.Equal("0", legacy.Parameters["grain-amount"]);
    }

    [Theory]
    [InlineData("parametric-curves")]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("looping-flow")]
    [InlineData("plasma-noise")]
    [InlineData("sdf-orbits")]
    [InlineData("poisson-stippling")]
    [InlineData("voronoi-mosaic")]
    [InlineData("topographic-contours")]
    public void FinishingIsAvailableForAllNineArtworksAndDoesChangeFinalPng(string id)
    {
        var animation = id == "looping-flow" ? new RecipeAnimation(4, 18) : null;
        var size = new CanvasSize(128, 96);
        ArtworkRecipe plain = ArtworkRecipe.Create(Catalog.Get(id), size, 55, animation: animation);
        ArtworkRecipe changed = ArtworkRecipe.Create(Catalog.Get(id), size, 55,
            new Dictionary<string, string> { ["grain-amount"] = "0.22", ["grain-size"] = "2",
                ["dither-mode"] = "bayer4", ["dither-levels"] = "4" }, animation);
        int? frame = animation is null ? null : 0;
        byte[] Render(ArtworkRecipe recipe)
        {
            using var output = new MemoryStream();
            Engine.Render(recipe, output, frame);
            Assert.True(output.CanWrite);
            return output.ToArray();
        }
        byte[] old = Render(plain);
        byte[] newer = Render(changed);
        Assert.False(old.AsSpan().SequenceEqual(newer));
        Assert.Equal(newer, Render(changed));
        using SKBitmap decoded = SKBitmap.Decode(newer) ?? throw new InvalidOperationException();
        Assert.Equal(size.Width, decoded.Width);
        Assert.Equal(size.Height, decoded.Height);
    }

    [Fact]
    public void PngPipelineFinishingPreservesAlphaAndDisabledModeKeepsBytes()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(48, 48), rasterScale: 2);
        surface.Clear(Color32.Transparent);
        surface.DrawCircle(24, 24, 13, new Color32(128, 210, 145, 70), fill: true);
        using var baseline = new MemoryStream();
        using var disabled = new MemoryStream();
        using var textured = new MemoryStream();
        surface.WritePng(baseline);
        surface.WritePng(disabled, new ArtisticFinishOptions(0, 1, "none", 8), 17);
        surface.WritePng(textured, new ArtisticFinishOptions(.2, 1, "bayer8", 8), 17);
        Assert.Equal(baseline.ToArray(), disabled.ToArray());
        Assert.False(baseline.ToArray().AsSpan().SequenceEqual(textured.ToArray()));
        using SKBitmap plain = SKBitmap.Decode(baseline.ToArray()) ?? throw new InvalidOperationException();
        using SKBitmap finished = SKBitmap.Decode(textured.ToArray()) ?? throw new InvalidOperationException();
        for (int y = 0; y < plain.Height; y++)
            for (int x = 0; x < plain.Width; x++)
                Assert.Equal(plain.GetPixel(x, y).Alpha, finished.GetPixel(x, y).Alpha);
        Assert.Equal((byte)0, finished.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void Fine2xAndFinishingCanCoexistAndSupportLargeStandardCanvas()
    {
        ArtworkRecipe fine = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(96, 64), 1, new Dictionary<string, string>
            { ["render-quality"] = "fine-2x", ["grain-amount"] = "0.08" });
        using var output = new MemoryStream();
        Engine.Render(fine, output);
        using SKBitmap bitmap = SKBitmap.Decode(output.ToArray()) ?? throw new InvalidOperationException();
        Assert.Equal(96, bitmap.Width);
        Assert.Equal(64, bitmap.Height);

        ArtworkRecipe poster = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(4000, 4000), 1, new Dictionary<string, string>
            { ["dither-mode"] = "bayer4" });
        Engine.Validate(poster);
        // Un 4000x4000 completo è validato senza imporre una costosa
        // codifica PNG durante il test; la verifica raster resta sui 96x64.
        Assert.Equal(16_777_216L, ArtisticFinishOptions.LegacyFullFrameFinishPixels);
        Assert.Equal(33_177_600L, ArtisticFinishOptions.MaximumFinishPixels);
    }
}
