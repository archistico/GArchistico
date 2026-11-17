using System.Numerics;
using System.Text.Json.Nodes;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class LayeredVoronoiTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);
    private static ArtworkRecipe Recipe(Dictionary<string,string>? values = null, ulong seed = 42, int width = 160, int height = 120)
    {
        var p = new Dictionary<string,string> { ["depth-style"]="layered", ["sites"]="48", ["depth-layers"]="3" };
        if (values is not null) foreach (var pair in values) p[pair.Key] = pair.Value;
        return ArtworkRecipe.Create(Catalog.Get("voronoi-mosaic"), new CanvasSize(width,height),seed,p);
    }
    private static byte[] Render(ArtworkRecipe r)
    { using var stream = new MemoryStream(); Engine.Render(r,stream); return stream.ToArray(); }

    [Theory]
    [InlineData("standard")]
    [InlineData("fine-2x")]
    public void LayeredMosaicIsSeededAndRoundTripsWithoutChangingDimensions(string quality)
    {
        var p = new Dictionary<string,string> { ["render-quality"]=quality };
        var r = Recipe(p); byte[] png = Render(r);
        Assert.Equal(png,Render(r));
        Assert.False(png.AsSpan().SequenceEqual(Render(Recipe(p,seed:43))));
        Assert.Equal(png,Render(ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(r,Catalog),Catalog)));
        using var image = SKBitmap.Decode(png);
        Assert.Equal(160,image.Width); Assert.Equal(120,image.Height);
        Assert.True(image.Pixels.Distinct().Count()>1000);
    }

    [Theory]
    [InlineData("depth-layers","5")]
    [InlineData("missing-tiles","0")]
    [InlineData("missing-tiles","0.8")]
    [InlineData("hole-pattern","random")]
    [InlineData("back-density","2.8")]
    [InlineData("parallax-x","80")]
    [InlineData("parallax-y","60")]
    [InlineData("distance-dimming","0.8")]
    [InlineData("atmosphere","0.75")]
    [InlineData("depth-blur","24")]
    [InlineData("back-palette","copper")]
    [InlineData("shadow-opacity","0")]
    [InlineData("shadow-softness","20")]
    [InlineData("shadow-distance","40")]
    [InlineData("light-angle","120")]
    [InlineData("tile-relief","0")]
    [InlineData("bevel-width","12")]
    [InlineData("edge-opacity","0.9")]
    [InlineData("tile-variation","0.5")]
    [InlineData("accent-strength","0.8")]
    [InlineData("shadow-color","#E84020")]
    [InlineData("depth-color","#40FF40")]
    public void CreativeDepthControlsActuallyAffectTheRaster(string key,string value) =>
        Assert.False(Render(Recipe()).AsSpan().SequenceEqual(Render(Recipe(new() { [key]=value }))));

    [Fact]
    public void RemovedTilesRevealTheRearMosaicInsteadOfPaintingOverIt()
    {
        var p = new Dictionary<string,string>
        {
            ["depth-layers"]="2", ["missing-tiles"]="0", ["depth-blur"]="0", ["shadow-opacity"]="0",
            ["tile-relief"]="0", ["edge-opacity"]="0", ["distance-dimming"]="0",
            ["depth-color"]="#00FF00", ["atmosphere"]="0.8", ["background-color"]="#0000FF"
        };
        for(int i=1;i<=5;i++)p[$"tile-color-{i}"]="#FF0000";
        using var closed = SKBitmap.Decode(Render(Recipe(p)));
        p["missing-tiles"]="1";
        using var open = SKBitmap.Decode(Render(Recipe(p)));
        Assert.True(closed.Pixels.Count(c=>c.Red>240 && c.Green<5)>closed.Pixels.Length*.7);
        Assert.True(open.Pixels.Count(c=>c.Green>150 && c.Red<100)>open.Pixels.Length*.7);
    }

    [Fact]
    public void BeveledTranslucentTilesDoNotAccumulateAlphaBetweenFaceAndRim()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(80,80));
        surface.DrawBeveledConvexPolygon([new(10,10),new(70,10),new(70,70),new(10,70)],
            new Color32(128,100,150,200),Color32.Transparent,1,8,.9,-Math.PI/3);
        using var stream=new MemoryStream();surface.WritePng(stream);
        using var image=SKBitmap.Decode(stream.ToArray());
        foreach(var p in new[]{new Vector2(40,40),new(12,40),new(40,12),new(40,67)})
            Assert.InRange(image.GetPixel((int)p.X,(int)p.Y).Alpha,(byte)127,(byte)128);
        Assert.Equal((byte)0,image.GetPixel(2,2).Alpha);
        Assert.NotEqual(image.GetPixel(12,40),image.GetPixel(67,40));
    }

    [Fact]
    public void TransparentBackingsAndPigmentsRetainPartialAlpha()
    {
        var p=new Dictionary<string,string>{["background-color"]="#00000000",["edge-opacity"]="0",["shadow-opacity"]="0"};
        for(int i=1;i<=5;i++)p[$"tile-color-{i}"]="#6080CCEE";
        using var image=SKBitmap.Decode(Render(Recipe(p)));
        Assert.Contains(image.Pixels,c=>c.Alpha is >0 and <255);
        Assert.DoesNotContain(image.Pixels,c=>c.Alpha==255);
    }

    [Fact]
    public void OldJsonRestoresFlatAndEveryNewControlIsOptional()
    {
        var r=Recipe(new(){["depth-style"]="flat"});
        var json=JsonNode.Parse(ArtworkRecipeJson.Serialize(r,Catalog))!;
        foreach(var p in VoronoiDepthControls.All)
        {Assert.True(p.OptionalInJson);json["parameters"]!.AsObject().Remove(p.Id);}
        var restored=ArtworkRecipeJson.Deserialize(json.ToJsonString(),Catalog);
        Assert.Equal("flat",restored.Parameters["depth-style"]);
        Assert.Equal(Render(r),Render(restored));
    }

    [Fact]
    public void FlatEightKIsRetainedAndExcessiveLayeredWorkIsRejectedBeforeOutput()
    {
        Engine.Validate(Recipe(new(){["depth-style"]="flat"},width:7680,height:4320));
        Engine.Validate(Recipe(new(){["render-quality"]="fine-2x"},width:1920,height:1080));
        ArtworkRecipe[] invalid=[Recipe(width:7680,height:4320),
            Recipe(new(){["sites"]="196",["relaxations"]="3",["depth-layers"]="8",["back-density"]="3"}),
            Recipe(new(){["render-quality"]="fine-2x",["depth-layers"]="8",["depth-blur"]="32",["shadow-softness"]="24"},width:1448,height:1448)];
        foreach(var r in invalid)
        {using var output=new MemoryStream();Assert.Throws<ArgumentOutOfRangeException>(()=>Engine.Render(r,output));Assert.Equal(0,output.Length);}
        using var canceled=new MemoryStream();
        Assert.Throws<OperationCanceledException>(()=>Engine.Render(Recipe(),canceled,cancellationToken:new CancellationToken(true)));
        Assert.Equal(0,canceled.Length);
    }
}
