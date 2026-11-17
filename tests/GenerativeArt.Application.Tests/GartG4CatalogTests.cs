using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class GartG4CatalogTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public void BothNewGeneratorsHaveEmbeddedFullHdAndCanonicalJson(string id)
    {
        ArtworkRecipe preset = StudioRecipeDefaults.RecipeFor(id);
        Assert.Equal(1920, preset.Size.Width);
        Assert.Equal(1080, preset.Size.Height);
        Assert.Equal(Catalog.Get(id).Parameters.Count, preset.Parameters.Count);
        var restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(preset, Catalog), Catalog);
        Assert.Equal(preset.Seed, restored.Seed);
        Assert.Equal(preset.Parameters.OrderBy(x => x.Key), restored.Parameters.OrderBy(x => x.Key));
    }

    [Fact]
    public void EightG4ExamplesAreStrictValidAndPreserveDifferentSeeds()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "GartG4Recipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(8, files.Length);
        int ocellus = 0, rete = 0;
        var seeds = new HashSet<ulong>();
        foreach (string path in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), Catalog);
            Assert.True(seeds.Add(recipe.Seed));
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            if (recipe.ArtworkId == "ocellus-eyespots") ocellus++;
            else if (recipe.ArtworkId == "rete-foam") rete++;
            else Assert.Fail($"ID estraneo: {recipe.ArtworkId}");
            Assert.Equal(recipe.Parameters.OrderBy(x => x.Key),
                ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog)
                    .Parameters.OrderBy(x => x.Key));
        }
        Assert.Equal(4, ocellus);
        Assert.Equal(4, rete);
    }

    [Fact]
    public void NewIdsAreIndependentOfHistoricalRecipeAndDescriptors()
    {
        Assert.Equal(30, Catalog.All.Count);
        Assert.Equal(1, Catalog.Get("ocellus-eyespots").AlgorithmVersion);
        Assert.Equal(1, Catalog.Get("rete-foam").AlgorithmVersion);
        Assert.Equal("classic", Catalog.Get("crinis-bristles").ValidateParameters()["detail-style"]);
        Assert.Throws<ArgumentException>(() => Catalog.Get("rete-foam")
            .ValidateParameters(new Dictionary<string, string> { ["layers"] = "6" }));
    }

    [Fact]
    public void SixG4R2ExamplesAreStrictValidAndHaveTheirArtisticControls()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "GartG4R2Recipes");
        string[] files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(6, files.Length);
        foreach (string path in files)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), Catalog);
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            Assert.Equal(1, recipe.AlgorithmVersion);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key),
                ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog)
                    .Parameters.OrderBy(p => p.Key));
            string key = recipe.ArtworkId == "ocellus-eyespots" ? "rim-light" : "membrane-opacity";
            Assert.True(double.Parse(recipe.Parameters[key], System.Globalization.CultureInfo.InvariantCulture) > 0);
        }
    }

    [Fact]
    public void G4R2StudioDefaultsAreEnhancedButCatalogRemainsNeutral()
    {
        foreach (var (id, feature) in new[]
        {
            ("ocellus-eyespots", "rim-light"), ("rete-foam", "membrane-opacity")
        })
        {
            Assert.Equal("0", Catalog.Get(id).ValidateParameters()[feature]);
            Assert.NotEqual("0", StudioRecipeDefaults.RecipeFor(id).Parameters[feature]);
        }
    }
}
