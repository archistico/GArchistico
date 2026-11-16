using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class MaterialPostProcessorTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    private static PixelBuffer Pattern(int width = 65, int height = 43)
    {
        var buffer = new PixelBuffer(new CanvasSize(width, height));
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                buffer.SetPixel(x, y, new Color32(170, (byte)(40 + x % 190),
                    (byte)(30 + y % 190), 115));
        buffer.SetPixel(0, 0, new Color32(0, 245, 100, 50));
        return buffer;
    }

    private static uint[] Pixels(PixelBuffer pixels) =>
        Enumerable.Range(0, pixels.Height)
            .SelectMany(y => pixels.GetPackedArgbRow(y).ToArray()).ToArray();

    [Theory]
    [InlineData("cotton")]
    [InlineData("rough")]
    public void PaperTextureIsSeededDeterministicAndPreservesAlpha(string paper)
    {
        var options = new ArtisticFinishOptions(0, 1, "none", 8,
            paper, .35, .3, 0);
        PixelBuffer a = Pattern(), b = Pattern(), c = Pattern();
        uint[] original = Pixels(a);
        MaterialPostProcessor.Apply(a, options, 752);
        MaterialPostProcessor.Apply(b, options, 752);
        MaterialPostProcessor.Apply(c, options, 753);
        Assert.Equal(Pixels(a), Pixels(b));
        Assert.False(Pixels(a).AsSpan().SequenceEqual(Pixels(c)));
        Assert.False(Pixels(a).AsSpan().SequenceEqual(original));
        uint[] after = Pixels(a);
        for (int i = 0; i < original.Length; i++)
            Assert.Equal((byte)(original[i] >> 24), (byte)(after[i] >> 24));
        Assert.Equal(new Color32(0, 245, 100, 50), a.GetPixel(0, 0));
    }

    [Fact]
    public void InkAbsorptionDependsOnOriginalDarkEdgesNotUpdatedPixels()
    {
        var pixels = new PixelBuffer(new CanvasSize(9, 6));
        pixels.Clear(Color32.Opaque(240, 240, 240));
        for (int y = 0; y < 6; y++) pixels.SetPixel(4, y, Color32.Opaque(10, 20, 30));
        var options = new ArtisticFinishOptions(0, 1, "none", 8,
            "none", 0, 0, .4);
        MaterialPostProcessor.Apply(pixels, options, 25);
        Assert.Equal(Color32.Opaque(240, 240, 240), pixels.GetPixel(0, 3));
        Assert.Equal(Color32.Opaque(10, 20, 30), pixels.GetPixel(4, 3));
        Assert.True(pixels.GetPixel(3, 3).R < 240);
        Assert.True(pixels.GetPixel(5, 3).R < 240);
        Assert.Equal(Color32.Opaque(240, 240, 240), pixels.GetPixel(2, 3));
    }

    [Fact]
    public void MaterialOffDoesNotAlterPixelsAndBadOptionsAreRejected()
    {
        PixelBuffer unchanged = Pattern();
        uint[] before = Pixels(unchanged);
        var disabled = new ArtisticFinishOptions(0, 1, "none", 8,
            "rough", 0, 0, 0);
        Assert.False(disabled.IsEnabled);
        ArtisticPostProcessor.Apply(unchanged, disabled, 11);
        Assert.Equal(before, Pixels(unchanged));
        Assert.Throws<ArgumentException>(() => (disabled with { PaperTexture = "silk" }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (disabled with { PaperStrength = double.PositiveInfinity }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (disabled with { PigmentGranulation = -.1 }).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (disabled with { InkAbsorption = .7 }).Validate());
    }

    [Fact]
    public void MaterialEffectsAreAvailableForEveryArtworkAndRoundTripAsOptionalV1()
    {
        foreach (ArtworkDescriptor descriptor in Catalog.All)
        {
            foreach (string key in new[] { "paper-texture", "paper-strength",
                         "pigment-granulation", "ink-absorption" })
                Assert.True(descriptor.Parameters.Single(p => p.Id == key).OptionalInJson);

            var animation = descriptor.Id == "looping-flow" ? new RecipeAnimation(3, 12) : null;
            ArtworkRecipe recipe = ArtworkRecipe.Create(descriptor, new CanvasSize(96, 64), 42,
                new Dictionary<string, string>
                {
                    ["paper-texture"] = "cotton", ["paper-strength"] = "0.25",
                    ["pigment-granulation"] = "0.22", ["ink-absorption"] = "0.15"
                }, animation);
            ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(
                ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
            Assert.Equal(1, loaded.FormatVersion);
            Assert.Equal(new ArtisticFinishOptions(0, 1, "none", 8,
                "cotton", .25, .22, .15), ArtisticFinishOptions.FromRecipe(loaded));
        }

        const string legacy = """
            {"formatVersion":1,"artworkId":"plasma-noise","algorithmVersion":1,
             "seed":"99","width":96,"height":64,
             "parameters":{"frequency":0.016,"octaves":4,"palette":"deep"}}
            """;
        var old = ArtworkRecipeJson.Deserialize(legacy, Catalog);
        Assert.Equal("none", old.Parameters["paper-texture"]);
        Assert.Equal("0", old.Parameters["paper-strength"]);
        Assert.Equal("0", old.Parameters["pigment-granulation"]);
        Assert.Equal("0", old.Parameters["ink-absorption"]);
        Assert.False(ArtisticFinishOptions.FromRecipe(old).IsEnabled);
    }

    [Fact]
    public void RendererProducesStableMaterialPngWithCorrectSizeAndPreservedTransparency()
    {
        var recipe = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(128, 96), 182, new Dictionary<string, string>
            {
                ["render-quality"] = "fine-2x", ["paper-texture"] = "rough",
                ["paper-strength"] = "0.32", ["pigment-granulation"] = "0.22",
                ["ink-absorption"] = "0.14"
            });
        var engine = new ArtworkRenderEngine(Catalog);
        using var a = new MemoryStream();
        using var b = new MemoryStream();
        engine.Render(recipe, a);
        engine.Render(recipe, b);
        Assert.Equal(a.ToArray(), b.ToArray());
        using SKBitmap image = SKBitmap.Decode(a.ToArray()) ?? throw new InvalidOperationException();
        Assert.Equal(128, image.Width);
        Assert.Equal(96, image.Height);

        // Canvas con trasparenza semiopaca: materiale non modifica mai alpha.
        using var surface = new SkiaRasterSurface(new CanvasSize(24, 24));
        surface.Clear(Color32.Transparent);
        surface.DrawCircle(12, 12, 8, new Color32(130, 50, 110, 220), fill: true);
        using var baseline = new MemoryStream();
        using var finished = new MemoryStream();
        surface.WritePng(baseline);
        surface.WritePng(finished, new ArtisticFinishOptions(0, 1, "none", 8,
            "cotton", .32, .25, .2), 182);
        using SKBitmap plain = SKBitmap.Decode(baseline.ToArray()) ?? throw new InvalidOperationException();
        using SKBitmap material = SKBitmap.Decode(finished.ToArray()) ?? throw new InvalidOperationException();
        for (int y = 0; y < 24; y++)
            for (int x = 0; x < 24; x++)
                Assert.Equal(plain.GetPixel(x, y).Alpha, material.GetPixel(x, y).Alpha);
        Assert.False(baseline.ToArray().AsSpan().SequenceEqual(finished.ToArray()));
    }

    [Theory]
    [InlineData(2500, 2000, "standard")]
    [InlineData(4000, 4000, "standard")]
    [InlineData(4096, 4096, "standard")]
    [InlineData(2048, 2048, "fine-2x")]
    public void MaterialBudgetAcceptsSupportedCanvasWithoutRendering(int width, int height,
        string quality)
    {
        var recipe = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(width, height), 7,
            new Dictionary<string, string>
            {
                ["pigment-granulation"] = "0.1", ["render-quality"] = quality
            });
        // Il budget si verifica senza allocare superfici o codificare PNG poster.
        new ArtworkRenderEngine(Catalog).Validate(recipe);
    }

    [Theory]
    [InlineData(4097, 4096, "standard")]
    [InlineData(4096, 4097, "standard")]
    [InlineData(2500, 2000, "fine-2x")]
    [InlineData(4000, 4000, "fine-2x")]
    public void MaterialBudgetRejectsUnsupportedCanvasBeforeOutput(int width, int height,
        string quality)
    {
        var recipe = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(width, height), 7,
            new Dictionary<string, string>
            {
                ["pigment-granulation"] = "0.1", ["render-quality"] = quality
            });
        using var stream = new MemoryStream();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ArtworkRenderEngine(Catalog).Render(recipe, stream));
        Assert.Equal("recipe", exception.ParamName);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void CancellationBeforeMaterialProcessingLeavesPixelsUntouched()
    {
        PixelBuffer pixels = Pattern();
        uint[] before = Pixels(pixels);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => MaterialPostProcessor.Apply(pixels,
            new ArtisticFinishOptions(0, 1, "none", 8, "rough", .2, .2, .2),
            8, cancellation.Token));
        Assert.Equal(before, Pixels(pixels));
    }
}
