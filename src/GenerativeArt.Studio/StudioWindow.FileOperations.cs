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

/// <summary>Import, salvataggio ed export cancellabile/atomico.</summary>
public sealed partial class StudioWindow
{
    private async Task OpenRecipeAsync()
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Apri ricetta GenerativeArt", AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Ricette JSON") { Patterns = ["*.json"] }]
            });
            if (files.Count == 0) return;
            await using Stream stream = await files[0].OpenReadAsync();
            await LoadRecipeStreamAsync(stream);
            if (files[0].Path.IsFile)
                await RecordRecentAsync(files[0].Path.LocalPath);
            SetStatus("Ricetta caricata: " + files[0].Name);
        }
        catch (Exception ex) { SetStatus("Impossibile aprire la ricetta: " + ex.Message, error: true); }
    }

    private async Task OpenRecipeFromPathAsync(string path)
    {
        try
        {
            ArtworkRecipe loaded = await _recipeDocuments.LoadFileAsync(path);
            SelectArtwork(_catalog.Get(loaded.ArtworkId), loaded);
            await RecordRecentAsync(path);
            SetStatus("Ricetta caricata: " + Path.GetFileName(path));
        }
        catch (Exception ex) { SetStatus("Impossibile aprire la ricetta recente: " + ex.Message, error: true); }
    }

    private async Task LoadRecipeStreamAsync(Stream stream)
    {
        ArtworkRecipe loaded = await _recipeDocuments.LoadAsync(stream);
        SelectArtwork(_catalog.Get(loaded.ArtworkId), loaded);
    }

    private async Task SaveRecipeAsync()
    {
        try
        {
            ArtworkRecipe recipe = BuildRecipe();
            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salva ricetta", SuggestedFileName = recipe.ArtworkId + ".json",
                DefaultExtension = "json", ShowOverwritePrompt = true,
                FileTypeChoices = [new FilePickerFileType("Ricette JSON") { Patterns = ["*.json"] }]
            });
            if (file is null) return;
            if (file.Path.IsFile)
                await _recipeDocuments.SaveFileAsync(file.Path.LocalPath, recipe);
            else
            {
                // I provider virtuali non garantiscono la pubblicazione atomica.
                await using Stream output = await file.OpenWriteAsync();
                await _recipeDocuments.SaveStreamAsync(output, recipe);
            }
            if (file.Path.IsFile)
                await RecordRecentAsync(file.Path.LocalPath);
            SetStatus("Ricetta salvata: " + file.Name);
        }
        catch (Exception ex) { SetStatus("Impossibile salvare la ricetta: " + ex.Message, error: true); }
    }

    private async Task ExportPngAsync()
    {
        if (_exportRunning || _closed) return;
        _exportRunning = true;
        _export.IsEnabled = false;
        try
        {
            ArtworkRecipe recipe = BuildRecipe();
            int? frame = recipe.Animation is null ? null : _currentFrame;
            _renderer.Validate(recipe, frame);
            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = recipe.Animation is null ? "Esporta PNG" : "Esporta PNG del fotogramma corrente",
                SuggestedFileName = recipe.ArtworkId + (frame is null ? "" : $"-frame-{frame.Value:D4}") + ".png",
                DefaultExtension = "png", ShowOverwritePrompt = true,
                FileTypeChoices = [new FilePickerFileType("Immagini PNG") { Patterns = ["*.png"] }]
            });
            if (file is null || _closed) return;
            using var source = new CancellationTokenSource();
            _exportCancellation = source;
            _cancelExport.IsEnabled = true;
            _exportProgress.IsVisible = true;
            StopPlayback();
            if (_previewCancellation is not null)
            {
                _previewDeferred = true;
                _previewCancellation.Cancel();
            }
            CancellationToken ct = source.Token;
            SetStatus("Export PNG: attesa del renderer…  ·  Annulla export per interrompere");
            if (file.Path.IsFile)
            {
                // Lo stesso coordinatore serializza export e anteprima, senza duplicare
                // il PNG in memoria durante l'esportazione su file locale.
                SetStatus("Export PNG: rendering e scrittura su file temporaneo…");
                await _renderCoordinator.RenderToAtomicFileAsync(recipe, file.Path.LocalPath, frame, ct);
            }
            else
            {
                // Provider virtuali: non garantiscono rename atomico. L'unico
                // buffer PNG viene creato dal coordinatore; stream gestito dal picker.
                SetStatus("Export PNG: rendering per provider virtuale (non atomico)…");
                byte[] png = await _renderCoordinator.RenderPngAsync(recipe, frame, ct);
                ct.ThrowIfCancellationRequested();
                await using Stream output = await file.OpenWriteAsync();
                await output.WriteAsync(png, ct);
                await output.FlushAsync(ct);
            }
            if (!_closed)
                SetStatus("PNG esportato: " + file.Name +
                    (frame is null ? "" : " (solo fotogramma corrente; sequenza completa via CLI)"));
        }
        catch (OperationCanceledException)
        {
            if (!_closed) SetStatus("Esportazione PNG annullata. Nessun file locale incompleto pubblicato.");
        }
        catch (Exception ex)
        {
            if (!_closed) SetStatus("Impossibile esportare PNG: " + ex.Message, error: true);
        }
        finally
        {
            _exportCancellation = null;
            _cancelExport.IsEnabled = false;
            _exportProgress.IsVisible = false;
            _export.IsEnabled = true;
            _exportRunning = false;
            if (_previewDeferred && !_closed)
            {
                _previewDeferred = false;
                SchedulePreview();
            }
        }
    }

}
