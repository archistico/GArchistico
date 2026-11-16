using System.Text.Json;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Cli;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class RecipeOutputWriterTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();

    private static string NewFolder() => Path.Combine(Path.GetTempPath(),
        "generativeart-m3c3-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void StaticRecipeProducesPngAndNeverOverwritesExistingFile()
    {
        string folder = NewFolder();
        string path = Path.Combine(folder, "plasma.png");
        try
        {
            ArtworkRecipe recipe = ArtworkRecipe.Create(Catalog.Get("plasma-noise"),
                new CanvasSize(80, 72), 123UL);
            var renderer = new ArtworkRenderEngine(Catalog);
            RecipeOutputWriter.Write(recipe, path, renderer);
            using SKBitmap bitmap = SKBitmap.Decode(path) ?? throw new InvalidOperationException();
            Assert.Equal(80, bitmap.Width);
            byte[] original = File.ReadAllBytes(path);
            Assert.Throws<IOException>(() => RecipeOutputWriter.Write(recipe, path, renderer));
            Assert.Equal(original, File.ReadAllBytes(path));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void LoopRecipeProducesExactFrameCountAndWritesManifestLast()
    {
        string folder = NewFolder();
        try
        {
            var recipe = ArtworkRecipe.Create(Catalog.Get("looping-flow"),
                new CanvasSize(64, 64), 123UL, animation: new RecipeAnimation(3, 24));
            var engine = new ArtworkRenderEngine(Catalog);
            RecipeOutputWriter.Write(recipe, folder, engine);
            string[] frames = Directory.GetFiles(folder, "frame_*.png");
            Assert.Equal(3, frames.Length);
            foreach (string path in frames)
            {
                using SKBitmap bitmap = SKBitmap.Decode(path) ?? throw new InvalidOperationException();
                Assert.Equal(64, bitmap.Width);
            }
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));
            Assert.Equal("looping-flow", manifest.RootElement.GetProperty("ArtworkId").GetString());
            Assert.Equal(3, manifest.RootElement.GetProperty("FrameCount").GetInt32());
            Assert.Throws<IOException>(() => RecipeOutputWriter.Write(recipe, folder, engine));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void FailedFrameRenderRemovesOnlyItsOwnOutput()
    {
        string folder = NewFolder();
        Directory.CreateDirectory(folder);
        try
        {
            ArtworkRecipe recipe = ArtworkRecipe.Create(Catalog.Get("looping-flow"),
                new CanvasSize(64, 64), 1UL, animation: new RecipeAnimation(3, 24));
            Assert.Throws<InvalidOperationException>(() => RecipeOutputWriter.Write(
                recipe, folder, new FailingRenderer()));
            Assert.Empty(Directory.EnumerateFileSystemEntries(folder));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void FailedStaticRenderDoesNotLeavePartialFile()
    {
        string folder = NewFolder();
        string path = Path.Combine(folder, "failed.png");
        try
        {
            ArtworkRecipe recipe = ArtworkRecipe.Create(Catalog.Get("plasma-noise"),
                new CanvasSize(64, 64), 1UL);
            Assert.Throws<InvalidOperationException>(() => RecipeOutputWriter.Write(
                recipe, path, new FailingRenderer()));
            Assert.False(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    private sealed class FailingRenderer : IArtworkRasterRenderer
    {
        public void Validate(ArtworkRecipe recipe, int? frameIndex = null) { }
        public void Render(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
            CancellationToken cancellationToken = default)
        {
            destination.WriteByte(123);
            if (frameIndex != 0) throw new InvalidOperationException("Errore simulato di rendering.");
        }
    }
}
