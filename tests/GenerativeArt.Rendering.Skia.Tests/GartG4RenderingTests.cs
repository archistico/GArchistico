using System.Text.Json.Nodes;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class GartG4RenderingTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(string id, ulong seed = 42, int width = 160, int height = 112,
        IReadOnlyDictionary<string, string>? changes = null) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, changes);

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output);
        return output.ToArray();
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public void BothArtworksRenderDeterministicSeedSensitiveFullFramePng(string id)
    {
        byte[] a = Png(Make(id));
        Assert.Equal(a, Png(Make(id)));
        Assert.False(a.AsSpan().SequenceEqual(Png(Make(id, 43))));
        using SKBitmap bitmap = SKBitmap.Decode(a) ?? throw new InvalidOperationException();
        Assert.Equal(160, bitmap.Width);
        Assert.Equal(112, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 5);
    }

    [Theory]
    [InlineData("ocellus-eyespots", "palette", "sunset")]
    [InlineData("ocellus-eyespots", "eyes", "3")]
    [InlineData("ocellus-eyespots", "fuse", "0.08")]
    [InlineData("ocellus-eyespots", "ring-color-2", "#FF7711AA")]
    [InlineData("rete-foam", "palette", "ice")]
    [InlineData("rete-foam", "wall", "10")]
    [InlineData("rete-foam", "layers", "1")]
    [InlineData("rete-foam", "wall-color", "#FFBC4352")]
    [InlineData("ocellus-eyespots", "rim-light", "0.8")]
    [InlineData("rete-foam", "membrane-opacity", "0.7")]
    public void ArtisticControlsChangeRaster(string id, string key, string value)
    {
        byte[] before = Png(Make(id));
        byte[] after = Png(Make(id, changes: new Dictionary<string, string> { [key] = value }));
        Assert.False(before.AsSpan().SequenceEqual(after));
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public void OptionalG4R2ControlsAreNeutralForOldJsonAndOriginalPixels(string id)
    {
        ArtworkRecipe baseline = Make(id, seed: 77);
        var json = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(
            ArtworkRecipeJson.Serialize(baseline, Catalog)))!.AsObject();
        var parameters = json["parameters"]!.AsObject();
        string[] keys = id == "ocellus-eyespots"
            ? ["pore-strength", "pore-radius", "rim-light", "rim-width"]
            : ["junction-strength", "junction-radius", "node-glow", "membrane-opacity"];
        foreach (string key in keys) Assert.True(parameters.Remove(key));
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(json.ToJsonString(), Catalog);
        Assert.Equal(Png(baseline), Png(restored));
    }

    [Fact]
    public void OcellusPoresAndRimLightRenderDeterministically()
    {
        var values = new Dictionary<string, string>
        {
            ["eyes"] = "24", ["room"] = "0", ["core-radius"] = "0.16",
            ["pore-strength"] = "1", ["pore-radius"] = "25",
            ["rim-light"] = "0.9", ["rim-width"] = "4"
        };
        var a = Make("ocellus-eyespots", seed: 142, width: 320, height: 240, changes: values);
        Assert.Equal(Png(a), Png(a));
        Assert.False(Png(Make("ocellus-eyespots", seed: 142, width: 320, height: 240))
            .AsSpan().SequenceEqual(Png(a)));
    }

    [Fact]
    public void ReteJunctionAndMembraneControlsChangePixels()
    {
        var values = new Dictionary<string, string>
        {
            ["junction-strength"] = "0.9", ["junction-radius"] = "12",
            ["node-glow"] = "0.9", ["membrane-opacity"] = "0.7"
        };
        var a = Make("rete-foam", seed: 143, width: 256, height: 192, changes: values);
        Assert.Equal(Png(a), Png(a));
        Assert.False(Png(Make("rete-foam", seed: 143, width: 256, height: 192))
            .AsSpan().SequenceEqual(Png(a)));
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public void JsonFine2xFinishingAndExactPreviewStayDeterministic(string id)
    {
        var recipe = Make(id, 100, changes: new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x", ["grain-amount"] = "0.01",
            ["paper-texture"] = "cotton", ["paper-strength"] = "0.04"
        });
        var restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
        Assert.Equal(Png(recipe), Png(restored));
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public void PreCancelledRenderingDoesNotWriteOutput(string id)
    {
        using var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(id), output,
            cancellationToken: tokenSource.Token));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void ValidatedBudgetsAndCoupledRadiiRejectBeforeWriting()
    {
        var tooManyEyes = Make("ocellus-eyespots", width: 4096, height: 4096,
            changes: new Dictionary<string, string> { ["eyes"] = "80" });
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(tooManyEyes));

        var reversedRadii = Make("rete-foam", changes: new Dictionary<string, string>
        {
            ["big-radius"] = "0.03", ["small-radius"] = "0.04"
        });
        Assert.Throws<ArgumentException>(() => Engine.Validate(reversedRadii));

        var tooLarge = Make("rete-foam", width: 4096, height: 4096,
            changes: new Dictionary<string, string> { ["layers"] = "5", ["bubbles"] = "850" });
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(tooLarge));
    }
}
