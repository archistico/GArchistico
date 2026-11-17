using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class StudioExactPreviewRenderingTests
{
    [Theory]
    [InlineData("plasma-noise", 192, 128)]
    [InlineData("voronoi-mosaic", 160, 120)]
    [InlineData("alien-letters", 180, 120)]
    [InlineData("cotton-circles", 180, 120)]
    [InlineData("rigor-grid", 180, 120)]
    [InlineData("crinis-bristles", 180, 120)]
    [InlineData("spiral-flamebrush", 180, 120)]
    [InlineData("fascia-ribbons", 180, 120)]
    [InlineData("eclectic-trails", 180, 120)]
    [InlineData("ocellus-eyespots", 180, 120)]
    [InlineData("rete-foam", 180, 120)]
    public async Task FullResolutionPreviewAndExportHaveIdenticalPngAndAlpha(string id, int width, int height)
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        var renderer = new ArtworkRenderEngine(catalog);
        var coordinator = new StudioRenderCoordinator(renderer);
        ArtworkRecipe recipe = ArtworkRecipe.Create(catalog.Get(id),
            new CanvasSize(width, height), 8675309UL);
        ArtworkRecipe exactPreview = StudioPreviewPolicy.CreateExactPreview(recipe, catalog);

        byte[] preview = await coordinator.RenderPngAsync(exactPreview);
        using var export = new MemoryStream();
        await coordinator.RenderToStreamAsync(recipe, export);
        byte[] output = export.ToArray();

        Assert.Equal(output, preview);
        using SKBitmap bitmap = SKBitmap.Decode(preview) ?? throw new InvalidOperationException("PNG non valido");
        Assert.Equal(width, bitmap.Width);
        Assert.Equal(height, bitmap.Height);
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    public async Task G4R2AdvancedControlsKeepExactPreviewAndExportEqual(string id)
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        var coordinator = new StudioRenderCoordinator(new ArtworkRenderEngine(catalog));
        var parameters = id == "ocellus-eyespots"
            ? new Dictionary<string, string> { ["rim-light"] = "0.8", ["pore-strength"] = "0.7" }
            : new Dictionary<string, string> { ["membrane-opacity"] = "0.5", ["node-glow"] = "0.65" };
        ArtworkRecipe recipe = ArtworkRecipe.Create(catalog.Get(id), new CanvasSize(180, 128), 8675309UL,
            parameters);
        byte[] preview = await coordinator.RenderPngAsync(StudioPreviewPolicy.CreateExactPreview(recipe, catalog));
        using var output = new MemoryStream();
        await coordinator.RenderToStreamAsync(recipe, output);
        Assert.Equal(preview, output.ToArray());
    }
}
