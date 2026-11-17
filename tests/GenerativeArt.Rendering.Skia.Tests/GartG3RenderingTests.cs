using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class GartG3RenderingTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(string id, ulong seed = 42,
        IReadOnlyDictionary<string, string>? changes = null) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(184, 120), seed, changes);

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output);
        return output.ToArray();
    }

    [Theory]
    [InlineData("fascia-ribbons")]
    [InlineData("eclectic-trails")]
    public void FullFramePngIsDeterministicAndSeedDependent(string id)
    {
        byte[] first = Png(Make(id));
        Assert.Equal(first, Png(Make(id)));
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(id, 99))));
        using SKBitmap image = SKBitmap.Decode(first) ?? throw new InvalidOperationException("PNG non valido.");
        Assert.Equal(184, image.Width);
        Assert.Equal(120, image.Height);
        Assert.True(image.Pixels.Distinct().Count() > 2);
    }

    [Theory]
    [InlineData("fascia-ribbons", "fade", "0")]
    [InlineData("fascia-ribbons", "band-width", "0.11")]
    [InlineData("fascia-ribbons", "palette", "copper")]
    [InlineData("fascia-ribbons", "ink-color-2", "#FFFF1111")]
    [InlineData("eclectic-trails", "stroke-width", "20")]
    [InlineData("eclectic-trails", "separation", "5")]
    [InlineData("eclectic-trails", "palette", "neon")]
    [InlineData("eclectic-trails", "trail-color-1", "#FFFF1111")]
    public void CreativeControlsChangeThePng(string id, string key, string value)
    {
        byte[] before = Png(Make(id));
        byte[] after = Png(Make(id, changes: new Dictionary<string, string> { [key] = value }));
        Assert.False(before.AsSpan().SequenceEqual(after));
    }

    [Theory]
    [InlineData("fascia-ribbons")]
    [InlineData("eclectic-trails")]
    public void FineTwoXAndFinishesKeepJsonDeterministic(string id)
    {
        var recipe = Make(id, 77, new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x",
            ["grain-amount"] = "0.01",
            ["paper-texture"] = "cotton",
            ["paper-strength"] = "0.04"
        });
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
        Assert.Equal(Png(recipe), Png(loaded));
    }

    [Theory]
    [InlineData("fascia-ribbons")]
    [InlineData("eclectic-trails")]
    public void CancelledRenderLeavesDestinationEmpty(string id)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(id), output,
            cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void BothWorkBudgetsRejectExcessiveConfigurations()
    {
        ArtworkRecipe fascia = ArtworkRecipe.Create(Catalog.Get("fascia-ribbons"),
            new CanvasSize(4096, 4096), 42,
            new Dictionary<string, string> { ["bands"] = "70", ["steps"] = "700",
                ["band-width"] = "0.16", ["droplet-size"] = "0.5" });
        ArtworkRecipe eclectic = Make("eclectic-trails", changes:
            new Dictionary<string, string> { ["trails"] = "500", ["steps"] = "800" });
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(fascia));
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(eclectic));
    }
}
