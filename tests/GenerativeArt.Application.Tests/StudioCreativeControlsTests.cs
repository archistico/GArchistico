using System.Text.Json;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioCreativeControlsTests
{
    [Theory]
    [InlineData("parametric-curves", "style", "woven")]
    [InlineData("noise-flow", "field", "curl")]
    [InlineData("plasma-noise", "pattern", "marble")]
    [InlineData("spaced-streamlines", "max-lines", "3000")]
    [InlineData("looping-flow", "lines", "220")]
    [InlineData("strata-landscape", "depth-blur", "6")]
    [InlineData("sdf-orbits", "style", "pigment")]
    public void FreshStudioWorksUseArtisticDefaultsWithoutChangingCatalogDefaults(string id, string key, string value)
    {
        var descriptor = _catalog.Get(id);
        var fresh = descriptor.ValidateParameters(StudioRecipeDefaults.ParametersFor(id));
        Assert.Equal(value, fresh[key]);
        Assert.NotEqual(fresh[key], descriptor.ValidateParameters()[key]);
        Assert.All(LegacyArtworkControls.ForArtwork(id), p => Assert.True(p.OptionalInJson));
    }

    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();

    [Fact]
    public void DefaultCanvasIsFullHdWhilePreviewIsLightweight()
    {
        Assert.Equal(1920, StudioRecipeDefaults.Width);
        Assert.Equal(1080, StudioRecipeDefaults.Height);
        var source = ArtworkRecipe.Create(_catalog.Get("topographic-contours"),
            new CanvasSize(StudioRecipeDefaults.Width, StudioRecipeDefaults.Height), 42);
        ArtworkRecipe preview = StudioPreviewPolicy.CreatePreview(source, _catalog);
        Assert.Equal(800, preview.Size.Width);
        Assert.Equal(450, preview.Size.Height);
        Assert.Equal(1920, source.Size.Width);
    }

    [Fact]
    public void LegacyRecipeWithoutNewOptionalParametersStillOpensAndRoundTrips()
    {
        const string legacy = """
            {"formatVersion":1,"artworkId":"poisson-stippling","algorithmVersion":1,
             "seed":"42","width":800,"height":600,
             "parameters":{"spacing":0.025,"max-points":2500,"dot-size":1.7,"palette":"paper"}}
            """;
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(legacy, _catalog);
        Assert.Equal("auto", loaded.Parameters["dot-color-1"]);
        Assert.Equal("solid", loaded.Parameters["dot-style"]);
        Assert.Equal("standard", loaded.Parameters["render-quality"]);
        Assert.Equal("0.36", loaded.Parameters["radius-variation"]);
        var changed = ArtworkRecipe.Create(_catalog.Get("poisson-stippling"), loaded.Size,
            loaded.Seed, new Dictionary<string, string> { ["dot-color-1"] = "#99aa22" });
        Assert.Equal("#99AA22", changed.Parameters["dot-color-1"]);
        Assert.Equal("#99AA22", ArtworkRecipeJson.Deserialize(
            ArtworkRecipeJson.Serialize(changed, _catalog), _catalog).Parameters["dot-color-1"]);
        string missingRequired = legacy.Replace("\"spacing\":0.025,", "", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(missingRequired, _catalog));
        string unknown = legacy.Replace("\"spacing\":0.025", "\"unknown\":0.025", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(unknown, _catalog));
    }

    [Fact]
    public void RoleColorsValidateAutoAndHexWhileRangesAreExpanded()
    {
        ArtworkParameterSpec color = _catalog.Get("topographic-contours").Parameters
            .Single(p => p.Id == "major-line-color");
        Assert.True(color.OptionalInJson);
        Assert.Equal("auto", color.ValidateValue("AUTO"));
        Assert.Equal("#FFAABBCC", color.ValidateValue("#ffaabbcc"));
        Assert.Throws<ArgumentException>(() => color.ValidateValue("red"));
        Assert.Throws<ArgumentException>(() => color.ValidateValue("#G0G0G0"));
        var poisson = _catalog.Get("poisson-stippling");
        Assert.Equal("0.001", poisson.Parameters.Single(p => p.Id == "spacing").ValidateValue("0.001"));
        Assert.Equal("128", poisson.Parameters.Single(p => p.Id == "dot-size").ValidateValue("128"));
        Assert.Throws<ArgumentException>(() => poisson.Parameters.Single(p => p.Id == "dot-size").ValidateValue("500"));
    }
}
