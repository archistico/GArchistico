using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class GartG1RecipeFixturesTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("alien-letters")]
    [InlineData("cotton-circles")]
    [InlineData("rigor-grid")]
    public void DefaultIsEmbeddedAndSerializable(string artwork)
    {
        var recipe = StudioRecipeDefaults.RecipeFor(artwork);
        Assert.Equal(artwork, recipe.ArtworkId);
        Assert.Equal(1920, recipe.Size.Width);
        Assert.Equal(1080, recipe.Size.Height);
        recipe.ValidateAgainst(Catalog);
        var loaded = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
        Assert.Equal(recipe.Seed, loaded.Seed);
        Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), loaded.Parameters.OrderBy(p => p.Key));
    }

    [Fact]
    public void NewGeneratorsLeaveHistoricalPersonalDefaultsUntouched()
    {
        Assert.Equal("#9E68258D", StudioRecipeDefaults.RecipeFor("noise-flow").Parameters["ink-color-1"]);
        Assert.Equal("fine-2x", StudioRecipeDefaults.RecipeFor("voronoi-mosaic").Parameters["render-quality"]);
        Assert.Equal(6_582_940_917_790_807_322UL, StudioRecipeDefaults.RecipeFor("domain-warp").Seed);
    }

    [Fact]
    public void ThreeColorPalettesExposeEveryRoleToTheStudioPicker()
    {
        Assert.Equal(5, Catalog.Get("alien-letters").Parameters.Count(p => p.Kind == ParameterKind.Color));
        Assert.Equal(5, Catalog.Get("cotton-circles").Parameters.Count(p => p.Kind == ParameterKind.Color));
        Assert.Equal(3, Catalog.Get("rigor-grid").Parameters.Count(p => p.Kind == ParameterKind.Color));
    }
}

public sealed class GartG1NineExamplesTests
{
    [Fact]
    public void NineFullHdStudiesAreValidAndRoundTrip()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "GartG1Recipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(9, files.Length);
        var catalog = BuiltInArtworkCatalog.Create();
        var artists = new HashSet<string>(StringComparer.Ordinal);
        var seeds = new HashSet<ulong>();
        foreach (string path in files)
        {
            ArtworkRecipe original = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), catalog);
            Assert.Equal(1920, original.Size.Width);
            Assert.Equal(1080, original.Size.Height);
            Assert.Contains(original.ArtworkId, new[] { "alien-letters", "cotton-circles", "rigor-grid" });
            Assert.True(seeds.Add(original.Seed), "Seed duplicato: " + path);
            artists.Add(original.ArtworkId);
            ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(original, catalog), catalog);
            Assert.Equal(original.Parameters.OrderBy(p => p.Key), restored.Parameters.OrderBy(p => p.Key));
            Assert.Equal(original.Seed, restored.Seed);
        }
        Assert.Equal(3, artists.Count);
    }
}
