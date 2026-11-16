using GenerativeArt.Application.Catalog;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class CatalogTests
{
    private static ArtworkDescriptor Demo(string id, params ArtworkParameterSpec[] parameters) =>
        new(id, 1, "Esempio", "Demo", "Descrizione", ArtworkCapabilities.Raster, parameters);

    [Fact]
    public void CatalogHasStableSortedIdsAndLoopCapability()
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        // Gli ID sono parte del contratto delle ricette: controllare l'elenco esatto,
        // non soltanto il numero dei generatori disponibili.
        string[] expectedIds =
        [
            "domain-warp",
            "flow-brush",
            "guilloche",
            "lic-flow",
            "looping-flow",
            "noise-flow",
            "parametric-curves",
            "plasma-noise",
            "poisson-stippling",
            "sdf-orbits",
            "spaced-streamlines",
            "strata-landscape",
            "tonal-engraving",
            "topographic-contours",
            "voronoi-mosaic"
        ];
        Assert.Equal(expectedIds, catalog.All.Select(d => d.Id).ToArray());
        Assert.True(catalog.Get("looping-flow").SupportsAnimation);
        Assert.True(catalog.Get("looping-flow").SupportsSeamlessLoop);
        Assert.False(catalog.Get("noise-flow").SupportsAnimation);
        Assert.False(catalog.Get("plasma-noise").SupportsAnimation);
        Assert.False(catalog.Get("topographic-contours").SupportsAnimation);
        Assert.False(catalog.Get("sdf-orbits").SupportsAnimation);
        Assert.False(catalog.Get("poisson-stippling").SupportsAnimation);
        Assert.False(catalog.Get("voronoi-mosaic").SupportsAnimation);
        Assert.Equal(ArtworkCapabilities.Raster, catalog.Get("sdf-orbits").Capabilities);
        Assert.Equal(ArtworkCapabilities.Raster, catalog.Get("poisson-stippling").Capabilities);
        Assert.Equal(ArtworkCapabilities.Raster, catalog.Get("voronoi-mosaic").Capabilities);
        Assert.Equal(ArtworkCapabilities.Raster, catalog.Get("topographic-contours").Capabilities);
        Assert.Equal(ArtworkCapabilities.Raster, catalog.Get("plasma-noise").Capabilities);
        string[] expectedPlasma = ["frequency", "octaves", .. LegacyArtworkControls.ForArtwork("plasma-noise").Select(p => p.Id), "palette",
            "gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4", "render-quality",
            "grain-amount", "grain-size", "dither-mode", "dither-levels",
            "paper-texture", "paper-strength", "pigment-granulation", "ink-absorption"];
        Assert.Equal(expectedPlasma,
            catalog.Get("plasma-noise").Parameters.Select(p => p.Id).ToArray());
    }

    [Fact]
    public void AllArtworksExposeOptionalFinishingControlsWithNeutralDefaults()
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        Assert.Equal(15, catalog.All.Count);
        foreach (ArtworkDescriptor art in catalog.All)
        {
            Assert.Equal(new[] { "grain-amount", "grain-size", "dither-mode", "dither-levels" },
                art.Parameters.Where(p => p.Group == "Finitura").Select(p => p.Id));
            Assert.All(art.Parameters.Where(p => p.Group == "Finitura"), p => Assert.True(p.OptionalInJson));
            Assert.Equal(new[] { "paper-texture", "paper-strength", "pigment-granulation", "ink-absorption" },
                art.Parameters.Where(p => p.Group == "Materia").Select(p => p.Id));
            Assert.All(art.Parameters.Where(p => p.Group == "Materia"), p => Assert.True(p.OptionalInJson));
            IReadOnlyDictionary<string, string> defaults = art.ValidateParameters();
            Assert.Equal("0", defaults["grain-amount"]);
            Assert.Equal("1", defaults["grain-size"]);
            Assert.Equal("none", defaults["dither-mode"]);
            Assert.Equal("8", defaults["dither-levels"]);
            Assert.Equal("none", defaults["paper-texture"]);
            Assert.Equal("0", defaults["paper-strength"]);
            Assert.Equal("0", defaults["pigment-granulation"]);
            Assert.Equal("0", defaults["ink-absorption"]);
            Assert.Throws<ArgumentException>(() => art.ValidateParameters(
                new Dictionary<string, string> { ["grain-amount"] = "0.4" }));
            Assert.Throws<ArgumentException>(() => art.ValidateParameters(
                new Dictionary<string, string> { ["dither-mode"] = "diffusion" }));
        }
    }

    [Fact]
    public void DuplicateArtworkIdRejected()
    {
        Assert.Throws<ArgumentException>(() => new ArtworkRegistry([Demo("test"), Demo("test")]));
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("../bad")]
    [InlineData("bad--name")]
    [InlineData("-bad")]
    [InlineData("bad-")]
    [InlineData("bad_name")]
    public void InvalidIdsRejected(string id)
    {
        Assert.Throws<ArgumentException>(() => Demo(id));
    }

    [Fact]
    public void DuplicateParameterIdRejected()
    {
        var p = ArtworkParameterSpec.Integer("count", "Count", 2, 1, 5);
        Assert.Throws<ArgumentException>(() => Demo("x", p, p));
    }

    [Fact]
    public void InvalidCapabilitiesRejected()
    {
        Assert.Throws<ArgumentException>(() => new ArtworkDescriptor("test", 1, "X", "X", "X",
            ArtworkCapabilities.Raster | ArtworkCapabilities.SeamlessLoop, []));
    }

    [Fact]
    public void ParameterValuesValidatedAndNormalized()
    {
        ArtworkDescriptor d = Demo("test",
            ArtworkParameterSpec.Integer("count", "Count", 2, 1, 9),
            ArtworkParameterSpec.Number("speed", "Speed", .5, 0, 1),
            ArtworkParameterSpec.Boolean("enabled", "Enabled", true),
            ArtworkParameterSpec.Color("ink", "Ink", "#ABCDEF"),
            ArtworkParameterSpec.Choice("style", "Style", "silk", ["silk", "paper"]));
        var values = d.ValidateParameters(new Dictionary<string, string>
        {
            ["count"] = "03", ["speed"] = "0.75", ["enabled"] = "FALSE", ["ink"] = "#a1b2c3"
        });
        Assert.Equal("3", values["count"]);
        Assert.Equal("0.75", values["speed"]);
        Assert.Equal("false", values["enabled"]);
        Assert.Equal("#A1B2C3", values["ink"]);
        Assert.Equal("silk", values["style"]);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1,2")]
    [InlineData("1.1")]
    public void IntegerParameterRejectsInvalidInput(string raw)
    {
        var param = ArtworkParameterSpec.Integer("count", "Count", 2, 1, 9);
        Assert.Throws<ArgumentException>(() => param.ValidateValue(raw));
    }

    [Fact]
    public void DecimalParameterRejectsNonFiniteAndOutOfRange()
    {
        var p = ArtworkParameterSpec.Number("weight", "Weight", .5, 0, 1);
        Assert.Throws<ArgumentException>(() => p.ValidateValue("NaN"));
        Assert.Throws<ArgumentException>(() => p.ValidateValue("Infinity"));
        Assert.Throws<ArgumentException>(() => p.ValidateValue("1.5"));
    }

    [Fact]
    public void UnknownParameterAndBadColorAreRejected()
    {
        var d = Demo("test", ArtworkParameterSpec.Color("ink", "Ink", "#CC8844"));
        Assert.Throws<ArgumentException>(() => d.ValidateParameters(new Dictionary<string, string> { ["unknown"] = "x" }));
        Assert.Throws<ArgumentException>(() => d.ValidateParameters(new Dictionary<string, string> { ["ink"] = "#GG00BB" }));
    }

    [Fact]
    public void ChoiceOptionsAreCopiedAndDuplicatesRejected()
    {
        var options = new List<string> { "alpha", "beta" };
        var param = ArtworkParameterSpec.Choice("mode", "Mode", "alpha", options);
        options[0] = "modified";
        Assert.Equal("alpha", param.Choices[0]);
        Assert.Throws<ArgumentException>(() => ArtworkParameterSpec.Choice("mode", "Mode", "a", ["a", "a"]));
    }
}
