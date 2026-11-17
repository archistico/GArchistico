using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class WaveInterferenceFixturesTests
{
    [Fact]
    public void FiveFullHdRecipesAreValidAndPreserveTheirSeeds()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "WaveInterferenceRecipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(5, files.Length);
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        var seeds = new HashSet<ulong>();
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), catalog);
            Assert.Equal("wave-interference", recipe.ArtworkId);
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            Assert.True(seeds.Add(recipe.Seed));
            Assert.Equal("standard", recipe.Parameters["render-quality"]);
            ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(
                ArtworkRecipeJson.Serialize(recipe, catalog), catalog);
            Assert.Equal(recipe.Seed, restored.Seed);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), restored.Parameters.OrderBy(p => p.Key));
        }
    }

    [Fact]
    public void NewGeneratorHasOwnEmbeddedDefaultWithoutChangingUserDefaults()
    {
        var initial = StudioRecipeDefaults.RecipeFor("wave-interference");
        Assert.Equal(42UL, initial.Seed);
        Assert.Equal(1920, initial.Size.Width);
        Assert.Equal("ring", initial.Parameters["layout"]);
        Assert.Equal("lagoon", initial.Parameters["palette"]);
        Assert.Equal("#9E68258D", StudioRecipeDefaults.RecipeFor("noise-flow").Parameters["ink-color-1"]);
    }
}
