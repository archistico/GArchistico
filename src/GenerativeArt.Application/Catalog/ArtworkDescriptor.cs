using System.Collections.ObjectModel;

namespace GenerativeArt.Application.Catalog;

/// <summary>Metadati immutabili; nessuna dipendenza da renderer o GUI.</summary>
public sealed class ArtworkDescriptor
{
    public string Id { get; }
    public int AlgorithmVersion { get; }
    public string Title { get; }
    public string Category { get; }
    public string Description { get; }
    public ArtworkCapabilities Capabilities { get; }
    public IReadOnlyList<ArtworkParameterSpec> Parameters { get; }
    public bool SupportsAnimation => (Capabilities & ArtworkCapabilities.Animated) != 0;
    public bool SupportsSeamlessLoop => (Capabilities & ArtworkCapabilities.SeamlessLoop) != 0;

    public ArtworkDescriptor(string id, int algorithmVersion, string title, string category,
        string description, ArtworkCapabilities capabilities, IEnumerable<ArtworkParameterSpec> parameters)
    {
        ArtworkIds.Validate(id, nameof(id));
        if (algorithmVersion < 1) throw new ArgumentOutOfRangeException(nameof(algorithmVersion));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Titolo mancante.", nameof(title));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Categoria mancante.", nameof(category));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Descrizione mancante.", nameof(description));
        if (capabilities == ArtworkCapabilities.None ||
            (capabilities & ~(ArtworkCapabilities.Raster | ArtworkCapabilities.Vector |
                               ArtworkCapabilities.Animated | ArtworkCapabilities.SeamlessLoop)) != 0 ||
            (capabilities & (ArtworkCapabilities.Raster | ArtworkCapabilities.Vector)) == 0 ||
            ((capabilities & ArtworkCapabilities.SeamlessLoop) != 0 &&
             (capabilities & ArtworkCapabilities.Animated) == 0))
            throw new ArgumentException("Capacità non coerenti.", nameof(capabilities));
        ArgumentNullException.ThrowIfNull(parameters);
        var items = parameters.ToArray();
        if (items.Any(p => p is null) ||
            items.Select(p => p.Id).Distinct(StringComparer.Ordinal).Count() != items.Length)
            throw new ArgumentException("Parametri null o identificativi duplicati.", nameof(parameters));
        Id = id;
        AlgorithmVersion = algorithmVersion;
        Title = title;
        Category = category;
        Description = description;
        Capabilities = capabilities;
        Parameters = Array.AsReadOnly(items);
    }

    /// <summary>Valida e normalizza una mappa completa di valori. Nessuna chiave sconosciuta.</summary>
    public IReadOnlyDictionary<string, string> ValidateParameters(
        IReadOnlyDictionary<string, string>? provided = null)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (provided is not null)
            foreach (string key in provided.Keys)
                if (!Parameters.Any(p => p.Id == key))
                    throw new ArgumentException($"Parametro sconosciuto: {key}.", nameof(provided));
        foreach (ArtworkParameterSpec param in Parameters)
        {
            string value = provided is not null && provided.TryGetValue(param.Id, out string? supplied)
                ? supplied ?? throw new ArgumentException($"Valore nullo per '{param.Id}'.", nameof(provided))
                : param.DefaultValue;
            result[param.Id] = param.ValidateValue(value);
        }
        return new ReadOnlyDictionary<string, string>(result);
    }
}
