using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class VoronoiDepthControlTests
{
    [Fact]
    public void FlatModeDisablesNewControlsWithoutChangingThePersonalPreset()
    {
        var d=BuiltInArtworkCatalog.Create().Get("voronoi-mosaic");
        var original=StudioRecipeDefaults.RecipeFor(d.Id);
        Assert.Equal("flat",original.Parameters["depth-style"]);
        Assert.Equal("81",original.Parameters["sites"]);
        Assert.Equal("fine-2x",original.Parameters["render-quality"]);
        foreach(var control in VoronoiDepthControls.All.Where(p=>p.Id!="depth-style"))
            Assert.NotNull(StudioParameterApplicability.InactiveReason(d.Id,control.Id,original.Parameters));
        var p=new Dictionary<string,string>(original.Parameters){["depth-style"]="layered"};
        Assert.Null(StudioParameterApplicability.InactiveReason(d.Id,"depth-layers",p));
        Assert.Null(StudioParameterApplicability.InactiveReason(d.Id,"tile-relief",p));
        p["missing-tiles"]="0";p["tile-relief"]="0";
        Assert.NotNull(StudioParameterApplicability.InactiveReason(d.Id,"hole-pattern",p));
        Assert.NotNull(StudioParameterApplicability.InactiveReason(d.Id,"bevel-width",p));
        for (int i=1;i<=5;i++)p[$"tile-color-{i}"]="#DDEEFF";
        Assert.NotNull(StudioParameterApplicability.InactiveReason(d.Id,"back-palette",p));
    }
}
