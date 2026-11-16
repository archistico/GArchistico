using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class LineworkRendererTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Recipe(string id, Dictionary<string, string>? values = null,
        ulong seed = 42, int width = 160, int height = 120)
    {
        var parameters = id == "guilloche" ? new Dictionary<string, string>
            { ["layers"] = "8", ["samples"] = "2000" } : new Dictionary<string, string>();
        if (values is not null) foreach (var pair in values) parameters[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, parameters);
    }

    private static byte[] Render(ArtworkRecipe recipe)
    {
        using var stream = new MemoryStream();
        Engine.Render(recipe, stream);
        return stream.ToArray();
    }

    [Theory]
    [InlineData("guilloche")]
    [InlineData("tonal-engraving")]
    public void DetailedArtworksProduceSeededPngAndRoundTripJson(string id)
    {
        ArtworkRecipe recipe = Recipe(id);
        byte[] png = Render(recipe);
        Assert.Equal(png, Render(recipe));
        Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(id, seed: 43))));
        Assert.Equal(png, Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog)));
        using SKBitmap image = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(160, image.Width);
        Assert.Equal(120, image.Height);
        Assert.True(image.Pixels.Distinct().Count() > 30);
    }

    [Theory]
    [InlineData("guilloche", "curve", "outside")]
    [InlineData("guilloche", "fixed-teeth", "67")]
    [InlineData("guilloche", "moving-teeth", "13")]
    [InlineData("guilloche", "pen-radius", "0.9")]
    [InlineData("guilloche", "waves", "13")]
    [InlineData("guilloche", "wave-amount", "0.2")]
    [InlineData("guilloche", "samples", "12000")]
    [InlineData("guilloche", "layers", "16")]
    [InlineData("guilloche", "inner-scale", "0.7")]
    [InlineData("guilloche", "layer-bias", "2")]
    [InlineData("guilloche", "twist", "-100")]
    [InlineData("guilloche", "scale", "0.7")]
    [InlineData("guilloche", "rotation", "35")]
    [InlineData("guilloche", "center-x", "0.3")]
    [InlineData("guilloche", "center-y", "0.4")]
    [InlineData("guilloche", "line-width", "2")]
    [InlineData("guilloche", "opacity", "0.3")]
    [InlineData("guilloche", "palette", "midnight")]
    [InlineData("tonal-engraving", "field", "rings")]
    [InlineData("tonal-engraving", "field", "marble")]
    [InlineData("tonal-engraving", "frequency", "0.007")]
    [InlineData("tonal-engraving", "octaves", "1")]
    [InlineData("tonal-engraving", "warp-amount", "0")]
    [InlineData("tonal-engraving", "spacing", "12")]
    [InlineData("tonal-engraving", "line-width", "2")]
    [InlineData("tonal-engraving", "hatch-layers", "1")]
    [InlineData("tonal-engraving", "angle", "-30")]
    [InlineData("tonal-engraving", "cross-angle", "90")]
    [InlineData("tonal-engraving", "bend", "0")]
    [InlineData("tonal-engraving", "contrast", "3")]
    [InlineData("tonal-engraving", "tone-offset", "0.2")]
    [InlineData("tonal-engraving", "gamma", "2")]
    [InlineData("tonal-engraving", "tone-direction", "inverted")]
    [InlineData("tonal-engraving", "wash", "0.4")]
    [InlineData("tonal-engraving", "mask", "circle")]
    [InlineData("tonal-engraving", "mask", "diamond")]
    [InlineData("tonal-engraving", "center-x", "0.3")]
    [InlineData("tonal-engraving", "center-y", "0.4")]
    [InlineData("tonal-engraving", "palette", "copper")]
    [InlineData("domain-warp", "lacunarity", "2.4")]
    [InlineData("domain-warp", "persistence", "0.7")]
    [InlineData("domain-warp", "warp-frequency", "0.5")]
    [InlineData("domain-warp", "rotation", "35")]
    [InlineData("domain-warp", "stretch-x", "2")]
    [InlineData("domain-warp", "stretch-y", "0.5")]
    [InlineData("domain-warp", "offset-x", "0.7")]
    [InlineData("domain-warp", "offset-y", "-0.7")]
    [InlineData("domain-warp", "contrast", "2")]
    [InlineData("domain-warp", "gamma", "2")]
    public void ArtisticControlsActuallyChangeOutput(string id, string parameter, string value)
    {
        Assert.False(Render(Recipe(id)).AsSpan().SequenceEqual(Render(Recipe(id,
            new Dictionary<string, string> { [parameter] = value }))));
    }

    [Fact]
    public void LegacyDomainWarpJsonGetsNeutralOptionalControls()
    {
        const string legacy = """
            {"formatVersion":1,"artworkId":"domain-warp","algorithmVersion":1,"seed":"42",
             "width":160,"height":120,"parameters":{"frequency":0.001,"octaves":3,
             "warp-amount":1.3,"warp-layers":2,"pattern":"marble","bands":4,"palette":"ocean"}}
            """;
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(legacy, Catalog);
        Assert.Equal("2", loaded.Parameters["lacunarity"]);
        Assert.Equal("0.5", loaded.Parameters["persistence"]);
        Assert.Equal("1", loaded.Parameters["contrast"]);
        Assert.Equal(Render(Recipe("domain-warp")), Render(loaded));
    }

    [Fact]
    public void MaskAndBandControlsAffectTheModesTheyBelongTo()
    {
        byte[] rings = Render(Recipe("tonal-engraving", new() { ["field"] = "rings" }));
        Assert.False(rings.AsSpan().SequenceEqual(Render(Recipe("tonal-engraving",
            new() { ["field"] = "rings", ["bands"] = "15" }))));
        byte[] masked = Render(Recipe("tonal-engraving", new() { ["mask"] = "circle" }));
        foreach (var (key, value) in new[] { ("mask-radius", "0.5"), ("mask-feather", "0.2") })
            Assert.False(masked.AsSpan().SequenceEqual(Render(Recipe("tonal-engraving",
                new() { ["mask"] = "circle", [key] = value }))));
    }

    [Theory]
    [InlineData("guilloche")]
    [InlineData("tonal-engraving")]
    public void TransparentBackgroundAndAlphaSurviveFineSamplingAndMaterial(string id)
    {
        var values = new Dictionary<string, string>
        {
            ["background-color"] = "#00000000", ["ink-color-1"] = "#604080C0",
            ["ink-color-2"] = "#604080C0", ["ink-color-3"] = "#604080C0",
            ["render-quality"] = "fine-2x"
        };
        if (id == "tonal-engraving") { values["mask"] = "circle"; values["mask-radius"] = "0.7"; }
        using SKBitmap plain = SKBitmap.Decode(Render(Recipe(id, values))) ?? throw new InvalidOperationException();
        values["paper-texture"] = "cotton"; values["paper-strength"] = "0.2";
        using SKBitmap finished = SKBitmap.Decode(Render(Recipe(id, values))) ?? throw new InvalidOperationException();
        Assert.Contains(plain.Pixels, p => p.Alpha == 0);
        Assert.Contains(plain.Pixels, p => p.Alpha > 0 && p.Alpha < 255);
        Assert.Equal(plain.Pixels.Select(p => p.Alpha), finished.Pixels.Select(p => p.Alpha));
    }

    [Fact]
    public void UnsupportedWorkAndInvalidCoupledControlsFailBeforeWriting()
    {
        var invalid = new[]
        {
            Recipe("guilloche", new() { ["layers"] = "200", ["samples"] = "48000" }),
            Recipe("guilloche", new() { ["fixed-teeth"] = "17", ["moving-teeth"] = "17" }),
            Recipe("tonal-engraving", new() { ["spacing"] = "2", ["line-width"] = "3" }),
            Recipe("tonal-engraving", new() { ["octaves"] = "5" }, width: 4096, height: 4096)
        };
        foreach (ArtworkRecipe recipe in invalid)
        {
            using var output = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, output));
            Assert.Equal(0, output.Length);
        }
        foreach (string id in new[] { "guilloche", "tonal-engraving" })
        {
            Engine.Validate(Recipe(id, new() { ["render-quality"] = "fine-2x" }, width: 1920, height: 1080));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            using var stream = new MemoryStream();
            Assert.Throws<OperationCanceledException>(() => Engine.Render(Recipe(id), stream,
                cancellationToken: cancellation.Token));
            Assert.Equal(0, stream.Length);
        }
    }
}
