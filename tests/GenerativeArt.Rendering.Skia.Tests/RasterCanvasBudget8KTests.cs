using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

/// <summary>
/// Solo validazione: la suite ordinaria non deve allocare 8K per provare
/// il budget. I rendering 8K reali sono coperti dallo smoke manuale.
/// </summary>
public sealed class RasterCanvasBudget8KTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Renderer = new(Catalog);

    private static ArtworkRecipe Make(string id, int width, int height,
        IReadOnlyDictionary<string, string>? options = null) =>
        ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(width, height), 42, options);

    [Theory]
    [InlineData("guilloche")]
    [InlineData("spaced-streamlines")]
    [InlineData("voronoi-mosaic")]
    [InlineData("poisson-stippling")]
    public void FourDirectSkiaRenderersAcceptEightKUhdWithoutAllocating(string id)
    {
        ArtworkRecipe landscape = Make(id, 7680, 4320);
        ArtworkRecipe portrait = Make(id, 4320, 7680);
        Renderer.Validate(landscape);
        Renderer.Validate(portrait);
        Assert.True(RasterCanvasBudgetPolicy.SupportsEightK(id));
        Assert.Equal(33_177_600L, landscape.Size.PixelCount);
    }

    [Theory]
    [InlineData("wave-interference")]
    [InlineData("plasma-noise")]
    [InlineData("sdf-orbits")]
    [InlineData("parametric-curves")]
    [InlineData("topographic-contours")]
    [InlineData("tonal-engraving")]
    [InlineData("domain-warp")]
    public void RasterHeavyRenderersKeepTheirFormerFourKLimit(string id)
    {
        ArtworkRecipe recipe = Make(id, 7680, 4320);
        var failure = Assert.Throws<ArgumentOutOfRangeException>(() => Renderer.Validate(recipe));
        Assert.Equal("recipe", failure.ParamName);
        Assert.False(RasterCanvasBudgetPolicy.SupportsEightK(id));
    }

    [Theory]
    [InlineData(8192, 8192)]
    [InlineData(8193, 100)]
    [InlineData(8192, 4096)]
    public void RejectsExcessiveDimensionsBeforeWritingAnyBytes(int width, int height)
    {
        var recipe = Make("voronoi-mosaic", width, height);
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Renderer.Render(recipe, output));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void EightKNeedsStandardSamplingBecauseFineTwoXHasItsOwnBudget()
    {
        ArtworkRecipe recipe = Make("guilloche", 7680, 4320,
            new Dictionary<string, string> { ["render-quality"] = "fine-2x" });
        Assert.Throws<ArgumentOutOfRangeException>(() => Renderer.Validate(recipe));
    }

    [Theory]
    [InlineData("grain-amount", "0.02")]
    [InlineData("paper-texture", "rough")]
    [InlineData("pigment-granulation", "0.05")]
    [InlineData("dither-mode", "bayer4")]
    [InlineData("ink-absorption", "0.12")]
    public void EightKArtisticFinishesUseScanlineMemoryBudget(string key, string value)
    {
        var options = new Dictionary<string, string> { [key] = value };
        if (key == "paper-texture") options["paper-strength"] = "0.1";
        foreach (string id in new[] { "voronoi-mosaic", "guilloche", "spaced-streamlines", "poisson-stippling" })
            Renderer.Validate(Make(id, 7680, 4320, options));
    }

    [Fact]
    public void RasterHeavyRenderersRemainFourKEvenWhenFinishingIsEnabled()
    {
        var settings = new Dictionary<string, string> { ["grain-amount"] = "0.08" };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Renderer.Validate(Make("plasma-noise", 7680, 4320, settings)));
    }

    [Fact]
    public void FourKAndSmallArtworksRemainSupportedAcrossOtherRenderers()
    {
        Renderer.Validate(Make("wave-interference", 128, 96));
        Renderer.Validate(Make("sdf-orbits", 128, 96));
        Renderer.Validate(Make("plasma-noise", 128, 96));
        Renderer.Validate(Make("voronoi-mosaic", 4096, 4096));
    }

    [Fact]
    public void NativePreviewAndOutputKeepSameEightKRecipe()
    {
        var recipe = Make("guilloche", 7680, 4320);
        ArtworkRecipe preview = GenerativeArt.Application.Rendering.StudioPreviewPolicy
            .CreateExactPreview(recipe, Catalog);
        Assert.Same(recipe, preview);
        Renderer.Validate(preview);
    }
}
