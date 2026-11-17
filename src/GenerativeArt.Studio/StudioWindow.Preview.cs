using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Files;
using GenerativeArt.Application.Preferences;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;

namespace GenerativeArt.Studio;

/// <summary>Anteprima, ridimensionamento viewport e cancellazione.</summary>
public sealed partial class StudioWindow
{
    // Nessuna riscalatura preliminare: l'anteprima usa gli stessi pixel dell'export.
    // Zoom e adattamento agiscono esclusivamente sulla vista Avalonia.
    private ArtworkRecipe BuildPreviewRecipe(ArtworkRecipe original) =>
        StudioPreviewPolicy.CreateExactPreview(original, _catalog);

    private void UpdatePreviewArea()
    {
        if (_previewStage is null) return;
        // Applicare soltanto valori validi: durante la digitazione i campi
        // passano temporaneamente per stringhe vuote.
        if (int.TryParse(_previewAreaWidth.Text, NumberStyles.None, CultureInfo.InvariantCulture,
                out int width) && width is >= 180 and <= 4096)
            _previewStage.MaxWidth = width;
        if (int.TryParse(_previewAreaHeight.Text, NumberStyles.None, CultureInfo.InvariantCulture,
                out int height) && height is >= 160 and <= 4096)
            _previewStage.MaxHeight = height;
    }

    private void SchedulePreview()
    {
        if (_closed) return;
        if (_exportRunning)
        {
            // Non lasciare pubblicare un'immagine ormai obsoleta mentre il
            // dialogo di export è attivo: rifaremo il render alla fine.
            _previewCancellation?.Cancel();
            _previewDeferred = true;
            return;
        }
        bool playbackFrame = _playTimer.IsEnabled;
        // Durante il playback non applicare il debounce: altrimenti, a 18 FPS,
        // ogni tick cancella quello precedente prima che inizi il rendering.
        // Saltare i tick se la GPU/CPU non ha terminato il frame corrente.
        if (playbackFrame && _playbackFrameBusy) return;
        if (playbackFrame) _playbackFrameBusy = true;
        _previewCancellation?.Cancel();
        var source = new CancellationTokenSource();
        _previewCancellation = source;
        long revision = ++_revision;
        if (!playbackFrame)
            SetStatus("Calcolo anteprima ai pixel finali… l'immagine precedente resta visibile fino al completamento.");
        _ = RenderPreviewAsync(revision, source, playbackFrame);
    }

    private async Task RenderPreviewAsync(long revision, CancellationTokenSource source, bool playbackFrame)
    {
        CancellationToken ct = source.Token;
        try
        {
            if (!playbackFrame) await Task.Delay(140, ct);
            if (_closed || ct.IsCancellationRequested) return;
            ArtworkRecipe original = BuildRecipe();
            ArtworkRecipe previewRecipe = BuildPreviewRecipe(original);
            int? frameIndex = previewRecipe.Animation is null ? null : _currentFrame;
            _renderer.Validate(previewRecipe, frameIndex);
            SetStatus("Rendering in corso…");
            byte[] png = await _renderCoordinator.RenderPngAsync(previewRecipe, frameIndex, ct);
            if (_closed || ct.IsCancellationRequested || revision != _revision) return;
            using var data = new MemoryStream(png, writable: false);
            Bitmap replacement = new(data);
            Bitmap? previous = _bitmap;
            _bitmap = replacement;
            _preview.SetBitmap(replacement);
            previous?.Dispose();
            SetStatus($"Anteprima reale: {previewRecipe.Size.Width} × {previewRecipe.Size.Height} px" +
                $"  ·  vista adattata al riquadro  ·  seed {previewRecipe.Seed}" +
                (frameIndex is null ? "" : $"  ·  frame {frameIndex + 1}"));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!_closed && !ct.IsCancellationRequested && revision == _revision)
            {
                // Evitare di far scambiare per valida una vecchia immagine
                // dopo che la nuova ricetta ha prodotto un errore.
                _preview.SetBitmap(null);
                _bitmap?.Dispose();
                _bitmap = null;
                SetStatus("Errore anteprima: " + ex.Message, error: true);
            }
        }
        finally
        {
            if (playbackFrame) _playbackFrameBusy = false;
            if (ReferenceEquals(_previewCancellation, source)) _previewCancellation = null;
            source.Dispose();
        }
    }

    private void ApplyZoom()
    {
        _zoomLabel.Text = $"{_zoom.Value * 100:0}%";
        _preview.SetZoom(_zoom.Value);
    }

}
