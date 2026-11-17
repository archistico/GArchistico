using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class WaveInterferenceRendererTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Renderer = new(Catalog);

    private static ArtworkRecipe Make(ulong seed = 42, int width = 128, int height = 96,
        IReadOnlyDictionary<string, string>? parameters = null) =>
        ArtworkRecipe.Create(Catalog.Get("wave-interference"), new CanvasSize(width, height), seed, parameters);

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var bytes = new MemoryStream();
        Renderer.Render(recipe, bytes);
        return bytes.ToArray();
    }

    [Fact]
    public void PngHasRequestedDimensionsAndStableBytesAcrossRenders()
    {
        var recipe = Make();
        byte[] first = Png(recipe);
        using SKBitmap bitmap = SKBitmap.Decode(first) ?? throw new InvalidOperationException();
        Assert.Equal(128, bitmap.Width);
        Assert.Equal(96, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 30);
        Assert.Equal(first, Png(recipe));
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(seed: 43))));
    }

    [Theory]
    [InlineData("sources", "9")]
    [InlineData("frequency", "28")]
    [InlineData("layout", "scatter")]
    [InlineData("mode", "caustics")]
    [InlineData("distortion", "0")]
    [InlineData("palette", "nocturne")]
    [InlineData("gradient-color-2", "#FF4288")]
    public void ArtisticControlsChangeTheRaster(string key, string value)
    {
        byte[] baseline = Png(Make());
        byte[] changed = Png(Make(parameters: new Dictionary<string, string> { [key] = value }));
        Assert.False(baseline.AsSpan().SequenceEqual(changed));
    }

    [Fact]
    public void JsonRoundTripAndFine2xRetainOutputSize()
    {
        var recipe = Make(parameters: new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x", ["paper-texture"] = "cotton",
            ["paper-strength"] = "0.1", ["gradient-color-3"] = "#A070D8F0"
        });
        byte[] roundtrip = ArtworkRecipeJson.Serialize(recipe, Catalog);
        var loaded = ArtworkRecipeJson.Deserialize(roundtrip, Catalog);
        Assert.Equal(Png(recipe), Png(loaded));
        using SKBitmap bitmap = SKBitmap.Decode(Png(loaded)) ?? throw new InvalidOperationException();
        Assert.Equal(recipe.Size.Width, bitmap.Width);
        Assert.Equal(recipe.Size.Height, bitmap.Height);
    }

    [Fact]
    public void CancellationPreservesEmptyDestination()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        using var destination = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Renderer.Render(Make(), destination,
            cancellationToken: cancelled.Token));
        Assert.Empty(destination.ToArray());
    }

    [Fact]
    public void ExpensiveCombinationsAreRejectedBeforeRendering()
    {
        Renderer.Validate(Make(width: 1920, height: 1080,
            parameters: new Dictionary<string, string> { ["sources"] = "12" }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Renderer.Validate(
            Make(width: 4096, height: 4096,
                parameters: new Dictionary<string, string> { ["sources"] = "12" })));
    }
}
