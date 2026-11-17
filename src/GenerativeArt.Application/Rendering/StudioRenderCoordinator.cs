using GenerativeArt.Application.Files;
using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Serializza i rendering dello Studio (anteprima ed export) su una sola istanza
/// di renderer. Non dipende da Avalonia, non memorizza bitmap e non cambia ricette.
/// Il chiamante gestisce la cancellazione delle richieste obsolete.
/// </summary>
public sealed class StudioRenderCoordinator
{
    private readonly IArtworkRasterRenderer _renderer;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public StudioRenderCoordinator(IArtworkRasterRenderer renderer) =>
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

    /// <summary>Restituisce i byte PNG per una singola anteprima alla risoluzione della ricetta.</summary>
    public async Task<byte[]> RenderPngAsync(ArtworkRecipe recipe, int? frameIndex = null,
        CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await RenderToStreamAsync(recipe, buffer, frameIndex, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }

    /// <summary>Rendering su stream di proprietà del chiamante, senza chiuderlo.</summary>
    public async Task RenderToStreamAsync(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Lo stream deve essere scrivibile.", nameof(destination));
        _renderer.Validate(recipe, frameIndex);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(() => _renderer.Render(recipe, destination, frameIndex, cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Genera il PNG direttamente in un file temporaneo. Il rename atomico avviene
    /// solo dopo il rendering completo; in caso di cancellazione conserva il file precedente.
    /// </summary>
    public async Task RenderToAtomicFileAsync(ArtworkRecipe recipe, string destination,
        int? frameIndex = null, CancellationToken cancellationToken = default)
    {
        _renderer.Validate(recipe, frameIndex);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AtomicOutputFiles.WriteGeneratedAsync(destination,
                (stream, token) => _renderer.Render(recipe, stream, frameIndex, token),
                overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }
}
