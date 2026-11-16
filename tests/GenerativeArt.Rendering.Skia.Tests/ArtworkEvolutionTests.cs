using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ArtworkEvolutionTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);
    private static ArtworkRecipe Recipe(string id, Dictionary<string, string>? values = null, ulong seed = 42)
    {
        var p = new Dictionary<string, string>(StudioRecipeDefaults.ParametersFor(id));
        switch (id)
        {
            case "flow-brush": p["strokes"] = "5"; p["bristles"] = "4"; p["steps"] = "48"; break;
            case "strata-landscape": p["layers"] = "7"; p["samples"] = "192"; break;
            case "parametric-curves": p["layers"] = "6"; p["samples"] = "1200"; break;
            case "noise-flow": p["lines"] = "24"; p["steps"] = "80"; break;
            case "spaced-streamlines": p["max-lines"] = "30"; p["max-steps"] = "60"; break;
            case "looping-flow": p["lines"] = "16"; p["steps"] = "60"; break;
        }
        if (values is not null) foreach (var pair in values) p[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(160, 120), seed, p,
            id == "looping-flow" ? new RecipeAnimation(4, 18) : null);
    }
    private static byte[] Render(ArtworkRecipe recipe, int frame = 0)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output, recipe.Animation is null ? null : frame);
        return output.ToArray();
    }

    [Theory]
    [InlineData("flow-brush")]
    [InlineData("strata-landscape")]
    [InlineData("parametric-curves")]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("plasma-noise")]
    [InlineData("looping-flow")]
    public void EvolvedArtworksAreSeededAndRoundTripJson(string id)
    {
        ArtworkRecipe recipe = Recipe(id);
        byte[] png = Render(recipe);
        Assert.Equal(png, Render(recipe));
        Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(id, seed: 43))));
        Assert.Equal(png, Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Catalog), Catalog)));
        using SKBitmap image = SKBitmap.Decode(png) ?? throw new InvalidOperationException();
        Assert.Equal(160, image.Width); Assert.Equal(120, image.Height);
        Assert.True(image.Pixels.Distinct().Count() > 30);
    }

    [Theory]
    [InlineData("parametric-curves", "frequency-x", "11")]
    [InlineData("parametric-curves", "frequency-y", "8")]
    [InlineData("parametric-curves", "phase", "-1")]
    [InlineData("parametric-curves", "style", "harmonic")]
    [InlineData("parametric-curves", "layers", "12")]
    [InlineData("parametric-curves", "twist", "-60")]
    [InlineData("parametric-curves", "rotation", "30")]
    [InlineData("parametric-curves", "layer-scale", "0.2")]
    [InlineData("parametric-curves", "stretch-x", "1.5")]
    [InlineData("parametric-curves", "stretch-y", "0.6")]
    [InlineData("parametric-curves", "samples", "6000")]
    [InlineData("parametric-curves", "line-width", "2")]
    [InlineData("parametric-curves", "orbit-width", "1")]
    [InlineData("noise-flow", "field", "braid")]
    [InlineData("noise-flow", "field", "vortex")]
    [InlineData("noise-flow", "frequency", "0.012")]
    [InlineData("noise-flow", "start-layout", "edge")]
    [InlineData("noise-flow", "steps", "180")]
    [InlineData("noise-flow", "step-size", "4")]
    [InlineData("noise-flow", "line-width", "2")]
    [InlineData("noise-flow", "opacity", "0.8")]
    [InlineData("spaced-streamlines", "field", "vortex")]
    [InlineData("spaced-streamlines", "frequency", "0.012")]
    [InlineData("spaced-streamlines", "max-steps", "140")]
    [InlineData("spaced-streamlines", "integration-step", "0.5")]
    [InlineData("spaced-streamlines", "opacity", "0.3")]
    [InlineData("plasma-noise", "pattern", "ridged")]
    [InlineData("plasma-noise", "pattern", "turbulence")]
    [InlineData("plasma-noise", "lacunarity", "2.7")]
    [InlineData("plasma-noise", "persistence", "0.8")]
    [InlineData("plasma-noise", "bands", "12")]
    [InlineData("plasma-noise", "rotation", "40")]
    [InlineData("plasma-noise", "stretch-x", "2")]
    [InlineData("plasma-noise", "stretch-y", "0.5")]
    [InlineData("plasma-noise", "contrast", "2")]
    [InlineData("plasma-noise", "gamma", "2")]
    [InlineData("plasma-noise", "contour-strength", "0.8")]
    [InlineData("looping-flow", "lines", "32")]
    [InlineData("looping-flow", "steps", "150")]
    [InlineData("looping-flow", "frequency", "0.012")]
    [InlineData("looping-flow", "step-size", "4")]
    [InlineData("looping-flow", "line-width", "2")]
    [InlineData("looping-flow", "opacity", "0.2")]
    [InlineData("flow-brush", "field", "vortex")]
    [InlineData("flow-brush", "frequency", "0.012")]
    [InlineData("flow-brush", "curvature", "5")]
    [InlineData("flow-brush", "strokes", "10")]
    [InlineData("flow-brush", "start-layout", "random")]
    [InlineData("flow-brush", "steps", "90")]
    [InlineData("flow-brush", "step-size", "4")]
    [InlineData("flow-brush", "bristles", "10")]
    [InlineData("flow-brush", "brush-width", "60")]
    [InlineData("flow-brush", "bristle-width", "2")]
    [InlineData("flow-brush", "taper", "0")]
    [InlineData("flow-brush", "roughness", "0.9")]
    [InlineData("flow-brush", "opacity", "0.9")]
    [InlineData("flow-brush", "shadow", "0")]
    [InlineData("flow-brush", "palette", "ember")]
    [InlineData("strata-landscape", "layers", "14")]
    [InlineData("strata-landscape", "horizon", "0.4")]
    [InlineData("strata-landscape", "height", "0.3")]
    [InlineData("strata-landscape", "frequency", "5")]
    [InlineData("strata-landscape", "layer-drift", "1")]
    [InlineData("strata-landscape", "tilt", "0.3")]
    [InlineData("strata-landscape", "profile", "ridge")]
    [InlineData("strata-landscape", "profile", "waves")]
    [InlineData("strata-landscape", "octaves", "1")]
    [InlineData("strata-landscape", "persistence", "0.8")]
    [InlineData("strata-landscape", "warp", "2")]
    [InlineData("strata-landscape", "samples", "1024")]
    [InlineData("strata-landscape", "atmosphere", "0")]
    [InlineData("strata-landscape", "shadow", "0.4")]
    [InlineData("strata-landscape", "shadow-width", "15")]
    [InlineData("strata-landscape", "edge-width", "3")]
    [InlineData("strata-landscape", "sun-radius", "0")]
    [InlineData("strata-landscape", "sun-x", "0.2")]
    [InlineData("strata-landscape", "sun-y", "0.3")]
    [InlineData("strata-landscape", "palette", "desert")]
    public void EveryArtisticControlChangesTheRaster(string id, string key, string value) =>
        Assert.False(Render(Recipe(id)).AsSpan().SequenceEqual(Render(Recipe(id, new() { [key] = value }))));

    [Fact]
    public void AngularCurvatureAndLoopPhasesAreEffective()
    {
        Assert.False(Render(Recipe("noise-flow", new() { ["field"] = "legacy" })).AsSpan()
            .SequenceEqual(Render(Recipe("noise-flow", new() { ["field"] = "legacy", ["curvature"] = "7" }))));
        ArtworkRecipe loop = Recipe("looping-flow");
        Assert.False(Render(loop, 0).AsSpan().SequenceEqual(Render(loop, 1)));
        Assert.Equal(Render(loop, 0), Render(loop, 0));
    }

    [Theory]
    [InlineData("parametric-curves")]
    [InlineData("noise-flow")]
    [InlineData("spaced-streamlines")]
    [InlineData("plasma-noise")]
    [InlineData("looping-flow")]
    public void OldRecipesWithoutTheOptionalExtensionKeepNeutralDefaults(string id)
    {
        var animation = id == "looping-flow" ? new RecipeAnimation(4, 18) : null;
        ArtworkRecipe original = ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(160, 120), 42, animation: animation);
        var root = System.Text.Json.Nodes.JsonNode.Parse(ArtworkRecipeJson.Serialize(original, Catalog))!;
        foreach (var spec in LegacyArtworkControls.ForArtwork(id)) root["parameters"]!.AsObject().Remove(spec.Id);
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(root.ToJsonString(), Catalog);
        foreach (var spec in LegacyArtworkControls.ForArtwork(id)) Assert.Equal(spec.DefaultValue, loaded.Parameters[spec.Id]);
        Assert.Equal(Render(original), Render(loaded));
        Assert.False(Render(original).AsSpan().SequenceEqual(Render(Recipe(id))));
    }

    [Theory]
    [InlineData("flow-brush")]
    [InlineData("strata-landscape")]
    public void TransparentColorOverridesAndMaterialKeepAlpha(string id)
    {
        var p = new Dictionary<string, string> { ["background-color"] = "#00000000", ["render-quality"] = "fine-2x", ["shadow"] = "0" };
        foreach (var spec in Catalog.Get(id).Parameters.Where(p => p.AllowAutoColor && p.Id != "background-color"))
            p[spec.Id] = "#604878A0";
        if (id == "strata-landscape") p["sun-radius"] = "0";
        using SKBitmap before = SKBitmap.Decode(Render(Recipe(id, p))) ?? throw new InvalidOperationException();
        p["paper-texture"] = "cotton"; p["paper-strength"] = "0.2";
        using SKBitmap after = SKBitmap.Decode(Render(Recipe(id, p))) ?? throw new InvalidOperationException();
        Assert.Contains(before.Pixels, p => p.Alpha == 0);
        Assert.Contains(before.Pixels, p => p.Alpha is > 0 and < 255);
        Assert.Equal(before.Pixels.Select(p => p.Alpha), after.Pixels.Select(p => p.Alpha));
    }

    [Theory]
    [InlineData("noise-flow")]
    [InlineData("looping-flow")]
    public void OpacityAloneDoesNotMoveTheHistoricalFlowPaths(string id)
    {
        var values = new Dictionary<string,string> { ["background-color"] = "#FFFFFF" };
        for (int i=1;i<=(id=="noise-flow"?3:4);i++) values[$"ink-color-{i}"]="#000000";
        var animation=id=="looping-flow"?new RecipeAnimation(4,18):null;
        var original=ArtworkRecipe.Create(Catalog.Get(id),new CanvasSize(160,120),42,values,animation);
        values["opacity"]="0.35";
        var faded=ArtworkRecipe.Create(Catalog.Get(id),original.Size,42,values,animation);
        using SKBitmap a=SKBitmap.Decode(Render(original)) ?? throw new InvalidOperationException();
        using SKBitmap b=SKBitmap.Decode(Render(faded)) ?? throw new InvalidOperationException();
        for(int y=0;y<a.Height;y++) for(int x=0;x<a.Width;x++)
            if(a.GetPixel(x,y)==SKColors.White) Assert.Equal(SKColors.White,b.GetPixel(x,y));
    }

    [Fact]
    public void WorkBudgetsFailBeforeAnyOutputAndCancellationIsHonored()
    {
        var invalid = new[]
        {
            Recipe("flow-brush", new() { ["strokes"] = "600", ["bristles"] = "40", ["steps"] = "800" }),
            Recipe("strata-landscape", new() { ["layers"] = "120", ["samples"] = "4096" }),
            Recipe("noise-flow", new() { ["lines"] = "10000", ["steps"] = "1000" }),
            Recipe("spaced-streamlines", new() { ["max-lines"] = "10000", ["max-steps"] = "800" }),
            ArtworkRecipe.Create(Catalog.Get("looping-flow"), new CanvasSize(64,64), 42,
                new Dictionary<string,string> { ["lines"] = "2000", ["steps"] = "800" }, new RecipeAnimation(120,18))
        };
        foreach (var recipe in invalid)
        {
            using var output = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, output, recipe.Animation is null ? null : 0));
            Assert.Equal(0, output.Length);
        }
        foreach (string id in new[] { "flow-brush", "strata-landscape", "noise-flow", "parametric-curves" })
        {
            using var cancel = new CancellationTokenSource(); cancel.Cancel();
            using var output = new MemoryStream();
            Assert.Throws<OperationCanceledException>(() => Engine.Render(Recipe(id), output, cancellationToken: cancel.Token));
            Assert.Equal(0, output.Length);
        }
    }
}
