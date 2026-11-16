using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ArtisticColorRoleTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);

    [Fact]
    public void EveryColorRoleAndPresetHasAValidAutoPreview()
    {
        foreach (ArtworkDescriptor descriptor in Catalog.All)
        {
            var defaults = ArtworkRecipe.Create(descriptor, new CanvasSize(128, 96), 42,
                animation: descriptor.SupportsAnimation ? new RecipeAnimation(4, 18) : null);
            string[] presets = descriptor.Parameters.FirstOrDefault(p => p.Id == "palette")?.Choices.ToArray()
                ?? ["default"];
            foreach (string palette in presets)
                foreach (ArtworkParameterSpec parameter in descriptor.Parameters.Where(p => p.AllowAutoColor))
                {
                    Assert.Equal("auto", defaults.Parameters[parameter.Id]);
                    Assert.True(parameter.OptionalInJson);
                    string hex = ArtworkRolePalettePreview.SuggestHex(descriptor.Id, palette, parameter.Id);
                    Assert.Equal(hex, parameter.ValidateValue(hex));
                    Assert.True(hex.Length is 7 or 9);
                }
        }
    }

    [Theory]
    [InlineData("parametric-curves", "background-color")]
    [InlineData("noise-flow", "background-color")]
    [InlineData("spaced-streamlines", "background-color")]
    [InlineData("looping-flow", "background-color")]
    [InlineData("plasma-noise", "gradient-color-2")]
    public void HistoricGeneratorsKeepAutoButRespectIndependentOverrides(string id, string role)
    {
        ArtworkDescriptor descriptor = Catalog.Get(id);
        RecipeAnimation? animation = descriptor.SupportsAnimation ? new RecipeAnimation(4, 18) : null;
        var original = ArtworkRecipe.Create(descriptor, new CanvasSize(160, 120), 42, animation: animation);
        var overrideColor = ArtworkRecipe.Create(descriptor, original.Size, 42,
            new Dictionary<string, string> { [role] = "#FF42E8" }, animation);
        int? index = animation is null ? null : 0;
        Assert.False(Render(original, index).AsSpan().SequenceEqual(Render(overrideColor, index)));
        Assert.Equal(Render(original, index), Render(original, index));
        byte[] saved = ArtworkRecipeJson.Serialize(overrideColor, Catalog);
        ArtworkRecipe reloaded = ArtworkRecipeJson.Deserialize(saved, Catalog);
        Assert.Equal("#FF42E8", reloaded.Parameters[role]);
        Assert.Equal(Render(overrideColor, index), Render(reloaded, index));
        Assert.Equal("auto", reloaded.Parameters[descriptor.Parameters
            .First(p => p.AllowAutoColor && p.Id != role).Id]);
    }

    [Theory]
    [InlineData("parametric-curves", "scale", "background-color")]
    [InlineData("noise-flow", "lines", "ink-color-1")]
    [InlineData("spaced-streamlines", "max-lines", "ink-color-4")]
    [InlineData("looping-flow", "loop-radius", "ink-color-1")]
    [InlineData("plasma-noise", "frequency", "gradient-color-1")]
    public void LegacyJsonV1AddsOnlyAutoOverrides(string artworkId, string firstKey, string role)
    {
        ArtworkDescriptor descriptor = Catalog.Get(artworkId);
        var original = ArtworkRecipe.Create(descriptor, new CanvasSize(160, 120), 42,
            animation: descriptor.SupportsAnimation ? new RecipeAnimation(4, 18) : null);
        var supplied = original.Parameters.Where(p => !descriptor.Parameters.Single(d => d.Id == p.Key).OptionalInJson)
            .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
        var parameters = new Dictionary<string, object>();
        foreach (var pair in supplied)
        {
            ArtworkParameterSpec p = descriptor.Parameters.Single(d => d.Id == pair.Key);
            parameters.Add(pair.Key, p.Kind switch
            {
                ParameterKind.Number => double.Parse(pair.Value, System.Globalization.CultureInfo.InvariantCulture),
                ParameterKind.Integer => int.Parse(pair.Value, System.Globalization.CultureInfo.InvariantCulture),
                _ => pair.Value
            });
        }
        var root = new Dictionary<string, object>
        {
            ["formatVersion"] = 1, ["artworkId"] = artworkId, ["algorithmVersion"] = 1,
            ["seed"] = "42", ["width"] = 160, ["height"] = 120,
            ["parameters"] = parameters
        };
        if (original.Animation is not null)
            root["animation"] = new { frameCount = 4, framesPerSecond = 18 };
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(
            System.Text.Json.JsonSerializer.Serialize(root), Catalog);
        Assert.Equal("auto", restored.Parameters[role]);
        Assert.Equal(original.Parameters[firstKey], restored.Parameters[firstKey]);
        Assert.Equal(Render(original, original.Animation is null ? null : 0),
            Render(restored, original.Animation is null ? null : 0));
    }

    [Fact]
    public void PlasmaAlphaOverrideChangesEncodedPngAndSurvivesJson()
    {
        var translucent = ArtworkRecipe.Create(Catalog.Get("plasma-noise"), new CanvasSize(96, 96), 21,
            new Dictionary<string, string>
            {
                ["gradient-color-1"] = "#280000FF", ["gradient-color-2"] = "#280000FF",
                ["gradient-color-3"] = "#280000FF", ["gradient-color-4"] = "#280000FF"
            });
        byte[] result = Render(translucent, null);
        using SKBitmap bitmap = SKBitmap.Decode(result) ?? throw new InvalidOperationException();
        Assert.InRange(bitmap.GetPixel(42, 42).Alpha, (byte)1, (byte)80);
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(translucent, Catalog), Catalog);
        Assert.Equal("#280000FF", restored.Parameters["gradient-color-4"]);
        Assert.Equal(result, Render(restored, null));
    }

    private static byte[] Render(ArtworkRecipe recipe, int? index)
    {
        using var stream = new MemoryStream();
        Engine.Render(recipe, stream, index);
        return stream.ToArray();
    }
}
