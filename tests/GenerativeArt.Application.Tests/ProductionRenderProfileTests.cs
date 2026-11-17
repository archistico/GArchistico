using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class ProductionRenderProfileTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    public static IEnumerable<object[]> Artworks() => Catalog.All.Select(d => new object[] { d.Id });

    [Theory]
    [MemberData(nameof(Artworks))]
    public void FourKMaximizesDetailAndPreservesArtistSettings(string id)
    {
        ArtworkRecipe original = StudioRecipeDefaults.RecipeFor(id);
        byte[] before = ArtworkRecipeJson.Serialize(original, Catalog);
        ArtworkRecipe prepared = ProductionRenderProfile.PrepareFourK(original, Catalog);
        Assert.Equal(new CanvasSize(3840, 2160), prepared.Size);
        Assert.Equal(original.Seed, prepared.Seed);
        Assert.Equal(original.Animation, prepared.Animation);
        Assert.Equal(original.AlgorithmVersion, prepared.AlgorithmVersion);
        Assert.Equal("production", prepared.Parameters["render-profile"]);
        Assert.Equal("fine-2x", prepared.Parameters["render-quality"]);
        Assert.NotEmpty(ProductionRenderProfile.MaximumControls(id));
        foreach (string control in ProductionRenderProfile.MaximumControls(id))
        {
            ArtworkParameterSpec spec = Catalog.Get(id).Parameters.Single(p => p.Id == control);
            Assert.NotNull(spec.Maximum);
            Assert.Equal(spec.Maximum, prepared.Parameters[control]);
        }
        foreach (var pair in original.Parameters.Where(p => p.Key.Contains("color", StringComparison.Ordinal)
            || p.Key is "palette" or "style" or "depth-style" or "pattern"))
            Assert.Equal(pair.Value, prepared.Parameters[pair.Key]);
        Assert.Equal(before, ArtworkRecipeJson.Serialize(original, Catalog));
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(prepared, Catalog), Catalog);
        Assert.Equal(prepared.Parameters, restored.Parameters);
    }

    [Theory]
    [MemberData(nameof(Artworks))]
    public void ProfileDefaultsToBalancedAndIsOptionalForOldJson(string id)
    {
        ArtworkParameterSpec spec = Catalog.Get(id).Parameters.Single(p => p.Id == "render-profile");
        Assert.True(spec.OptionalInJson);
        Assert.Equal("balanced", StudioRecipeDefaults.RecipeFor(id).Parameters[spec.Id]);
    }

    [Fact]
    public void PortraitAndKeepComplexityPreserveTheCurrentComposition()
    {
        ArtworkRecipe original = ArtworkRecipe.Create(Catalog.Get("plasma-noise"), new CanvasSize(300, 500),
            ulong.MaxValue, new Dictionary<string, string> { ["octaves"] = "2", ["gradient-color-1"] = "#80402010" });
        ArtworkRecipe prepared = ProductionRenderProfile.PrepareFourK(original, Catalog, maximumComplexity: false);
        Assert.Equal(new CanvasSize(2160, 3840), prepared.Size);
        Assert.Equal(original.Seed, prepared.Seed);
        foreach (var pair in original.Parameters.Where(p => p.Key is not ("render-profile" or "render-quality")))
            Assert.Equal(pair.Value, prepared.Parameters[pair.Key]);
    }
}
