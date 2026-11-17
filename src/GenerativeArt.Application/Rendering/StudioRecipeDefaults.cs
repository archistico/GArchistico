using System.Collections.ObjectModel;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Ricette iniziali dello Studio. Le 17 risorse JSON sono parte dell'assembly:
/// non dipendono dalla cartella corrente, dall'installazione o da file esterni.
/// I default del catalogo e la deserializzazione delle ricette storiche restano invariati.
/// </summary>
public static class StudioRecipeDefaults
{
    public const int Width = 1920;
    public const int Height = 1080;
    public const int FrameCount = 16;
    public const int FramesPerSecond = 18;

    private const string ResourcePrefix = "GenerativeArt.Application.Defaults.";
    private static readonly Lazy<IReadOnlyDictionary<string, ArtworkRecipe>> InitialRecipes =
        new(LoadInitialRecipes);

    /// <summary>Ricetta completa iniziale (seed, dimensioni, parametri, eventuale animazione).</summary>
    public static ArtworkRecipe RecipeFor(string artworkId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artworkId);
        return InitialRecipes.Value.TryGetValue(artworkId, out ArtworkRecipe? recipe)
            ? recipe
            : throw new KeyNotFoundException($"Nessuna ricetta iniziale per '{artworkId}'.");
    }

    /// <summary>Compatibilità con la CLI smoke e i test già presenti.</summary>
    public static IReadOnlyDictionary<string, string> ParametersFor(string artworkId) =>
        RecipeFor(artworkId).Parameters;

    private static IReadOnlyDictionary<string, ArtworkRecipe> LoadInitialRecipes()
    {
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        var recipes = new Dictionary<string, ArtworkRecipe>(StringComparer.Ordinal);
        foreach (ArtworkDescriptor descriptor in catalog.All)
        {
            string name = ResourcePrefix + descriptor.Id + ".json";
            using Stream stream = typeof(StudioRecipeDefaults).Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Risorsa di default mancante: '{name}'.");
            using var content = new MemoryStream();
            stream.CopyTo(content);
            ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(content.ToArray(), catalog);
            if (!string.Equals(recipe.ArtworkId, descriptor.Id, StringComparison.Ordinal))
                throw new InvalidOperationException($"Il file '{name}' contiene l'opera '{recipe.ArtworkId}'.");
            recipes.Add(descriptor.Id, recipe);
        }
        return new ReadOnlyDictionary<string, ArtworkRecipe>(recipes);
    }
}
