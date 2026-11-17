using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class EightKRecipeFixturesTests
{
    [Fact]
    public void FourEightKStudiesAreValidVersionOneDocuments()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "EightKRecipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(4, files.Length);
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), catalog);
            Assert.Equal(7680, recipe.Size.Width);
            Assert.Equal(4320, recipe.Size.Height);
            Assert.Equal(1, recipe.FormatVersion);
            Assert.Null(recipe.Animation);
            Assert.Equal("standard", recipe.Parameters["render-quality"]);
            Assert.Equal("0", recipe.Parameters["grain-amount"]);
            Assert.Equal("none", recipe.Parameters["dither-mode"]);
            Assert.Equal("none", recipe.Parameters["paper-texture"]);
            Assert.Equal("0", recipe.Parameters["paper-strength"]);
            Assert.Equal("0", recipe.Parameters["pigment-granulation"]);
            Assert.Equal("0", recipe.Parameters["ink-absorption"]);
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
