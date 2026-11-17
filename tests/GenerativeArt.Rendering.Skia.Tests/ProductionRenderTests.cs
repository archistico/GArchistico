using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ProductionRenderTests
{
    private static readonly ArtworkRegistry Catalog = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Catalog);
    public static IEnumerable<object[]> Artworks() => Catalog.All.Select(d => new object[] { d.Id });

    [Theory]
    [MemberData(nameof(Artworks))]
    public void EveryGeneratorAllowsFourKAtMaximumComplexity(string id)
    {
        ArtworkRecipe recipe = ProductionRenderProfile.PrepareFourK(StudioRecipeDefaults.RecipeFor(id), Catalog);
        Engine.Validate(recipe, recipe.Animation is null ? null : 0);
        foreach (CanvasSize size in new[] { new CanvasSize(4096, 2160), new CanvasSize(2160, 4096) })
        {
            ArtworkRecipe dci = ArtworkRecipe.Create(Catalog.Get(id), size, recipe.Seed, recipe.Parameters, recipe.Animation);
            Engine.Validate(dci, dci.Animation is null ? null : 0);
        }
    }

    [Fact]
    public void GeneralEightKIsRejectedBeforeWriting()
    {
        ArtworkRecipe recipe = ArtworkRecipe.Create(Catalog.Get("plasma-noise"), new CanvasSize(7680, 4320), 42,
            new Dictionary<string, string> { ["render-profile"] = "production", ["render-quality"] = "fine-2x" });
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, output));
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData("plasma-noise")]
    [InlineData("domain-warp")]
    [InlineData("lic-flow")]
    [InlineData("wave-interference")]
    [InlineData("reaction-diffusion")]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    [InlineData("sdf-orbits")]
    public void ParallelProductionPreservesStandardPixels(string id)
    {
        ArtworkRecipe original = ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(80, 64), 42);
        var parameters = new Dictionary<string, string>(original.Parameters) { ["render-profile"] = "production" };
        ArtworkRecipe production = ArtworkRecipe.Create(Catalog.Get(id), original.Size, original.Seed, parameters);
        Assert.Equal(Render(original), Render(production));
    }

    [Theory]
    [InlineData("ocellus-eyespots")]
    [InlineData("rete-foam")]
    [InlineData("sdf-orbits")]
    public void FineProductionHasRealDeterministicSamplingAndFinalDimensions(string id)
    {
        ArtworkRecipe original = ArtworkRecipe.Create(Catalog.Get(id), new CanvasSize(80, 64), 42,
            new Dictionary<string, string> { ["render-profile"] = "production" });
        ArtworkRecipe fine = ArtworkRecipe.Create(Catalog.Get(id), original.Size, original.Seed,
            new Dictionary<string, string>(original.Parameters) { ["render-quality"] = "fine-2x" });
        byte[] png = Render(fine);
        Assert.Equal(png, Render(fine));
        Assert.False(png.AsSpan().SequenceEqual(Render(original)));
        using SKBitmap decoded = SKBitmap.Decode(png);
        Assert.Equal(80, decoded.Width);
        Assert.Equal(64, decoded.Height);
    }

    private static byte[] Render(ArtworkRecipe recipe)
    {
        using var output = new MemoryStream();
        Engine.Render(recipe, output);
        return output.ToArray();
    }
}
