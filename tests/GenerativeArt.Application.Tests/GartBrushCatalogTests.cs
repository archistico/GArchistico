using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class GartBrushCatalogTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("crinis-bristles")]
    [InlineData("spiral-flamebrush")]
    public void EmbeddedPresetContainsValidFullHdRecipe(string id)
    {
        ArtworkRecipe recipe = StudioRecipeDefaults.RecipeFor(id);
        Assert.Equal(1920, recipe.Size.Width);
        Assert.Equal(1080, recipe.Size.Height);
        Assert.Equal(Catalog.Get(id).Parameters.Count, recipe.Parameters.Count);
        Assert.Equal("standard", recipe.Parameters["render-quality"]);
        Assert.Null(recipe.Animation);
        Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), ArtworkRecipeJson.Deserialize(
            ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog).Parameters.OrderBy(p => p.Key));
    }

    [Fact]
    public void EightGartBrushStudiesRemainValidJsonV1()
    {
        var files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "GartG2Recipes"), "*.json");
        Assert.Equal(8, files.Length);
        var seeds = new HashSet<ulong>();
        int bristles = 0, spirals = 0;
        foreach (string file in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), Catalog);
            Assert.True(seeds.Add(recipe.Seed));
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            if (recipe.ArtworkId == "crinis-bristles") bristles++;
            else if (recipe.ArtworkId == "spiral-flamebrush") spirals++;
            else Assert.Fail($"Generatore inatteso: {recipe.ArtworkId}");
        }
        Assert.Equal(4, bristles);
        Assert.Equal(4, spirals);
    }
}
