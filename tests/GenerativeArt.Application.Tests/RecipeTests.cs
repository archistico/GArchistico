using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class RecipeTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    private static ArtworkRecipe Static(ulong seed = 42) => ArtworkRecipe.Create(
        Catalog.Get("parametric-curves"), new CanvasSize(640, 480), seed,
        new Dictionary<string, string> { ["scale"] = "0.25" });

    private static ArtworkRecipe Animated(ulong seed = 42) => ArtworkRecipe.Create(
        Catalog.Get("looping-flow"), new CanvasSize(256, 256), seed,
        new Dictionary<string, string> { ["loop-radius"] = "2.1", ["palette"] = "ember" },
        new RecipeAnimation(24, 30));

    private static string Json(ArtworkRecipe recipe) =>
        Encoding.UTF8.GetString(ArtworkRecipeJson.Serialize(recipe, Catalog));

    [Fact]
    public void StaticRecipeRoundTripsWithTypedNumberAndCanonicalFields()
    {
        string json = Json(Static());
        Assert.Contains("\"scale\": 0.25", json);
        Assert.Contains("\"seed\": \"42\"", json);
        Assert.DoesNotContain("\"animation\"", json);
        ArtworkRecipe result = ArtworkRecipeJson.Deserialize(json, Catalog);
        Assert.Equal("parametric-curves", result.ArtworkId);
        Assert.Equal(640, result.Size.Width);
        Assert.Equal("0.25", result.Parameters["scale"]);
        Assert.Null(result.Animation);
        Assert.Equal(json, Json(result));
    }

    [Fact]
    public void AnimatedRecipeRoundTripsWithAllSettings()
    {
        string json = Json(Animated());
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Number, doc.RootElement.GetProperty("parameters").GetProperty("loop-radius").ValueKind);
        Assert.Equal(JsonValueKind.String, doc.RootElement.GetProperty("parameters").GetProperty("palette").ValueKind);
        ArtworkRecipe result = ArtworkRecipeJson.Deserialize(json, Catalog);
        Assert.Equal(24, result.Animation!.FrameCount);
        Assert.Equal(30, result.Animation.FramesPerSecond);
        Assert.Equal("ember", result.Parameters["palette"]);
        Assert.Equal("2.1", result.Parameters["loop-radius"]);
        Assert.Equal(json, Json(result));
    }

    [Fact]
    public void Uint64MaximumSeedSurvivesRoundTripExactly()
    {
        var recipe = Static(ulong.MaxValue);
        string json = Json(recipe);
        Assert.Contains("18446744073709551615", json);
        Assert.Equal(ulong.MaxValue, ArtworkRecipeJson.Deserialize(json, Catalog).Seed);
    }

    [Fact]
    public void CanonicalJsonIsStableAcrossCalls()
    {
        byte[] first = ArtworkRecipeJson.Serialize(Animated(), Catalog);
        byte[] second = ArtworkRecipeJson.Serialize(Animated(), Catalog);
        Assert.Equal(first, second);
    }

    [Fact]
    public void CreationFillsDefaultsAndNormalizesValues()
    {
        ArtworkRecipe r = ArtworkRecipe.Create(Catalog.Get("looping-flow"), new CanvasSize(120, 100), 2,
            new Dictionary<string, string> { ["loop-radius"] = "2.500", ["palette"] = "mist" });
        Assert.Equal("2.5", r.Parameters["loop-radius"]);
        Assert.Equal("mist", r.Parameters["palette"]);
        Assert.Equal("looping-flow", r.ArtworkId);
    }

    [Fact]
    public void CreatingAnimationForStaticArtworkIsRejected()
    {
        Assert.Throws<ArgumentException>(() => ArtworkRecipe.Create(
            Catalog.Get("parametric-curves"), new CanvasSize(100, 100), 1,
            animation: new RecipeAnimation(10, 24)));
    }

    [Theory]
    [InlineData(1, 24)]
    [InlineData(10001, 24)]
    [InlineData(24, 0)]
    [InlineData(24, 241)]
    public void BadAnimationBoundsRejected(int frames, int fps) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecipeAnimation(frames, fps));

    [Theory]
    [InlineData("\"formatVersion\": 1", "\"formatVersion\": 2")]
    [InlineData("\"algorithmVersion\": 1", "\"algorithmVersion\": 2")]
    [InlineData("\"artworkId\": \"parametric-curves\"", "\"artworkId\": \"unknown-art\"")]
    public void UnknownFormatOrArtworkVersionsRejected(string from, string to)
    {
        string tampered = Json(Static()).Replace(from, to, StringComparison.Ordinal);
        if (to.Contains("unknown-art", StringComparison.Ordinal))
            Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
        else
            Assert.Throws<NotSupportedException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Theory]
    [InlineData("\"scale\": 0.25", "\"scale\": \"0.25\"")]
    [InlineData("\"scale\": 0.25", "\"scale\": 2.5")]
    [InlineData("\"scale\": 0.25", "\"unknown\": 0.25")]
    [InlineData("\"scale\": 0.25", "\"scale\": true")]
    public void ParameterInvalidTypeOrValueRejected(string from, string to)
    {
        string tampered = Json(Static()).Replace(from, to, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void RootUnknownPropertyIsRejected()
    {
        string tampered = Json(Static()).Replace("\"formatVersion\": 1,",
            "\"extra\": 10, \"formatVersion\": 1,", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void DuplicateRootKeysAreRejected()
    {
        string tampered = Json(Static()).Replace("\"formatVersion\": 1,",
            "\"formatVersion\": 1, \"formatVersion\": 1,", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void DuplicateParameterKeysAreRejected()
    {
        string tampered = Json(Static()).Replace("\"scale\": 0.25",
            "\"scale\": 0.25, \"scale\": 0.25", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void MissingParametersAreRejected()
    {
        // La vecchia sostituzione testuale lasciava una virgola finale e
        // provocava JsonReaderException PRIMA della validazione dei parametri.
        // Rimuoviamo la proprietà dall'albero per ottenere JSON valido.
        JsonNode root = JsonNode.Parse(Json(Static()))!;
        Assert.True(root["parameters"]!.AsObject().Remove("scale"));
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(root.ToJsonString(), Catalog));
    }

    [Fact]
    public void StaticArtworkRejectsInjectedAnimation()
    {
        string tampered = Json(Static()).Replace("\"parameters\": {",
            "\"animation\": {\"frameCount\": 12, \"framesPerSecond\": 24}, \"parameters\": {",
            StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void WrongSeedTypeAndNonCanonicalSeedRejected()
    {
        string json = Json(Static());
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(
            json.Replace("\"seed\": \"42\"", "\"seed\": 42", StringComparison.Ordinal), Catalog));
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(
            json.Replace("\"seed\": \"42\"", "\"seed\": \"042\"", StringComparison.Ordinal), Catalog));
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(
            json.Replace("\"seed\": \"42\"", "\"seed\": \"18446744073709551616\"", StringComparison.Ordinal), Catalog));
    }

    [Fact]
    public void LargeJsonIsRejectedBeforeParsing()
    {
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(
            new byte[ArtworkRecipeJson.MaximumUtf8Bytes + 1], Catalog));
    }

    [Fact]
    public void TooLargeJsonStringIsRejectedBeforeUtf8Allocation()
    {
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(
            new string('x', ArtworkRecipeJson.MaximumUtf8Bytes + 1), Catalog));
    }

    [Fact]
    public void FilesRoundTripAndCannotOverwrite()
    {
        string folder = Path.Combine(Path.GetTempPath(), "generativeart-recipes-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "sample.json");
        try
        {
            ArtworkRecipeFiles.SaveNew(path, Animated(), Catalog);
            ArtworkRecipe loaded = ArtworkRecipeFiles.Load(path, Catalog);
            Assert.Equal("ember", loaded.Parameters["palette"]);
            Assert.Throws<IOException>(() => ArtworkRecipeFiles.SaveNew(path, Static(), Catalog));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void FileReaderRejectsOverLimitWithoutWholeFileAllocation()
    {
        string path = Path.Combine(Path.GetTempPath(), "generativeart-large-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllBytes(path, new byte[ArtworkRecipeJson.MaximumUtf8Bytes + 100]);
            Assert.Throws<InvalidDataException>(() => ArtworkRecipeFiles.Load(path, Catalog));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void RecipeParametersCannotBeChangedThroughCallerDictionary()
    {
        var source = new Dictionary<string, string> { ["scale"] = "0.28" };
        ArtworkRecipe recipe = ArtworkRecipe.Create(Catalog.Get("parametric-curves"),
            new CanvasSize(100, 100), 4, source);
        source["scale"] = "0.47";
        Assert.Equal("0.28", recipe.Parameters["scale"]);
    }
    [Fact]
    public void AllParameterKindsUseTheirNativeJsonTypes()
    {
        var descriptor = new ArtworkDescriptor("all-types", 1, "Types", "Test", "All kinds",
            ArtworkCapabilities.Raster,
            [
                ArtworkParameterSpec.Boolean("enabled", "Enabled", true),
                ArtworkParameterSpec.Integer("count", "Count", 3, 0, 10),
                ArtworkParameterSpec.Number("speed", "Speed", 0.25, 0, 2),
                ArtworkParameterSpec.Color("ink", "Ink", "#FFAABB"),
                ArtworkParameterSpec.Choice("style", "Style", "thin", ["thin", "bold"])
            ]);
        var registry = new ArtworkRegistry([descriptor]);
        var input = new Dictionary<string, string>
        {
            ["enabled"] = "false", ["count"] = "5", ["speed"] = "1.25",
            ["ink"] = "#aabbcc", ["style"] = "bold"
        };
        ArtworkRecipe recipe = ArtworkRecipe.Create(descriptor, new CanvasSize(80, 60), 1, input);
        byte[] bytes = ArtworkRecipeJson.Serialize(recipe, registry);
        using JsonDocument document = JsonDocument.Parse(bytes);
        JsonElement parameters = document.RootElement.GetProperty("parameters");
        Assert.Equal(JsonValueKind.False, parameters.GetProperty("enabled").ValueKind);
        Assert.Equal(5, parameters.GetProperty("count").GetInt32());
        Assert.Equal(1.25, parameters.GetProperty("speed").GetDouble());
        Assert.Equal("#AABBCC", parameters.GetProperty("ink").GetString());
        Assert.Equal("bold", parameters.GetProperty("style").GetString());
        Assert.Equal("false", ArtworkRecipeJson.Deserialize(bytes, registry).Parameters["enabled"]);
    }

    [Fact]
    public void UnknownAnimationFieldIsRejected()
    {
        string tampered = Json(Animated()).Replace("\"frameCount\": 24,",
            "\"mystery\": 1, \"frameCount\": 24,", StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ArtworkRecipeJson.Deserialize(tampered, Catalog));
    }

    [Fact]
    public void TrailingCommaAndCommentsAreRejected()
    {
        string json = Json(Static());
        Assert.ThrowsAny<JsonException>(() => ArtworkRecipeJson.Deserialize(json + " // comment", Catalog));
        Assert.ThrowsAny<JsonException>(() => ArtworkRecipeJson.Deserialize(
            json.Replace("\"scale\": 0.25", "\"scale\": 0.25,", StringComparison.Ordinal), Catalog));
    }

}
