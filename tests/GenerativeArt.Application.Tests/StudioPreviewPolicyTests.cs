using Xunit;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Tests;

public sealed class StudioPreviewPolicyTests
{
    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();

    [Fact]
    public void UserCanRequestFullSizeFiveMegapixelPreviewWithoutChangingExportRecipe()
    {
        var original = ArtworkRecipe.Create(_catalog.Get("plasma-noise"),
            new CanvasSize(2500, 2000), 2718, new Dictionary<string, string>
            { ["grain-amount"] = "0.08" });
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(original, _catalog,
            maximumWidth: 2500, maximumHeight: 2000, maximumPixels: 5_000_000);
        Assert.Equal(original.Size, preview.Size);
        Assert.Equal(original.Seed, preview.Seed);
        Assert.Equal(original.Parameters["grain-amount"], preview.Parameters["grain-amount"]);
        Assert.Equal(2500, original.Size.Width);
    }

    [Fact]
    public void UserConfiguredPreviewResolutionPreservesAspectRatio()
    {
        var original = ArtworkRecipe.Create(_catalog.Get("parametric-curves"),
            new CanvasSize(2500, 2000), 12);
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(original, _catalog,
            maximumWidth: 1200, maximumHeight: 800, maximumPixels: 960_000);
        Assert.Equal(1000, preview.Size.Width);
        Assert.Equal(800, preview.Size.Height);
        Assert.Equal(new CanvasSize(2500, 2000), original.Size);
    }

    [Fact]
    public void ResizingStaticPreservesSeedAndParametersWithoutChangingSource()
    {
        var descriptor = _catalog.Get("plasma-noise");
        var recipe = ArtworkRecipe.Create(descriptor, new CanvasSize(3200, 2400), 928UL,
            new Dictionary<string, string> { ["palette"] = "moss", ["octaves"] = "6" });
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(recipe, _catalog);
        Assert.Equal(800, preview.Size.Width);
        Assert.Equal(600, preview.Size.Height);
        Assert.Equal(928UL, preview.Seed);
        Assert.Equal("moss", preview.Parameters["palette"]);
        Assert.Equal("6", preview.Parameters["octaves"]);
        Assert.Equal(3200, recipe.Size.Width);
    }

    [Fact]
    public void AnimationPreservesFrameRateAndCount()
    {
        var recipe = ArtworkRecipe.Create(_catalog.Get("looping-flow"), new CanvasSize(1024, 1024),
            42UL, animation: new RecipeAnimation(24, 12));
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(recipe, _catalog);
        Assert.True(preview.Size.PixelCount <= 480_000L);
        Assert.True(preview.Size.Width >= 64);
        Assert.Equal(24, preview.Animation!.FrameCount);
        Assert.Equal(12, preview.Animation.FramesPerSecond);
    }

    [Fact]
    public void SmallArtworkRemainsAtNativeDimensions()
    {
        var recipe = ArtworkRecipe.Create(_catalog.Get("parametric-curves"),
            new CanvasSize(256, 128), 12UL);
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(recipe, _catalog);
        Assert.Equal(recipe.Size, preview.Size);
    }

    [Fact]
    public void RejectsInvalidPreviewBudgetsAndTooSmallAnimatedPreviews()
    {
        var recipe = ArtworkRecipe.Create(_catalog.Get("looping-flow"), new CanvasSize(128, 128),
            1UL, animation: new RecipeAnimation(24, 24));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StudioPreviewPolicy.CreatePreview(recipe, _catalog, maximumPixels: 100));
        var narrow = ArtworkRecipe.Create(_catalog.Get("looping-flow"),
            new CanvasSize(4096, 64), 1UL, animation: new RecipeAnimation(24, 24));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StudioPreviewPolicy.CreatePreview(narrow, _catalog, maximumWidth: 64,
                maximumHeight: 64, maximumPixels: 4096));
    }
}
