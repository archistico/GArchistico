using System.Collections.ObjectModel;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Recipes;

/// <summary>
/// Ricetta immutabile per versione esatta di un generatore. La validazione passa sempre
/// dal suo descrittore: chiavi sconosciute e valori fuori schema sono respinti.
/// </summary>
public sealed class ArtworkRecipe
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion => CurrentFormatVersion;
    public string ArtworkId { get; }
    public int AlgorithmVersion { get; }
    public ulong Seed { get; }
    public CanvasSize Size { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public RecipeAnimation? Animation { get; }

    private ArtworkRecipe(ArtworkDescriptor descriptor, CanvasSize size, ulong seed,
        IReadOnlyDictionary<string, string> parameters, RecipeAnimation? animation)
    {
        ArtworkId = descriptor.Id;
        AlgorithmVersion = descriptor.AlgorithmVersion;
        Size = size;
        Seed = seed;
        var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> parameter in parameters)
            sorted.Add(parameter.Key, parameter.Value);
        Parameters = new ReadOnlyDictionary<string, string>(sorted);
        Animation = animation;
    }

    public static ArtworkRecipe Create(ArtworkDescriptor descriptor, CanvasSize size, ulong seed,
        IReadOnlyDictionary<string, string>? parameters = null, RecipeAnimation? animation = null)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(size);
        if (animation is not null && !descriptor.SupportsAnimation)
            throw new ArgumentException($"L'opera '{descriptor.Id}' non supporta l'animazione.", nameof(animation));
        IReadOnlyDictionary<string, string> validated = descriptor.ValidateParameters(parameters);
        return new ArtworkRecipe(descriptor, size, seed, validated, animation);
    }

    public void ValidateAgainst(ArtworkRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArtworkDescriptor descriptor = registry.Get(ArtworkId);
        if (descriptor.AlgorithmVersion != AlgorithmVersion)
            throw new NotSupportedException(
                $"Versione dell'algoritmo non supportata per '{ArtworkId}': " +
                $"ricetta v{AlgorithmVersion}, catalogo v{descriptor.AlgorithmVersion}.");
        if (Animation is not null && !descriptor.SupportsAnimation)
            throw new ArgumentException($"L'opera '{ArtworkId}' non supporta l'animazione.");
        descriptor.ValidateParameters(Parameters);
    }
}
