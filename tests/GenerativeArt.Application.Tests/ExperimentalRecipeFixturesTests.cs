using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using Xunit;

namespace GenerativeArt.Application.Tests;

/// <summary>
/// Le varianti creative sono documenti v1 autonomi e possono essere aperte
/// nello Studio senza alterare i 17 preset incorporati.
/// </summary>
public sealed class ExperimentalRecipeFixturesTests
{
    [Fact]
    public void TenNewArtisticRecipesLoadWithTheirOriginalSeedAndDimensions()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "ExperimentalRecipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(10, files.Length);
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        var seeds = new HashSet<ulong>();
        var generators = new HashSet<string>(StringComparer.Ordinal);
        foreach (string file in files)
        {
            byte[] data = File.ReadAllBytes(file);
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(data, catalog);
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            Assert.True(seeds.Add(recipe.Seed), "Seed duplicato: " + file);
            Assert.Equal("standard", recipe.Parameters["render-quality"]);
            generators.Add(recipe.ArtworkId);
            ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(
                ArtworkRecipeJson.Serialize(recipe, catalog), catalog);
            Assert.Equal(recipe.Seed, restored.Seed);
            Assert.Equal(recipe.Parameters.OrderBy(x => x.Key), restored.Parameters.OrderBy(x => x.Key));
        }
        Assert.Equal(9, generators.Count); // SDF è presente con due varianti distinte.
    }
}
