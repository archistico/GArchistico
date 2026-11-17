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
}
