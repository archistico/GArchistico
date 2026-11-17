using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class GartModularRenderingTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(string id, ulong seed = 42,
        IReadOnlyDictionary<string, string>? changes = null)
    {
        var defaults = new Dictionary<string, string>(StringComparer.Ordinal);
        if (changes is not null)
            foreach (var pair in changes) defaults[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(184, 120), seed, defaults);
    }

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var destination = new MemoryStream();
        Engine.Render(recipe, destination);
        return destination.ToArray();
    }

    [Theory]
    [InlineData("alien-letters")]
    [InlineData("cotton-circles")]
    [InlineData("rigor-grid")]
    public void ModularArtworkIsDeterministicAndIncludesArtisticVariation(string id)
    {
        ArtworkRecipe recipe = Make(id);
        byte[] original = Png(recipe);
        Assert.Equal(original, Png(recipe));
        Assert.False(original.AsSpan().SequenceEqual(Png(Make(id, 104729))));
        using SKBitmap bitmap = SKBitmap.Decode(original) ?? throw new InvalidOperationException("PNG non leggibile.");
        Assert.Equal(184, bitmap.Width);
        Assert.Equal(120, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() >= 3);
    }

    [Theory]
    [InlineData("alien-letters", "glyph-density", "0.22")]
    [InlineData("alien-letters", "join-probability", "1")]
    [InlineData("alien-letters", "palette", "neon")]
    [InlineData("alien-letters", "tile-color-2", "#FFEEDDAA")]
    [InlineData("cotton-circles", "motif", "quarter")]
    [InlineData("cotton-circles", "columns", "5")]
    [InlineData("cotton-circles", "palette", "terra")]
    [InlineData("cotton-circles", "tile-color-1", "#FF111111")]
    [InlineData("rigor-grid", "letters", "none")]
    [InlineData("rigor-grid", "period", "5")]
    [InlineData("rigor-grid", "palette", "night")]
    [InlineData("rigor-grid", "ink-color", "#FFF02020")]
    public void ArtisticControlsActuallyChangeTheOutput(string id, string key, string value)
    {
        byte[] before = Png(Make(id));
        byte[] after = Png(Make(id, changes: new Dictionary<string, string> { [key] = value }));
        Assert.False(before.AsSpan().SequenceEqual(after));
    }

    [Theory]
    [InlineData("alien-letters")]
    [InlineData("cotton-circles")]
    [InlineData("rigor-grid")]
    public void JsonFine2xAndFinishingPreserveOutputDimensions(string id)
    {
        var recipe = Make(id, 99, new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x",
            ["grain-amount"] = "0.018",
            ["paper-texture"] = "cotton",
            ["paper-strength"] = "0.05"
        });
        var restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
        byte[] png = Png(restored);
        Assert.Equal(png, Png(recipe));
        using SKBitmap bitmap = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(184, bitmap.Width);
        Assert.Equal(120, bitmap.Height);
    }

    [Theory]
    [InlineData("alien-letters")]
    [InlineData("cotton-circles")]
    [InlineData("rigor-grid")]
    public void CancellationDoesNotWritePartialPng(string id)
    {
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(id), output,
            cancellationToken: stop.Token));
        Assert.Equal(0, output.Length);
    }
}
