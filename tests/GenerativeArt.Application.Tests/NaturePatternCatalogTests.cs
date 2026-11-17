using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class NaturePatternCatalogTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("phyllotaxis", "botanic")]
    [InlineData("reaction-diffusion", "tidal")]
    public void NewGeneratorHasEmbeddedDefaultAndCompleteParameters(string id, string palette)
    {
        ArtworkRecipe recipe = StudioRecipeDefaults.RecipeFor(id);
        Assert.Equal(1920, recipe.Size.Width);
        Assert.Equal(1080, recipe.Size.Height);
        Assert.Equal(palette, recipe.Parameters["palette"]);
        Assert.Equal(Catalog.Get(id).Parameters.Count, recipe.Parameters.Count);
        Assert.Equal("standard", recipe.Parameters["render-quality"]);
        Assert.Null(recipe.Animation);
    }

    [Fact]
    public void EightArtistStudiesUseJsonV1AndRoundTrip()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "NaturePatternRecipes");
        string[] files = Directory.GetFiles(path, "*.json");
        Assert.Equal(8, files.Length);
        var seeds = new HashSet<ulong>();
        int phyllotaxis = 0, reaction = 0;
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), Catalog);
            Assert.True(recipe.ArtworkId is "phyllotaxis" or "reaction-diffusion");
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            Assert.True(seeds.Add(recipe.Seed));
            Assert.Equal("standard", recipe.Parameters["render-quality"]);
            if (recipe.ArtworkId == "phyllotaxis") phyllotaxis++; else reaction++;
            ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), restored.Parameters.OrderBy(p => p.Key));
        }
        Assert.Equal(4, phyllotaxis);
        Assert.Equal(4, reaction);
    }

    [Fact]
    public void ConditionalControlsAreExplainedWithoutMutatingTheRecipe()
    {
        ArtworkRecipe a = StudioRecipeDefaults.RecipeFor("phyllotaxis");
        Assert.NotNull(StudioParameterApplicability.InactiveReason("phyllotaxis", "arms", a.Parameters));
        var b = StudioRecipeDefaults.RecipeFor("reaction-diffusion");
        Assert.NotNull(StudioParameterApplicability.InactiveReason("reaction-diffusion", "bands", b.Parameters));
        Assert.Null(StudioParameterApplicability.InactiveReason("reaction-diffusion", "feed", b.Parameters));
    }

    [Fact]
    public void PersonalPresetRemainsUntouched()
    {
        Assert.Equal("#9E68258D", StudioRecipeDefaults.RecipeFor("noise-flow").Parameters["ink-color-1"]);
        Assert.Equal("fine-2x", StudioRecipeDefaults.RecipeFor("voronoi-mosaic").Parameters["render-quality"]);
        Assert.Equal(6_582_940_917_790_807_322UL, StudioRecipeDefaults.RecipeFor("domain-warp").Seed);
    }
}
