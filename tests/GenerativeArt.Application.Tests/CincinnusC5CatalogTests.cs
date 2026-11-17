using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class CincinnusC5CatalogTests
{
    private static readonly ArtworkRegistry Registry = BuiltInArtworkCatalog.Create();

    [Fact]
    public void CincinnusIsASeparateStaticRasterArtworkWithEmbeddedPreset()
    {
        Assert.Equal(30, Registry.All.Count);
        ArtworkDescriptor descriptor = Registry.Get("cincinnus-bristles");
        Assert.Equal(1, descriptor.AlgorithmVersion);
        Assert.False(descriptor.SupportsAnimation);
        Assert.Contains(descriptor.Parameters, p => p.Id == "mode");
        Assert.Equal(Enumerable.Range(1, 8).Select(i => $"pigment-{i}"),
            descriptor.Parameters.Where(p => p.Kind == ParameterKind.Color &&
                p.Id.StartsWith("pigment-", StringComparison.Ordinal)).Select(p => p.Id));
        ArtworkRecipe initial = StudioRecipeDefaults.RecipeFor(descriptor.Id);
        Assert.Equal("simple", initial.Parameters["mode"]);
        Assert.Equal(1920, initial.Size.Width);
        Assert.Equal(1080, initial.Size.Height);
        Assert.Equal(descriptor.Parameters.Count, initial.Parameters.Count);
        initial.ValidateAgainst(Registry);
    }

    [Fact]
    public void NewC5ExamplesAreStrictValidAndRoundTrip()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "CincinnusRecipes");
        string[] paths = Directory.GetFiles(folder, "*.json");
        Assert.Equal(3, paths.Length);
        var modes = new HashSet<string>();
        foreach (string path in paths)
        {
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), Registry);
            Assert.Equal("cincinnus-bristles", recipe.ArtworkId);
            Assert.Equal(768, recipe.Size.Width);
            Assert.Equal(768, recipe.Size.Height);
            modes.Add(recipe.Parameters["mode"]);
            ArtworkRecipe copy = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Registry), Registry);
            Assert.Equal(recipe.Parameters.OrderBy(p => p.Key), copy.Parameters.OrderBy(p => p.Key));
        }
        Assert.Equal(new[] { "adaptive", "simple" }, modes.OrderBy(v => v));
    }

    [Fact]
    public void StudioHidesOnlyModeSpecificControlsWithoutDeletingValues()
    {
        ArtworkDescriptor descriptor = Registry.Get("cincinnus-bristles");
        var simple = descriptor.ValidateParameters();
        Assert.NotNull(StudioParameterApplicability.InactiveReason(descriptor.Id, "main-strokes", simple));
        Assert.Null(StudioParameterApplicability.InactiveReason(descriptor.Id, "separation", simple));
        var adaptive = descriptor.ValidateParameters(new Dictionary<string, string> { ["mode"] = "adaptive" });
        Assert.NotNull(StudioParameterApplicability.InactiveReason(descriptor.Id, "separation", adaptive));
        Assert.Null(StudioParameterApplicability.InactiveReason(descriptor.Id, "main-strokes", adaptive));
        Assert.Equal(simple.Count, adaptive.Count);
    }

    [Fact]
    public void IndividualPigmentAndQualityModeAreTypedAndValidated()
    {
        ArtworkDescriptor descriptor = Registry.Get("cincinnus-bristles");
        var values = descriptor.ValidateParameters(new Dictionary<string, string>
        {
            ["pigment-4"] = "#FFAABBCC", ["background-color"] = "#102030",
            ["require-complete"] = "true", ["mode"] = "adaptive"
        });
        Assert.Equal("#FFAABBCC", values["pigment-4"]);
        Assert.Equal("#102030", values["background-color"]);
        Assert.Equal("true", values["require-complete"]);
        Assert.Throws<ArgumentException>(() => descriptor.ValidateParameters(
            new Dictionary<string, string> { ["palette"] = "invalid" }));
        Assert.Throws<ArgumentException>(() => descriptor.ValidateParameters(
            new Dictionary<string, string> { ["placement-attempts"] = "1000000" }));
    }
}
