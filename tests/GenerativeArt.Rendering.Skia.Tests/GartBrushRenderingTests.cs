using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class GartBrushRenderingTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    private static ArtworkRecipe Make(string id, ulong seed = 42,
        IReadOnlyDictionary<string, string>? changes = null) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(184, 120), seed, changes);

    private static byte[] Png(ArtworkRecipe r)
    {
        using var stream = new MemoryStream();
        Engine.Render(r, stream);
        return stream.ToArray();
    }

    [Theory]
    [InlineData("crinis-bristles")]
    [InlineData("spiral-flamebrush")]
    public void BrushGeneratorIsSeededAndRendersFullFrame(string id)
    {
        byte[] first = Png(Make(id));
        Assert.Equal(first, Png(Make(id)));
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(id, 99))));
        using SKBitmap bitmap = SKBitmap.Decode(first) ?? throw new InvalidOperationException("PNG non valido.");
        Assert.Equal(184, bitmap.Width);
        Assert.Equal(120, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 2);
    }

    [Theory]
    [InlineData("crinis-bristles", "turn", "2.3")]
    [InlineData("crinis-bristles", "band-width", "0.42")]
    [InlineData("crinis-bristles", "palette", "copper")]
    [InlineData("crinis-bristles", "fiber-color-3", "#FFFF1111")]
    [InlineData("crinis-bristles", "body-opacity", "0")]
    [InlineData("crinis-bristles", "fiber-relief", "0.9")]
    [InlineData("crinis-bristles", "fiber-fray", "0.8")]
    [InlineData("spiral-flamebrush", "spiral-turns", "6")]
    [InlineData("spiral-flamebrush", "rings", "4")]
    [InlineData("spiral-flamebrush", "palette", "ember")]
    [InlineData("spiral-flamebrush", "particle-color-2", "#FFFF1111")]
    public void CreativeControlsAlterImage(string id, string key, string value)
    {
        byte[] standard = Png(Make(id));
        byte[] modified = Png(Make(id, changes: new Dictionary<string, string> { [key] = value }));
        Assert.False(standard.AsSpan().SequenceEqual(modified));
    }

    [Theory]
    [InlineData("crinis-bristles")]
    [InlineData("spiral-flamebrush")]
    public void JsonFine2xAndMaterialsMaintainDeterministicOutput(string id)
    {
        ArtworkRecipe original = Make(id, 77, new Dictionary<string, string>
        {
            ["render-quality"] = "fine-2x", ["grain-amount"] = "0.012",
            ["paper-texture"] = "cotton", ["paper-strength"] = "0.06"
        });
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(original, Catalog), Catalog);
        Assert.Equal(Png(original), Png(restored));
    }

    [Theory]
    [InlineData("crinis-bristles")]
    [InlineData("spiral-flamebrush")]
    public void CancellationDoesNotWritePartialOutput(string id)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(id), output,
            cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void RejectsExcessiveBristleOrParticleWorkBeforeAllocatingPng()
    {
        var crinis = Make("crinis-bristles", changes: new Dictionary<string, string>
        {
            ["bands"] = "40", ["samples"] = "700", ["band-width"] = "0.55",
            ["bristle-pitch"] = "1.5"
        });
        // Il budget è indipendente dal raster, quindi forzare un canvas grande senza
        // generare l'immagine completa.
        var huge = ArtworkRecipe.Create(Catalog.Get("crinis-bristles"), new CanvasSize(4096, 4096),
            42, crinis.Parameters);
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(huge));
        var flame = Make("spiral-flamebrush", changes: new Dictionary<string, string>
        {
            ["rings"] = "25", ["stamps"] = "300", ["particles"] = "60"
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Validate(flame));
    }
}
