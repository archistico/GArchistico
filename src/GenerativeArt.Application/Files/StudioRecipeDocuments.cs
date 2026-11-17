using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Application.Files;

/// <summary>
/// Operazioni sui documenti ricetta dello Studio, indipendenti da Avalonia e dai file picker.
/// Import bounded, convalida prima di modificare lo stato UI ed esportazione locale atomica.
/// Lo stream del chiamante resta di sua proprietà.
/// </summary>
public sealed class StudioRecipeDocuments
{
    private readonly ArtworkRegistry _catalog;
    private readonly IArtworkRasterRenderer _renderer;

    public StudioRecipeDocuments(ArtworkRegistry catalog, IArtworkRasterRenderer renderer)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public async Task<ArtworkRecipe> LoadAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Lo stream ricetta deve essere leggibile.", nameof(source));

        // Limite applicato durante la lettura, senza dipendere da Seek/Length.
        // Leggere un byte oltre il limite basta per rifiutare il documento.
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[4096];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int remaining = ArtworkRecipeJson.MaximumUtf8Bytes + 1 - (int)buffer.Length;
            int read = await source.ReadAsync(chunk.AsMemory(0, Math.Min(chunk.Length, remaining)),
                cancellationToken).ConfigureAwait(false);
            if (read == 0) break;
            if (buffer.Length + read > ArtworkRecipeJson.MaximumUtf8Bytes)
                throw new InvalidDataException("Ricetta oltre 64 KiB.");
            buffer.Write(chunk, 0, read);
        }
        cancellationToken.ThrowIfCancellationRequested();
        ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(buffer.ToArray(), _catalog);
        _renderer.Validate(recipe, recipe.Animation is null ? null : 0);
        return recipe;
    }

    public async Task<ArtworkRecipe> LoadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using var source = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 4096, useAsync: true);
        return await LoadAsync(source, cancellationToken).ConfigureAwait(false);
    }

    public byte[] Serialize(ArtworkRecipe recipe) => ArtworkRecipeJson.Serialize(recipe, _catalog);

    /// <summary>Un file locale è pubblicato solo dopo il completamento della scrittura.</summary>
    public Task SaveFileAsync(string path, ArtworkRecipe recipe, CancellationToken cancellationToken = default) =>
        AtomicOutputFiles.WriteAsync(path, Serialize(recipe), overwrite: true, cancellationToken: cancellationToken);

    /// <summary>Per i provider virtuali, che non supportano la rinomina atomica.</summary>
    public async Task SaveStreamAsync(Stream destination, ArtworkRecipe recipe,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Lo stream destinazione deve essere scrivibile.", nameof(destination));
        byte[] data = Serialize(recipe);
        await destination.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
