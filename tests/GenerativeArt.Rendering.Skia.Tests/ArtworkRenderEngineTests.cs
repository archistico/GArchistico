using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Files;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ArtworkRenderEngineTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Recipe(string id, ulong seed = 42,
        IReadOnlyDictionary<string, string>? values = null, RecipeAnimation? animation = null,
        int width = 128, int height = 96) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, values, animation);

    private static byte[] Render(ArtworkRecipe recipe, int? frameIndex = null)
    {
        using var stream = new MemoryStream();
        Engine.Render(recipe, stream, frameIndex);
        Assert.True(stream.CanWrite); // il renderer non possiede lo stream
        return stream.ToArray();
    }

    [Theory]
    [InlineData("parametric-curves")]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("plasma-noise")]
    [InlineData("topographic-contours")]
    [InlineData("sdf-orbits")]
    [InlineData("poisson-stippling")]
    [InlineData("voronoi-mosaic")]
    public void EveryStaticRegisteredArtworkProducesDecodablePng(string id)
    {
        byte[] png = Render(Recipe(id));
        using SKBitmap bitmap = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(128, bitmap.Width);
        Assert.Equal(96, bitmap.Height);
        Assert.Equal(png, Render(Recipe(id)));
    }

    [Theory]
    [InlineData("noise-flow", "lines", "1", "120")]
    [InlineData("spaced-streamlines", "max-lines", "1", "120")]
    [InlineData("parametric-curves", "scale", "0.15", "0.45")]
    [InlineData("plasma-noise", "frequency", "0.002", "0.06")]
    [InlineData("plasma-noise", "octaves", "1", "6")]
    [InlineData("plasma-noise", "palette", "deep", "ember")]
    [InlineData("topographic-contours", "frequency", "0.004", "0.04")]
    [InlineData("topographic-contours", "contours", "4", "20")]
    [InlineData("topographic-contours", "cell-size", "3", "12")]
    [InlineData("topographic-contours", "line-width", "0.5", "3")]
    [InlineData("topographic-contours", "palette", "atlas", "terra")]
    [InlineData("topographic-contours", "line-style", "precise", "handdrawn")]
    [InlineData("topographic-contours", "line-style", "precise", "soft")]
    [InlineData("topographic-contours", "major-line-color", "auto", "#FF00FF")]
    [InlineData("topographic-contours", "band-color-4", "auto", "#000000")]
    [InlineData("sdf-orbits", "shape-scale", "0.6", "1.2")]
    [InlineData("sdf-orbits", "smoothness", "0.002", "0.18")]
    [InlineData("sdf-orbits", "bands", "4", "20")]
    [InlineData("sdf-orbits", "edge-width", "0.002", "0.02")]
    [InlineData("sdf-orbits", "palette", "mineral", "iris")]
    [InlineData("sdf-orbits", "ink-color", "auto", "#00FF00")]
    [InlineData("poisson-stippling", "spacing", "0.015", "0.08")]
    [InlineData("poisson-stippling", "max-points", "100", "1200")]
    [InlineData("poisson-stippling", "dot-size", "0.5", "5")]
    [InlineData("poisson-stippling", "palette", "paper", "night")]
    [InlineData("poisson-stippling", "dot-style", "solid", "ring")]
    [InlineData("poisson-stippling", "radius-variation", "0", "1")]
    [InlineData("poisson-stippling", "pigment-frequency", "0.003", "0.08")]
    [InlineData("poisson-stippling", "background-color", "auto", "#AABBCC")]
    [InlineData("voronoi-mosaic", "sites", "24", "160")]
    [InlineData("voronoi-mosaic", "relaxations", "0", "3")]
    [InlineData("voronoi-mosaic", "jitter", "0", "0.9")]
    [InlineData("voronoi-mosaic", "gutter", "0.5", "8")]
    [InlineData("voronoi-mosaic", "palette", "glass", "lagoon")]
    [InlineData("voronoi-mosaic", "edge-color", "auto", "#FFFFFF")]
    public void ParametersActuallyAffectRaster(string id, string key, string first, string second)
    {
        var a = Recipe(id, values: new Dictionary<string, string> { [key] = first });
        var b = Recipe(id, values: new Dictionary<string, string> { [key] = second });
        Assert.False(Render(a).AsSpan().SequenceEqual(Render(b)));
    }

    [Theory]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("plasma-noise")]
    [InlineData("topographic-contours")]
    [InlineData("sdf-orbits")]
    [InlineData("poisson-stippling")]
    [InlineData("voronoi-mosaic")]
    public void SeedAffectsRendererAndRepeatSeedIsStable(string id)
    {
        byte[] first = Render(Recipe(id, 1));
        Assert.Equal(first, Render(Recipe(id, 1)));
        Assert.False(first.AsSpan().SequenceEqual(Render(Recipe(id, 2))));
    }

    [Fact]
    public void LoopProducesIndependentFramesAndHonorsPaletteAndRadius()
    {
        var options = new RecipeAnimation(4, 24);
        var baseRecipe = Recipe("looping-flow", animation: options, width: 96, height: 96);
        byte[] first = Render(baseRecipe, 0);
        using SKBitmap image = SKBitmap.Decode(first) ?? throw new InvalidOperationException();
        Assert.Equal(96, image.Width);
        Assert.False(first.AsSpan().SequenceEqual(Render(baseRecipe, 1)));
        Assert.Equal(first, Render(baseRecipe, 0));
        Assert.False(first.AsSpan().SequenceEqual(Render(Recipe("looping-flow", animation: options, width: 96, height: 96,
            values: new Dictionary<string, string> { ["palette"] = "ember" }), 0)));
        Assert.False(first.AsSpan().SequenceEqual(Render(Recipe("looping-flow", animation: options, width: 96, height: 96,
            values: new Dictionary<string, string> { ["loop-radius"] = "3" }), 0)));
    }

    [Fact]
    public void InvalidFrameAndBudgetFailBeforeWriting()
    {
        var recipe = Recipe("looping-flow", animation: new RecipeAnimation(5, 24), width: 96, height: 96);
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, stream));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, stream, 5));
        Assert.Equal(0, stream.Length);
        Assert.Throws<ArgumentException>(() => Engine.Validate(Recipe("plasma-noise"), 0));
        var oversized = Recipe("plasma-noise", width: 4097, height: 1024);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(oversized, stream));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void FiveMegapixelPlasmaWithMaterialAndGrainIsAccepted()
    {
        var large = Recipe("plasma-noise", width: 2500, height: 2000,
            values: new Dictionary<string, string>
            {
                ["grain-amount"] = "0.08", ["paper-texture"] = "cotton",
                ["paper-strength"] = "0.1", ["dither-mode"] = "bayer4"
            });
        Engine.Validate(large); // validazione del caso segnalato, senza allocare 5 MP nei test
    }

    [Fact]
    public void FullHdLoopIsValidWithStudioDefaultFrameCountButNotUnboundedFrames()
    {
        var valid = Recipe("looping-flow", width: 1920, height: 1080,
            animation: new RecipeAnimation(16, 18));
        Engine.Validate(valid, 0);
        Engine.Validate(valid, 15);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(
            Recipe("looping-flow", width: 1920, height: 1080,
                animation: new RecipeAnimation(36, 18)), 0));
    }

    [Fact]
    public void PreCancelledRenderingLeavesStreamUnmodified()
    {
        using var token = new CancellationTokenSource();
        token.Cancel();
        using var stream = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(
            Recipe("plasma-noise"), stream, cancellationToken: token.Token));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void NewArtworkJsonRoundTripAndInvalidParameterRejected()
    {
        ArtworkRecipe recipe = Recipe("plasma-noise", values: new Dictionary<string, string>
        {
            ["frequency"] = "0.025", ["octaves"] = "3", ["palette"] = "moss"
        });
        byte[] json = ArtworkRecipeJson.Serialize(recipe, Catalog);
        ArtworkRecipe rebuilt = ArtworkRecipeJson.Deserialize(json, Catalog);
        Assert.Equal(Render(recipe), Render(rebuilt));
        Assert.Throws<ArgumentException>(() => Recipe("plasma-noise",
            values: new Dictionary<string, string> { ["octaves"] = "7" }));
    }

    [Fact]
    public void TopographicRecipeRoundTripBudgetAndCancellation()
    {
        ArtworkRecipe original = Recipe("topographic-contours", 421,
            new Dictionary<string, string>
            {
                ["frequency"] = "0.016", ["contours"] = "8",
                ["cell-size"] = "4", ["line-width"] = "1.7", ["palette"] = "nocturne"
            });
        byte[] encoded = ArtworkRecipeJson.Serialize(original, Catalog);
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(encoded, Catalog);
        Assert.Equal(Render(original), Render(restored));
        Assert.False(Render(original).AsSpan().SequenceEqual(Render(Recipe("topographic-contours", 422))));
        Engine.Validate(Recipe("topographic-contours", width: 1600, height: 1600));
        Engine.Validate(Recipe("topographic-contours", width: 4000, height: 4000));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(Recipe("topographic-contours", width: 4097, height: 4000)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(Recipe("topographic-contours", width: 12, height: 20)));
        // 1333 x 1333 celle: 22 livelli = 39.091.558 operazioni (accettato),
        // 23 livelli = 40.868.447 operazioni (rifiutato con budget 40 milioni).
        Engine.Validate(Recipe("topographic-contours", width: 4000, height: 4000,
            values: new Dictionary<string, string> { ["cell-size"] = "3", ["contours"] = "22" }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(Recipe("topographic-contours", width: 4000, height: 4000,
            values: new Dictionary<string, string> { ["cell-size"] = "3", ["contours"] = "23" })));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(Recipe("topographic-contours", width: 4000, height: 4000,
            values: new Dictionary<string, string> { ["cell-size"] = "3", ["contours"] = "24" })));
        Assert.Throws<ArgumentException>(() => Recipe("topographic-contours",
            values: new Dictionary<string, string> { ["contours"] = "40" }));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(original, output, cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData("sdf-orbits")]
    [InlineData("poisson-stippling")]
    [InlineData("voronoi-mosaic")]
    public void NewGeometryRenderersAcceptPosterSizeAndRejectInvalidRecipes(string artworkId)
    {
        Engine.Validate(Recipe(artworkId, width: 4000, height: 4000));
        // R2: Voronoi e Poisson disegnano direttamente su Skia e ora ammettono
        // canvas maggiori del vecchio limite 4K; SDF usa buffer per pixel.
        if (RasterCanvasBudgetPolicy.SupportsEightK(artworkId))
            Engine.Validate(Recipe(artworkId, width: 4097, height: 4000));
        else
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Engine.Validate(Recipe(artworkId, width: 4097, height: 4000)));
        ArtworkRecipe small = Recipe(artworkId, width: 160, height: 120);
        byte[] json = ArtworkRecipeJson.Serialize(small, Catalog);
        Assert.Equal(Render(small), Render(ArtworkRecipeJson.Deserialize(json, Catalog)));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() =>
            Engine.Render(small, output, cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void VoronoiRecipeRoundTripAndValidationBeforeWriting()
    {
        ArtworkRecipe original = Recipe("voronoi-mosaic", 404,
            new Dictionary<string, string>
            {
                ["sites"] = "42", ["relaxations"] = "2", ["jitter"] = "0.32",
                ["gutter"] = "1.8", ["palette"] = "copper"
            }, width: 192, height: 140);
        byte[] json = ArtworkRecipeJson.Serialize(original, Catalog);
        Assert.Equal(Render(original), Render(ArtworkRecipeJson.Deserialize(json, Catalog)));
        Assert.Throws<ArgumentException>(() => Recipe("voronoi-mosaic",
            values: new Dictionary<string, string> { ["sites"] = "257" }));
        Assert.Throws<ArgumentException>(() => Recipe("voronoi-mosaic",
            values: new Dictionary<string, string> { ["relaxations"] = "4" }));
        using var destination = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(
            Recipe("voronoi-mosaic", width: 63, height: 256), destination));
        Assert.Equal(0, destination.Length);
    }

    [Fact]
    public void PoissonRejectsSmallCanvasBeforeWriting()
    {
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(
            Recipe("poisson-stippling", width: 63, height: 200), output));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public async Task AtomicStreamingExportMatchesRendererAndDecodesPng()
    {
        string directory = Path.Combine(Path.GetTempPath(), "ga-png-export-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "plasma.png");
        try
        {
            ArtworkRecipe recipe = Recipe("plasma-noise", width: 96, height: 80);
            await AtomicOutputFiles.WriteGeneratedAsync(path,
                (stream, ct) => Engine.Render(recipe, stream, cancellationToken: ct));
            byte[] bytes = await File.ReadAllBytesAsync(path);
            using SKBitmap bitmap = SKBitmap.Decode(bytes) ?? throw new InvalidDataException("PNG illeggibile");
            Assert.Equal(96, bitmap.Width);
            Assert.Equal(80, bitmap.Height);
            Assert.Equal(Render(recipe), bytes);
            Assert.Single(Directory.GetFiles(directory));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
}
