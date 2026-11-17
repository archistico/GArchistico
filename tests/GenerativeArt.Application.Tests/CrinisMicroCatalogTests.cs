using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class CrinisMicroCatalogTests
{
    private static readonly ArtworkRegistry Registry = BuiltInArtworkCatalog.Create();

    [Fact]
    public void StudioDefaultIsMicroButHasNoOpaqueBandBody()
    {
        ArtworkRecipe recipe = StudioRecipeDefaults.RecipeFor("crinis-bristles");
        Assert.Equal("micro", recipe.Parameters["detail-style"]);
        Assert.Equal("0", recipe.Parameters["body-opacity"]);
        Assert.Equal("3.2", recipe.Parameters["hair-width"]);
        Assert.Equal("0.28", recipe.Parameters["bundle-compression"]);
        Assert.Equal("0.68", recipe.Parameters["bundle-volume"]);
        Assert.Equal("0.82", recipe.Parameters["bundle-lighting"]);
        Assert.Equal("0.85", recipe.Parameters["intra-bundle-depth"]);
        Assert.Equal("0.82", recipe.Parameters["featured-fibers"]);
        Assert.Equal("0.88", recipe.Parameters["cross-section-light"]);
        Assert.Equal("0.72", recipe.Parameters["internal-occlusion"]);
        Assert.Equal(1920, recipe.Size.Width);
        Assert.Equal(1080, recipe.Size.Height);
        Assert.Equal(Registry.Get(recipe.ArtworkId).Parameters.Count, recipe.Parameters.Count);
    }

    [Fact]
    public void PreviousJsonRecipesWithoutNewControlsStillLoad()
    {
        ArtworkRecipe example = ArtworkRecipe.Create(Registry.Get("crinis-bristles"),
            new GenerativeArt.Core.Geometry.CanvasSize(180, 120), 42);
        using var doc = System.Text.Json.JsonDocument.Parse(ArtworkRecipeJson.Serialize(example, Registry));
        var root = System.Text.Json.Nodes.JsonNode.Parse(doc.RootElement.GetRawText())!.AsObject();
        var parameters = root["parameters"]!.AsObject();
        foreach (string id in new[] { "body-opacity", "fiber-relief", "fiber-fray",
                     "detail-style", "fiber-density", "fiber-coverage", "pigment-contrast",
                     "depth-lighting", "rear-softness", "micro-detail",
                     "fiber-taper", "fiber-shadow-depth", "vortex-centers", "ink-flecks",
                     "bundle-compression", "bundle-volume", "bundle-lighting",
                     "intra-bundle-depth", "featured-fibers", "cross-section-light", "internal-occlusion" })
            parameters.Remove(id);
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(root.ToJsonString(), Registry);
        Assert.Equal("classic", loaded.Parameters["detail-style"]);
        Assert.Equal("1", loaded.Parameters["fiber-density"]);
        Assert.Equal("0.16", loaded.Parameters["body-opacity"]);
        Assert.Equal("0.8", loaded.Parameters["fiber-coverage"]);
        Assert.Equal("0.6", loaded.Parameters["pigment-contrast"]);
        Assert.Equal("0.68", loaded.Parameters["depth-lighting"]);
        Assert.Equal("0.32", loaded.Parameters["rear-softness"]);
        Assert.Equal("0", loaded.Parameters["bundle-compression"]);
        Assert.Equal("0", loaded.Parameters["bundle-volume"]);
        Assert.Equal("0", loaded.Parameters["bundle-lighting"]);
        Assert.Equal("0", loaded.Parameters["intra-bundle-depth"]);
        Assert.Equal("0", loaded.Parameters["featured-fibers"]);
        Assert.Equal("0", loaded.Parameters["cross-section-light"]);
        Assert.Equal("0", loaded.Parameters["internal-occlusion"]);
    }

    [Theory]
    [InlineData("CrinisR2Recipes", 4)]
    [InlineData("CrinisMicroRecipes", 6)]
    [InlineData("CrinisR5Recipes", 3)]
    [InlineData("CrinisR7Recipes", 2)]
    [InlineData("CrinisR8Recipes", 2)]
    [InlineData("CrinisR9Recipes", 3)]
    [InlineData("CrinisR10Recipes", 3)]
    public void CrinisExamplesAreStrictValidJsonV1(string directory, int count)
    {
        string path = Path.Combine(AppContext.BaseDirectory, directory);
        string[] recipes = Directory.GetFiles(path, "*.json");
        Assert.Equal(count, recipes.Length);
        foreach (string file in recipes)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), Registry);
            Assert.Equal("crinis-bristles", recipe.ArtworkId);
            Assert.Contains(recipe.Parameters["detail-style"], new[] { "classic", "micro" });
        }
    }
}
