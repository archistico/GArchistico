using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class GartG3CatalogTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("fascia-ribbons")]
    [InlineData("eclectic-trails")]
    public void G3EmbeddedPresetIsFullHdAndRoundTrips(string id)
    {
        ArtworkRecipe recipe = StudioRecipeDefaults.RecipeFor(id);
        Assert.Equal(1920, recipe.Size.Width);
        Assert.Equal(1080, recipe.Size.Height);
        Assert.Null(recipe.Animation);
        Assert.Equal(Catalog.Get(id).Parameters.Count, recipe.Parameters.Count);
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
        Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), restored.Parameters.OrderBy(p => p.Key));
    }

    [Fact]
    public void EightG3RecipesUseDistinctSeedsAndValidJson()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "GartG3Recipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(8, files.Length);
        var seeds = new HashSet<ulong>();
        int fascia = 0, eclectic = 0;
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), Catalog);
            Assert.True(seeds.Add(recipe.Seed));
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            if (recipe.ArtworkId == "fascia-ribbons") fascia++;
            else if (recipe.ArtworkId == "eclectic-trails") eclectic++;
            else Assert.Fail($"Generatore inatteso: {recipe.ArtworkId}");
        }
        Assert.Equal(4, fascia);
        Assert.Equal(4, eclectic);
    }
}
