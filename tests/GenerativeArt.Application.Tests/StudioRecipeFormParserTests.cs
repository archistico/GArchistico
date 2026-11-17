using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioRecipeFormParserTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Fact]
    public void ParsingStaticEditorKeepsAllParametersAndAllowsFiveMegapixels()
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("noise-flow");
        ArtworkRecipe result = StudioRecipeFormParser.Parse(Catalog.Get(preset.ArtworkId),
            "123", "2500", "2000", preset.Parameters, Array.Empty<string>());
        Assert.Equal((ulong)123, result.Seed);
        Assert.Equal(2500, result.Size.Width);
        Assert.Equal(2000, result.Size.Height);
        Assert.Null(result.Animation);
        Assert.Equal(preset.Parameters.Count, result.Parameters.Count);
        foreach (var entry in preset.Parameters)
            Assert.Equal(entry.Value, result.Parameters[entry.Key]);
    }

    [Fact]
    public void ParsingAnimatedEditorPreservesExplicitTimeline()
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("looping-flow");
        ArtworkRecipe result = StudioRecipeFormParser.Parse(Catalog.Get(preset.ArtworkId),
            preset.Seed.ToString(), "1920", "1080", preset.Parameters,
            Array.Empty<string>(), "16", "18");
        Assert.Equal(16, result.Animation?.FrameCount);
        Assert.Equal(18, result.Animation?.FramesPerSecond);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("18446744073709551616")]
    public void ParsingRejectsNonUInt64Seeds(string invalidSeed)
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("noise-flow");
        Assert.Throws<ArgumentException>(() => StudioRecipeFormParser.Parse(
            Catalog.Get(preset.ArtworkId), invalidSeed, "1920", "1080",
            preset.Parameters, Array.Empty<string>()));
    }

    [Fact]
    public void ParsingRejectsPartialNumericEditorInputWithoutPublishingRecipe()
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("noise-flow");
        Assert.Throws<ArgumentException>(() => StudioRecipeFormParser.Parse(
            Catalog.Get(preset.ArtworkId), "42", "1920", "1080",
            preset.Parameters, new[] { "frequency" }));
    }

    [Fact]
    public void ParsingKeepsFullUnsignedDomainWarpSeed()
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("domain-warp");
        ArtworkRecipe result = StudioRecipeFormParser.Parse(Catalog.Get(preset.ArtworkId),
            "6582940917790807322", "1920", "1080", preset.Parameters, Array.Empty<string>());
        Assert.Equal(6582940917790807322UL, result.Seed);
    }

    [Fact]
    public void ParsingRejectsOutOfRangeCanvasBeforeAnyRendering()
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor("plasma-noise");
        Assert.Throws<ArgumentOutOfRangeException>(() => StudioRecipeFormParser.Parse(
            Catalog.Get(preset.ArtworkId), "42", "10000", "10000", preset.Parameters,
            Array.Empty<string>()));
    }
}
