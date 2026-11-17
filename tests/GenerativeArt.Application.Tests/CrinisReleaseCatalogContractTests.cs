using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

/// <summary>
/// Verifica il contratto dell'assembly Application effettivamente usato dai test:
/// una DLL Release della R2 accoppiata ai renderer/test R3 non lo supera.
/// </summary>
public sealed class CrinisReleaseCatalogContractTests
{
    [Fact]
    public void MicroInkFlecksDoNotPolluteGlobalFinishingGroup()
    {
        ArtworkDescriptor crinis = BuiltInArtworkCatalog.Create().Get("crinis-bristles");
        Assert.Equal("Microfibre", crinis.Parameters.Single(p => p.Id == "ink-flecks").Group);
        Assert.Equal(new[] { "grain-amount", "grain-size", "dither-mode", "dither-levels" },
            crinis.Parameters.Where(p => p.Group == "Finitura").Select(p => p.Id));
    }

    [Fact]
    public void CompiledApplicationCatalogContainsAllR3Controls()
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        ArtworkDescriptor descriptor = catalog.Get("crinis-bristles");
        string[] addedControls =
        [
            "body-opacity", "fiber-relief", "fiber-fray", "detail-style",
            "fiber-density", "fiber-coverage", "pigment-contrast", "micro-detail", "fiber-taper",
            "fiber-shadow-depth", "vortex-centers", "ink-flecks",
            "bundle-compression", "bundle-volume", "bundle-lighting",
            "intra-bundle-depth", "featured-fibers", "cross-section-light", "internal-occlusion"
        ];
        foreach (string id in addedControls)
        {
            ArtworkParameterSpec? spec = descriptor.Parameters.SingleOrDefault(p => p.Id == id);
            Assert.True(spec is not null, $"Parametro Crinis mancante nell'assembly Application compilato: {id}. " +
                "Eseguire dotnet clean -c Release, poi .\\tools\\validate.ps1.");
            Assert.True(spec!.OptionalInJson, $"Il parametro {id} deve essere opzionale nei JSON v1 precedenti.");
        }
        ArtworkRecipe initial = StudioRecipeDefaults.RecipeFor("crinis-bristles");
        Assert.Equal("8", descriptor.Parameters.Single(p => p.Id == "hair-width").Maximum);
        Assert.Equal("micro", initial.Parameters["detail-style"]);
        Assert.Equal("0", initial.Parameters["body-opacity"]);
        Assert.Equal(descriptor.Parameters.Count, initial.Parameters.Count);
        ArtworkRecipe roundTrip = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(initial, catalog), catalog);
        Assert.Equal(initial.Parameters.OrderBy(p => p.Key), roundTrip.Parameters.OrderBy(p => p.Key));
    }
}
