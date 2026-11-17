using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioBundledDefaultsTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Fact]
    public void EachBuiltInGeneratorHasAnEmbeddedAndValidInitialRecipe()
    {
        Assert.Equal(30, Catalog.All.Count);
        foreach (ArtworkDescriptor descriptor in Catalog.All)
        {
            ArtworkRecipe recipe = StudioRecipeDefaults.RecipeFor(descriptor.Id);
            Assert.Equal(descriptor.Id, recipe.ArtworkId);
            Assert.Equal(descriptor.AlgorithmVersion, recipe.AlgorithmVersion);
            Assert.Equal(1920, recipe.Size.Width);
            Assert.Equal(1080, recipe.Size.Height);
            Assert.Equal(descriptor.Parameters.Count, recipe.Parameters.Count);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key).ToArray(),
                StudioRecipeDefaults.ParametersFor(descriptor.Id).OrderBy(p => p.Key).ToArray());
            recipe.ValidateAgainst(Catalog);
            ArtworkRecipe roundTrip = ArtworkRecipeJson.Deserialize(
                ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog);
            Assert.Equal(recipe.Seed, roundTrip.Seed);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key).ToArray(),
                roundTrip.Parameters.OrderBy(p => p.Key).ToArray());
            Assert.Equal(recipe.Animation?.FrameCount, roundTrip.Animation?.FrameCount);
        }
    }

    [Fact]
    public void IndividualDefaultsHonorTheUserSuppliedSeedColorsAndAnimation()
    {
        Assert.Equal(6_582_940_917_790_807_322UL, StudioRecipeDefaults.RecipeFor("domain-warp").Seed);
        Assert.Equal("12.394594594594574", StudioRecipeDefaults.ParametersFor("domain-warp")["bands"]);
        Assert.Equal("#9E68258D", StudioRecipeDefaults.ParametersFor("noise-flow")["ink-color-1"]);
        Assert.Equal("#B07DCACB", StudioRecipeDefaults.ParametersFor("parametric-curves")["curve-color"]);
        Assert.Equal("fine-2x", StudioRecipeDefaults.ParametersFor("voronoi-mosaic")["render-quality"]);
        Assert.Equal("pigment", StudioRecipeDefaults.ParametersFor("sdf-orbits")["color-mode"]);
        Assert.Equal("4028", StudioRecipeDefaults.ParametersFor("spaced-streamlines")["max-lines"]);
        RecipeAnimation? animation = StudioRecipeDefaults.RecipeFor("looping-flow").Animation;
        Assert.NotNull(animation);
        Assert.Equal(16, animation.FrameCount);
        Assert.Equal(18, animation.FramesPerSecond);
    }

    [Fact]
    public void NewStudioDefaultsDoNotChangeCatalogDefaultsOrPreviousJson()
    {
        var descriptor = Catalog.Get("sdf-orbits");
        Assert.Equal("classic", descriptor.ValidateParameters()["style"]);
        Assert.Equal("pigment", StudioRecipeDefaults.RecipeFor("sdf-orbits").Parameters["style"]);
        Assert.Equal("classic", ArtworkRecipe.Create(descriptor,
            new GenerativeArt.Core.Geometry.CanvasSize(256, 256), 42).Parameters["style"]);
    }
}
