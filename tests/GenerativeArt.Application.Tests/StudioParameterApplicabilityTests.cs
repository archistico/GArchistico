using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioParameterApplicabilityTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    [Theory]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("flow-brush")]
    [InlineData("lic-flow")]
    public void VortexDisablesOnlyFrequencyAndCurlReenablesIt(string artwork)
    {
        var parameters = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor(artwork).Parameters);
        parameters["field"] = "vortex";
        Assert.NotNull(StudioParameterApplicability.InactiveReason(artwork, "frequency", parameters));
        parameters["field"] = "curl";
        Assert.Null(StudioParameterApplicability.InactiveReason(artwork, "frequency", parameters));
    }

    [Theory]
    [InlineData("noise-flow", "curl", true)]
    [InlineData("noise-flow", "legacy", false)]
    [InlineData("noise-flow", "braid", false)]
    [InlineData("flow-brush", "curl", true)]
    [InlineData("flow-brush", "braid", false)]
    public void CurvatureDependsOnFieldType(string artwork, string field, bool disabled)
    {
        var parameters = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor(artwork).Parameters)
        { ["field"] = field };
        Assert.Equal(disabled, StudioParameterApplicability.InactiveReason(artwork, "curvature", parameters) is not null);
    }

    [Fact]
    public void ConditionalFinishingControlsFollowTheirEnablers()
    {
        var parameters = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor("parametric-curves").Parameters)
        {
            ["grain-amount"] = "0", ["dither-mode"] = "none", ["paper-texture"] = "none"
        };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("parametric-curves", "grain-size", parameters));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("parametric-curves", "dither-levels", parameters));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("parametric-curves", "paper-strength", parameters));
        parameters["grain-amount"] = "0e0";
        Assert.NotNull(StudioParameterApplicability.InactiveReason("parametric-curves", "grain-size", parameters));
        parameters["grain-amount"] = "0.01";
        parameters["dither-mode"] = "bayer4";
        parameters["paper-texture"] = "cotton";
        Assert.Null(StudioParameterApplicability.InactiveReason("parametric-curves", "grain-size", parameters));
        Assert.Null(StudioParameterApplicability.InactiveReason("parametric-curves", "dither-levels", parameters));
        Assert.Null(StudioParameterApplicability.InactiveReason("parametric-curves", "paper-strength", parameters));
    }

    [Fact]
    public void MaskControlsAreInapplicableOnlyWithoutMask()
    {
        var parameters = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor("tonal-engraving").Parameters)
        { ["mask"] = "none" };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("tonal-engraving", "mask-radius", parameters));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("tonal-engraving", "mask-feather", parameters));
        parameters["mask"] = "circle";
        Assert.Null(StudioParameterApplicability.InactiveReason("tonal-engraving", "mask-radius", parameters));
    }

    [Fact]
    public void InapplicableParameterValueIsStillSavedInRecipe()
    {
        var preset = StudioRecipeDefaults.RecipeFor("spaced-streamlines");
        var parameters = new Dictionary<string, string>(preset.Parameters)
        { ["field"] = "vortex", ["frequency"] = "0.012" };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("spaced-streamlines", "frequency", parameters));
        var parsed = StudioRecipeFormParser.Parse(Catalog.Get("spaced-streamlines"),
            preset.Seed.ToString(), preset.Size.Width.ToString(), preset.Size.Height.ToString(),
            parameters, Array.Empty<string>());
        Assert.Equal("0.012", parsed.Parameters["frequency"]);
    }

    [Fact]
    public void ApplicabilityDoesNotMutateTheRecipeOrDefaults()
    {
        foreach (ArtworkDescriptor descriptor in Catalog.All)
        {
            var recipe = StudioRecipeDefaults.RecipeFor(descriptor.Id);
            var before = recipe.Parameters.ToDictionary(x => x.Key, x => x.Value);
            foreach (var spec in descriptor.Parameters)
                _ = StudioParameterApplicability.InactiveReason(descriptor.Id, spec.Id, recipe.Parameters);
            Assert.Equal(before.Count, recipe.Parameters.Count);
            foreach (var (key, value) in before)
                Assert.Equal(value, recipe.Parameters[key]);
        }
    }

    [Fact]
    public void G4R2DependentControlsAreDisabledOnlyWhenTheirEffectsAreZero()
    {
        var eyes = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor("ocellus-eyespots").Parameters)
        {
            ["pore-strength"] = "0", ["rim-light"] = "0"
        };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("ocellus-eyespots", "pore-radius", eyes));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("ocellus-eyespots", "rim-width", eyes));
        eyes["pore-strength"] = "0.8";
        eyes["rim-light"] = "0.5";
        Assert.Null(StudioParameterApplicability.InactiveReason("ocellus-eyespots", "pore-radius", eyes));
        Assert.Null(StudioParameterApplicability.InactiveReason("ocellus-eyespots", "rim-width", eyes));

        var foam = new Dictionary<string, string>(StudioRecipeDefaults.RecipeFor("rete-foam").Parameters)
        {
            ["junction-strength"] = "0", ["node-glow"] = "0"
        };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("rete-foam", "junction-radius", foam));
        foam["node-glow"] = "0.6";
        Assert.Null(StudioParameterApplicability.InactiveReason("rete-foam", "junction-radius", foam));
    }
    [Fact]
    public void CrinisMicroAndBodyControlsFollowTheirActiveModes()
    {
        var values = new Dictionary<string, string>
        {
            ["detail-style"] = "classic", ["body-opacity"] = "0"
        };
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "shadow-strength", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "fiber-density", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "fiber-coverage", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "pigment-contrast", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "depth-lighting", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "rear-softness", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-compression", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-volume", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-lighting", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "intra-bundle-depth", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "featured-fibers", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "cross-section-light", values));
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "internal-occlusion", values));
        values["detail-style"] = "micro";
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "fiber-density", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "fiber-coverage", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "pigment-contrast", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "depth-lighting", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "rear-softness", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-compression", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-volume", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "bundle-lighting", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "intra-bundle-depth", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "featured-fibers", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "cross-section-light", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "internal-occlusion", values));
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "fiber-shadow-depth", values));
        values["intra-bundle-depth"] = "0";
        Assert.NotNull(StudioParameterApplicability.InactiveReason("crinis-bristles", "internal-occlusion", values));
        values["intra-bundle-depth"] = "0.8";
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "internal-occlusion", values));
        values["body-opacity"] = "0.2";
        Assert.Null(StudioParameterApplicability.InactiveReason("crinis-bristles", "shadow-strength", values));
    }

}
