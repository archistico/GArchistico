using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class FlowTextureRendererTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Recipe(string id, Dictionary<string, string>? parameters = null,
        ulong seed = 42, int width = 160, int height = 120) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, parameters);

    private static byte[] Render(ArtworkRecipe recipe)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output);
        return output.ToArray();
    }

    [Theory]
    [InlineData("domain-warp")]
    [InlineData("lic-flow")]
    public void NewArtworksProduceDeterministicPngAndRoundTripJson(string id)
    {
        ArtworkRecipe original = Recipe(id);
        byte[] png = Render(original);
        Assert.Equal(png, Render(original));
        Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(id, seed: 43))));
        var loaded = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(original, Catalog), Catalog);
        Assert.Equal(png, Render(loaded));
        using SKBitmap image = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(160, image.Width);
        Assert.Equal(120, image.Height);
        Assert.True(image.Pixels.Select(p => p.Red).Distinct().Count() > 8);
    }

    [Theory]
    [InlineData("domain-warp", "frequency", "0.02")]
    [InlineData("domain-warp", "octaves", "1")]
    [InlineData("domain-warp", "warp-amount", "0")]
    [InlineData("domain-warp", "warp-layers", "1")]
    [InlineData("domain-warp", "pattern", "clouds")]
    [InlineData("domain-warp", "pattern", "ridges")]
    [InlineData("domain-warp", "bands", "3")]
    [InlineData("lic-flow", "field", "vortex")]
    [InlineData("lic-flow", "frequency", "0.02")]
    [InlineData("lic-flow", "length", "3")]
    [InlineData("lic-flow", "contrast", "6")]
    [InlineData("lic-flow", "texture-scale", "3")]
    [InlineData("lic-flow", "palette", "ember")]
    [InlineData("domain-warp", "palette", "graphite")]
    [InlineData("lic-flow", "gradient-color-2", "#FF0066")]
    [InlineData("domain-warp", "gradient-color-2", "#FF0066")]
    public void ArtisticParametersActuallyChangePixels(string id, string key, string value)
    {
        Assert.False(Render(Recipe(id)).AsSpan().SequenceEqual(
            Render(Recipe(id, new Dictionary<string, string> { [key] = value }))));
    }

    [Theory]
    [InlineData("domain-warp")]
    [InlineData("lic-flow")]
    public void AlphaAndMaterialArePreservedWithFineSampling(string id)
    {
        var parameters = new Dictionary<string, string>
        {
            ["gradient-color-1"] = "#60104090", ["gradient-color-2"] = "#602070A0",
            ["gradient-color-3"] = "#6030B0B0", ["gradient-color-4"] = "#60A0E0C0",
            ["render-quality"] = "fine-2x", ["paper-texture"] = "cotton",
            ["paper-strength"] = "0.2", ["pigment-granulation"] = "0.1"
        };
        using SKBitmap image = SKBitmap.Decode(Render(Recipe(id, parameters))) ?? throw new InvalidOperationException();
        Assert.Equal(160, image.Width);
        Assert.Equal(120, image.Height);
        Assert.All(image.Pixels, p => Assert.InRange(p.Alpha, (byte)95, (byte)97));
    }

    [Theory]
    [InlineData("domain-warp", "octaves", "6")]
    [InlineData("lic-flow", "length", "48")]
    public void ExcessiveWorkIsRejectedBeforeWriting(string id, string key, string value)
    {
        var parameters = new Dictionary<string, string> { [key] = value };
        if (id == "domain-warp") parameters["warp-layers"] = "3";
        ArtworkRecipe oversized = Recipe(id, parameters, width: 4096, height: 4096);
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(oversized, output));
        Assert.Equal(0, output.Length);
        // Full HD e fine-2x predefiniti restano consentiti.
        Engine.Validate(Recipe(id, width: 1920, height: 1080));
        Engine.Validate(Recipe(id, new Dictionary<string, string> { ["render-quality"] = "fine-2x" },
            width: 1920, height: 1080));
    }

    [Theory]
    [InlineData("domain-warp")]
    [InlineData("lic-flow")]
    public void CancelledRenderingDoesNotWriteOutput(string id)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() =>
            Engine.Render(Recipe(id), output, cancellationToken: cancellation.Token));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void ExpandedStreamlinesAcceptTenThousandAndNewControlsAffectOutput()
    {
        Engine.Validate(Recipe("spaced-streamlines", new Dictionary<string, string>
            { ["max-lines"] = "10000", ["separation"] = "4" }, width: 1920, height: 1080));
        Assert.Throws<ArgumentException>(() => Recipe("spaced-streamlines",
            new Dictionary<string, string> { ["max-lines"] = "10001" }));
        byte[] original = Render(Recipe("spaced-streamlines"));
        foreach (var (key, value) in new[] { ("separation", "4"), ("line-width", "3") })
            Assert.False(original.AsSpan().SequenceEqual(Render(Recipe("spaced-streamlines",
                new Dictionary<string, string> { [key] = value }))));
        const string legacy = """
            {"formatVersion":1,"artworkId":"spaced-streamlines","algorithmVersion":1,
             "seed":"42","width":160,"height":120,"parameters":{"max-lines":210}}
            """;
        Assert.Equal(original, Render(ArtworkRecipeJson.Deserialize(legacy, Catalog)));
    }
}
