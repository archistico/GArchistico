using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class NaturePatternRendererTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(string id, ulong seed = 42,
        IReadOnlyDictionary<string, string>? parameters = null, int width = 160, int height = 112)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (id == "reaction-diffusion")
        {
            values.Add("cell-size", "5");
            values.Add("iterations", "120");
            values.Add("inoculations", "14");
            values.Add("inoculation-radius", "3");
        }
        if (parameters is not null)
            foreach (var pair in parameters) values[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), seed, values);
    }

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var buffer = new MemoryStream();
        Engine.Render(recipe, buffer);
        return buffer.ToArray();
    }

    [Theory]
    [InlineData("phyllotaxis")]
    [InlineData("reaction-diffusion")]
    public void NewRenderersAreDeterministicAndProduceRichPng(string id)
    {
        var recipe = Make(id);
        byte[] first = Png(recipe);
        Assert.Equal(first, Png(recipe));
        using SKBitmap bitmap = SKBitmap.Decode(first) ?? throw new InvalidOperationException("PNG non leggibile.");
        Assert.Equal(160, bitmap.Width);
        Assert.Equal(112, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 8);
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(id, seed: 987654UL))));
    }

    [Theory]
    [InlineData("phyllotaxis", "seeds", "800")]
    [InlineData("phyllotaxis", "shape", "galaxy")]
    [InlineData("phyllotaxis", "point-style", "petal")]
    [InlineData("phyllotaxis", "angle", "121.5")]
    [InlineData("phyllotaxis", "palette", "petals")]
    [InlineData("phyllotaxis", "seed-color-2", "#80FF58BB")]
    [InlineData("reaction-diffusion", "feed", "0.028")]
    [InlineData("reaction-diffusion", "iterations", "180")]
    [InlineData("reaction-diffusion", "style", "contours")]
    [InlineData("reaction-diffusion", "palette", "lichen")]
    [InlineData("reaction-diffusion", "gradient-color-3", "#A0D2C1F0")]
    public void CreativeControlsAlterTheRaster(string id, string key, string value)
    {
        byte[] basePng = Png(Make(id));
        byte[] changed = Png(Make(id, parameters: new Dictionary<string, string> { [key] = value }));
        Assert.False(basePng.AsSpan().SequenceEqual(changed));
    }

    [Theory]
    [InlineData("phyllotaxis")]
    [InlineData("reaction-diffusion")]
    public void FineRasterAndJsonRoundTripKeepFinalDimensions(string id)
    {
        ArtworkRecipe original = Make(id, parameters: new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x", ["grain-amount"] = "0.04",
            ["paper-texture"] = "cotton", ["paper-strength"] = "0.08"
        });
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(original, Catalog), Catalog);
        byte[] first = Png(restored);
        Assert.Equal(first, Png(original));
        using SKBitmap bitmap = SKBitmap.Decode(first) ?? throw new InvalidOperationException();
        Assert.Equal(original.Size.Width, bitmap.Width);
        Assert.Equal(original.Size.Height, bitmap.Height);
    }

    [Fact]
    public void ReactionDiffusionLargeSimulationIsRejectedBeforeOutput()
    {
        ArtworkRecipe oversized = Make("reaction-diffusion", width: 4096, height: 4096,
            parameters: new Dictionary<string, string> { ["cell-size"] = "3", ["iterations"] = "1200" });
        using var destination = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(oversized, destination));
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData("phyllotaxis")]
    [InlineData("reaction-diffusion")]
    public void CancellationDoesNotWritePartialPng(string id)
    {
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        using var destination = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(id), destination,
            cancellationToken: canceled.Token));
        Assert.Empty(destination.ToArray());
    }
}
