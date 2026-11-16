using System.Text.Json.Nodes;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class DepthArtworkTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);
    private static ArtworkRecipe Recipe(string id, Dictionary<string, string>? p = null,
        ulong seed = 42, int width = 160, int height = 120)
    {
        var values = id == "bokeh-depth" ? new Dictionary<string,string> { ["particles"] = "65", ["planes"] = "7" }
            : id == "paper-cut" ? new Dictionary<string,string> { ["layers"] = "9", ["samples"] = "256", ["depth-blur"] = "12" }
            : new Dictionary<string,string> { ["layers"] = "9", ["samples"] = "256", ["depth-blur"] = "8", ["soft-shadow"] = "0.3", ["rim-light"] = "0.25" };
        if (p is not null) foreach (var pair in p) values[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, values);
    }
    private static byte[] Render(ArtworkRecipe r)
    { using var stream = new MemoryStream(); Engine.Render(r, stream); return stream.ToArray(); }

    [Theory]
    [InlineData("paper-cut")]
    [InlineData("bokeh-depth")]
    [InlineData("strata-landscape")]
    public void DepthArtIsSeededAndRoundTripsAtBothRasterQualities(string id)
    {
        foreach (string quality in new[] { "standard", "fine-2x" })
        {
            var r = Recipe(id, new() { ["render-quality"] = quality });
            byte[] png = Render(r);
            Assert.Equal(png, Render(r));
            Assert.Equal(png, Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(r, Catalog), Catalog)));
            Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(id, new() { ["render-quality"] = quality }, seed: 43))));
            using var image = SKBitmap.Decode(png);
            Assert.Equal(160, image.Width); Assert.Equal(120, image.Height);
            Assert.True(image.Pixels.Distinct().Count() > 30);
        }
    }

    [Theory]
    [InlineData("paper-cut", "layers", "16")]
    [InlineData("paper-cut", "lobes", "9")]
    [InlineData("paper-cut", "ripple", "0.3")]
    [InlineData("paper-cut", "roughness", "0")]
    [InlineData("paper-cut", "twist", "-100")]
    [InlineData("paper-cut", "drift", "0.3")]
    [InlineData("paper-cut", "inner-radius", "0.2")]
    [InlineData("paper-cut", "outer-radius", "0.4")]
    [InlineData("paper-cut", "layer-bias", "2")]
    [InlineData("paper-cut", "stretch", "0.7")]
    [InlineData("paper-cut", "rotation", "80")]
    [InlineData("paper-cut", "center-x", "0.3")]
    [InlineData("paper-cut", "rim-light", "0")]
    [InlineData("paper-cut", "shadow-opacity", "0")]
    [InlineData("paper-cut", "shadow-distance", "24")]
    [InlineData("paper-cut", "shadow-softness", "15")]
    [InlineData("paper-cut", "light-angle", "120")]
    [InlineData("paper-cut", "depth-blur", "25")]
    [InlineData("paper-cut", "focus", "0.1")]
    [InlineData("paper-cut", "focus-range", "0.8")]
    [InlineData("paper-cut", "palette", "canyon")]
    [InlineData("bokeh-depth", "particles", "160")]
    [InlineData("bokeh-depth", "planes", "16")]
    [InlineData("bokeh-depth", "layout", "spiral")]
    [InlineData("bokeh-depth", "layout", "cloud")]
    [InlineData("bokeh-depth", "spread", "0.5")]
    [InlineData("bokeh-depth", "turns", "7")]
    [InlineData("bokeh-depth", "radius", "45")]
    [InlineData("bokeh-depth", "depth-scale", "1.5")]
    [InlineData("bokeh-depth", "opacity", "0.9")]
    [InlineData("bokeh-depth", "light-strength", "0")]
    [InlineData("bokeh-depth", "aperture-ring", "0.9")]
    [InlineData("bokeh-depth", "glow", "0")]
    [InlineData("bokeh-depth", "shadow-opacity", "0")]
    [InlineData("bokeh-depth", "depth-blur", "0")]
    [InlineData("bokeh-depth", "focus", "0.1")]
    [InlineData("bokeh-depth", "palette", "pearl")]
    [InlineData("strata-landscape", "depth-blur", "0")]
    [InlineData("strata-landscape", "soft-shadow", "0")]
    [InlineData("strata-landscape", "soft-shadow-softness", "18")]
    [InlineData("strata-landscape", "soft-shadow-distance", "28")]
    [InlineData("strata-landscape", "light-angle", "120")]
    [InlineData("strata-landscape", "rim-light", "0")]
    public void DepthControlsChangeTheImage(string id, string key, string value) =>
        Assert.False(Render(Recipe(id)).AsSpan().SequenceEqual(Render(Recipe(id, new() { [key] = value }))));

    [Theory]
    [InlineData("paper-cut")]
    [InlineData("bokeh-depth")]
    public void ColorRolesAcceptAlphaAndRespectTransparentBackground(string id)
    {
        var r = Recipe(id, new() { ["background-color"] = "#00000000", ["depth-color-1"] = "#8080CCFF",
            ["depth-color-2"] = "#9080CCFF", ["depth-color-3"] = "#A080CCFF", ["highlight-color"] = "#8080CCFF", ["shadow-color"] = "#FF102030" });
        using var image = SKBitmap.Decode(Render(r));
        Assert.Contains(image.Pixels, p => p.Alpha < 255);
        Assert.Contains(image.Pixels, p => p.Alpha is > 0 and < 255);
        Assert.Equal(Render(r), Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(r, Catalog), Catalog)));
    }

    [Fact]
    public void OldLandscapeJsonUsesNeutralDepthDefaults()
    {
        var original = ArtworkRecipe.Create(Catalog.Get("strata-landscape"), new CanvasSize(160, 120), 42);
        var json = JsonNode.Parse(ArtworkRecipeJson.Serialize(original, Catalog))!;
        foreach (var p in Catalog.Get("strata-landscape").Parameters.Where(p => p.Group == "Profondità"))
        { Assert.True(p.OptionalInJson); json["parameters"]!.AsObject().Remove(p.Id); }
        var loaded = ArtworkRecipeJson.Deserialize(json.ToJsonString(), Catalog);
        Assert.Equal("0", loaded.Parameters["depth-blur"]); Assert.Equal("0", loaded.Parameters["soft-shadow"]);
        Assert.Equal(Render(original), Render(loaded));
    }

    [Fact]
    public void DepthBudgetsRejectBeforeWritingOutput()
    {
        ArtworkRecipe[] bad =
        [Recipe("paper-cut", new() { ["layers"] = "64", ["samples"] = "4096" }),
         Recipe("paper-cut", width: 4096, height: 2160),
         Recipe("paper-cut", width: 63),
         Recipe("bokeh-depth", new() { ["particles"] = "4000", ["radius"] = "100", ["depth-scale"] = "6" }),
         Recipe("bokeh-depth", new() { ["planes"] = "32", ["depth-blur"] = "32", ["shadow-softness"] = "32", ["render-quality"] = "fine-2x" }, width:1920, height:1080),
         Recipe("strata-landscape", new() { ["depth-blur"] = "32" }, width:4096, height:2160)];
        foreach (var r in bad)
        {
            using var output = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(r, output));
            Assert.Equal(0, output.Length);
        }
        using var invalidRadius = new MemoryStream();
        Assert.Throws<ArgumentException>(() => Engine.Render(Recipe("paper-cut", new() { ["inner-radius"] = "0.3", ["outer-radius"] = "0.2" }), invalidRadius));
        Assert.Equal(0, invalidRadius.Length);
    }

    [Theory]
    [InlineData("paper-cut")]
    [InlineData("bokeh-depth")]
    [InlineData("strata-landscape")]
    public void CancellationDoesNotWritePng(string id)
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Recipe(id), output, cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }
}
