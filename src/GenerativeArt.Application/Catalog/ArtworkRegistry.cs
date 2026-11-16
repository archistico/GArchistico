namespace GenerativeArt.Application.Catalog;

/// <summary>Catalogo esplicito; non effettua reflection sui campi delle opere.</summary>
public sealed class ArtworkRegistry
{
    private readonly Dictionary<string, ArtworkDescriptor> _byId;
    private readonly ArtworkDescriptor[] _all;

    public IReadOnlyList<ArtworkDescriptor> All { get; }

    public ArtworkRegistry(IEnumerable<ArtworkDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        _byId = new Dictionary<string, ArtworkDescriptor>(StringComparer.Ordinal);
        foreach (ArtworkDescriptor descriptor in descriptors)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            if (!_byId.TryAdd(descriptor.Id, descriptor))
                throw new ArgumentException($"Opera duplicata: {descriptor.Id}.", nameof(descriptors));
        }
        _all = _byId.Values.OrderBy(d => d.Id, StringComparer.Ordinal).ToArray();
        All = Array.AsReadOnly(_all);
    }

    public bool TryGet(string id, out ArtworkDescriptor? descriptor) =>
        _byId.TryGetValue(id, out descriptor);

    public ArtworkDescriptor Get(string id) => TryGet(id, out ArtworkDescriptor? descriptor)
        ? descriptor! : throw new KeyNotFoundException($"Opera sconosciuta: {id}.");
}
