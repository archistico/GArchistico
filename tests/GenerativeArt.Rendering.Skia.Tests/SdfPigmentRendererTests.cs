using System.Text.Json.Nodes;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class SdfPigmentRendererTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);
    private static ArtworkRecipe Recipe(Dictionary<string,string>? values = null, ulong seed = 42, int width = 160, int height = 120)
    {
        var p = new Dictionary<string,string>(StudioRecipeDefaults.ParametersFor("sdf-orbits"))
        { ["blobs"] = "7", ["layers"] = "3", ["blob-radius"] = "0.17" };
        if (values is not null) foreach (var pair in values) p[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get("sdf-orbits"), new CanvasSize(width, height), seed, p);
    }
    private static byte[] Render(ArtworkRecipe r)
    { using var output = new MemoryStream(); Engine.Render(r,output); return output.ToArray(); }

    [Theory]
    [InlineData("pigment", "standard")]
    [InlineData("pigment", "fine-2x")]
    [InlineData("glass", "standard")]
    [InlineData("glass", "fine-2x")]
    public void OrganicSdfIsSeededDetailedAndSurvivesJson(string style, string quality)
    {
        var values = new Dictionary<string,string> { ["style"] = style, ["render-quality"] = quality };
        var r = Recipe(values); var png = Render(r);
        Assert.Equal(png, Render(r));
        Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(values, seed: 43))));
        Assert.Equal(png, Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(r,Catalog),Catalog)));
        using var image = SKBitmap.Decode(png);
        Assert.Equal(160,image.Width); Assert.Equal(120,image.Height);
        Assert.True(image.Pixels.Distinct().Count() > 1000);
    }

    [Theory]
    [InlineData("style", "glass")]
    [InlineData("layout", "orbital")]
    [InlineData("layout", "river")]
    [InlineData("blobs", "14")]
    [InlineData("layers", "1")]
    [InlineData("blob-radius", "0.28")]
    [InlineData("radius-variation", "0")]
    [InlineData("elongation", "3")]
    [InlineData("spread", "0.5")]
    [InlineData("warp-amount", "0.16")]
    [InlineData("warp-frequency", "8")]
    [InlineData("color-mix", "0")]
    [InlineData("color-mode", "linear")]
    [InlineData("marbling", "0")]
    [InlineData("marble-frequency", "24")]
    [InlineData("opacity", "0.4")]
    [InlineData("contour-opacity", "0.7")]
    [InlineData("outline", "0.8")]
    [InlineData("relief", "0")]
    [InlineData("highlight", "0")]
    [InlineData("light-angle", "120")]
    [InlineData("shadow-opacity", "0")]
    [InlineData("shadow-softness", "20")]
    [InlineData("shadow-distance", "25")]
    [InlineData("depth-blur", "18")]
    [InlineData("rotation", "60")]
    [InlineData("stretch-x", "1.8")]
    [InlineData("stretch-y", "0.5")]
    [InlineData("center-x", "0.3")]
    [InlineData("center-y", "0.7")]
    [InlineData("smoothness", "0.18")]
    [InlineData("shape-scale", "0.6")]
    [InlineData("bands", "24")]
    [InlineData("edge-width", "0.02")]
    [InlineData("palette", "lagoon")]
    [InlineData("pigment-color-4", "#FF1828")]
    [InlineData("shadow-color", "#FF1828")]
    [InlineData("fill-color", "#FF1828")]
    [InlineData("ink-color", "#FF1828")]
    [InlineData("accent-color", "#FF1828")]
    public void EveryControlAffectsTheNewRaster(string key, string value) =>
        Assert.False(Render(Recipe()).AsSpan().SequenceEqual(Render(Recipe(new() { [key] = value }))));

    [Fact]
    public void LegacyJsonKeepsClassicStyleAndHistoricRendering()
    {
        var original = ArtworkRecipe.Create(Catalog.Get("sdf-orbits"),new CanvasSize(160,120),42);
        var json = JsonNode.Parse(ArtworkRecipeJson.Serialize(original,Catalog))!;
        foreach (var p in SdfPigmentControls.All)
        { Assert.True(p.OptionalInJson); json["parameters"]!.AsObject().Remove(p.Id); }
        var loaded = ArtworkRecipeJson.Deserialize(json.ToJsonString(),Catalog);
        Assert.Equal("classic",loaded.Parameters["style"]);
        Assert.Equal(Render(original),Render(loaded));
        Assert.Equal("pigment",Catalog.Get("sdf-orbits").ValidateParameters(StudioRecipeDefaults.ParametersFor("sdf-orbits"))["style"]);
    }

    private static Dictionary<string,string> EdgeOnly() => new()
    {
        ["background-color"]="#00000000", ["fill-color"]="#F0783C", ["ink-color"]="#F0783C",
        ["accent-color"]="#F0783C", ["pigment-color-4"]="#F0783C", ["layers"]="1", ["blobs"]="4",
        ["marbling"]="0", ["relief"]="0", ["highlight"]="0", ["shadow-opacity"]="0",
        ["depth-blur"]="0", ["outline"]="0", ["contour-opacity"]="0", ["opacity"]="1", ["warp-amount"]="0"
    };

    [Theory]
    [InlineData("0")]
    [InlineData("0.09")]
    public void AnalyticEdgesAreCloserToFourfoldReferenceThanBinaryThresholds(string warp)
    {
        var values=EdgeOnly(); values["warp-amount"]=warp;
        using var image = SKBitmap.Decode(Render(Recipe(values,width:128,height:96)));
        using var reference = SKBitmap.Decode(Render(Recipe(values,width:512,height:384)));
        double analyticError = 0, binaryError = 0; int edges = 0;
        for (int y=0;y<96;y++) for(int x=0;x<128;x++)
        {
            double alpha=0;
            for(int dy=0;dy<4;dy++) for(int dx=0;dx<4;dx++) alpha+=reference.GetPixel(x*4+dx,y*4+dy).Alpha/16d;
            if(alpha is <=0 or >=255)continue;
            byte actual=image.GetPixel(x,y).Alpha;
            analyticError+=Math.Abs(actual-alpha);binaryError+=Math.Abs((actual>=128?255:0)-alpha);edges++;
        }
        Assert.True(edges>50);
        Assert.True(analyticError<binaryError*.75,$"AA {analyticError} vs binary {binaryError}");
        var partial=image.Pixels.Where(c=>c.Alpha is >10 and <245).ToArray();
        Assert.True(partial.Length>50);
        Assert.All(partial,c=>{Assert.InRange(c.Red,(byte)230,(byte)250);Assert.InRange(c.Green,(byte)110,(byte)130);});
    }

    [Fact]
    public void TransparentPigmentDoesNotContributeHiddenRgb()
    {
        var values=EdgeOnly(); values["pigment-color-4"]="#00FF0000";
        byte[] a=Render(Recipe(values));values["pigment-color-4"]="#0000FF00";
        Assert.Equal(a,Render(Recipe(values)));
    }

    [Fact]
    public void BudgetsAndCancellationRejectBeforeWriting()
    {
        ArtworkRecipe[] invalid=[Recipe(width:63), Recipe(width:4096,height:2160),
            Recipe(new(){["layers"]="6",["blobs"]="40"},width:1920,height:1080),
            Recipe(new(){["render-quality"]="fine-2x",["layers"]="4",["blobs"]="10"},width:1920,height:1080)];
        foreach(var r in invalid)
        {
            using var output=new MemoryStream();Assert.Throws<ArgumentOutOfRangeException>(()=>Engine.Render(r,output));Assert.Equal(0,output.Length);
        }
        using var cts=new CancellationTokenSource();cts.Cancel();using var canceled=new MemoryStream();
        Assert.Throws<OperationCanceledException>(()=>Engine.Render(Recipe(),canceled,cancellationToken:cts.Token));Assert.Equal(0,canceled.Length);
    }

    [Fact]
    public void FullHdFreshStudioCompositionAllowsFineQuality()
    {
        var p=new Dictionary<string,string>(StudioRecipeDefaults.ParametersFor("sdf-orbits"))
        { ["render-quality"]="fine-2x", ["blobs"]="14" };
        Engine.Validate(ArtworkRecipe.Create(Catalog.Get("sdf-orbits"),new CanvasSize(1920,1080),42,p));
    }
}
