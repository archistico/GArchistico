using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class EightKFinishRecipeFixturesTests
{
    [Fact]
    public void ThreeEightKFinishRecipesRoundTripWithoutChangingPresetDefaults()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "EightKFinishRecipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(3, files.Length);
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), catalog);
            Assert.Equal(7680, recipe.Size.Width);
            Assert.Equal(4320, recipe.Size.Height);
            Assert.Null(recipe.Animation);
            Assert.Equal("standard", recipe.Parameters["render-quality"]);
            Assert.NotEqual("0", recipe.Parameters["grain-amount"]);
            ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(
                ArtworkRecipeJson.Serialize(recipe, catalog), catalog);
            Assert.Equal(recipe.ArtworkId, restored.ArtworkId);
            Assert.Equal(recipe.Seed, restored.Seed);
            Assert.Equal(recipe.Size, restored.Size);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key),
                restored.Parameters.OrderBy(p => p.Key));
        }
    }
}
