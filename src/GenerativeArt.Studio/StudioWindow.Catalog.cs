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

/// <summary>Catalogo, preferiti, recenti e preferenze.</summary>
public sealed partial class StudioWindow
{
    private void PopulateCatalog()
    {
        _catalogItems.Children.Clear();
        string query = _search.Text?.Trim() ?? "";
        foreach (ArtworkDescriptor artwork in _catalog.All)
        {
            if (_favoritesOnly.IsChecked == true && !_preferences.IsFavorite(artwork.Id)) continue;
            if (!string.IsNullOrEmpty(query) &&
                !(artwork.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                  artwork.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                  artwork.Category.Contains(query, StringComparison.OrdinalIgnoreCase))) continue;
            var heading = new TextBlock { Text = (_preferences.IsFavorite(artwork.Id) ? "★ " : "") + artwork.Title,
                FontSize = 14, FontWeight = FontWeight.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.NoWrap };
            var detail = new TextBlock { Text = artwork.Category +
                (artwork.SupportsSeamlessLoop ? "  ·  LOOP" : "  ·  STATIC"), FontSize = 11,
                Foreground = Ink("#A8BDCE"), TextTrimming = TextTrimming.CharacterEllipsis };
            var labels = new StackPanel { Spacing = 3, HorizontalAlignment = HorizontalAlignment.Stretch };
            labels.Children.Add(heading);
            labels.Children.Add(detail);
            var card = new Button { Content = labels, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(12), CornerRadius = new CornerRadius(8),
                Background = artwork.Id == _selected?.Id ? Ink("#2A6B69") : Ink("#223040") };
            card.Click += (_, _) => SelectArtwork(artwork);
            _catalogItems.Children.Add(card);
        }
        if (_catalogItems.Children.Count == 0)
            _catalogItems.Children.Add(new TextBlock { Text = "Nessun risultato", Foreground = Ink("#9EAEC0") });
    }

    private void UpdateFavoriteButton()
    {
        _favoriteToggle.Content = _selected is not null && _preferences.IsFavorite(_selected.Id)
            ? "★ Rimuovi dai preferiti" : "☆ Aggiungi ai preferiti";
        _favoriteToggle.HorizontalAlignment = HorizontalAlignment.Left;
    }

    private void PopulateRecentRecipes()
    {
        _recentItems.Children.Clear();
        foreach (string path in _preferences.RecentRecipePaths)
        {
            string captured = path;
            var entry = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*"),
                HorizontalAlignment = HorizontalAlignment.Stretch };
            var filename = new TextBlock { Text = Path.GetFileName(path),
                TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.NoWrap,
                Margin = new Thickness(0, 0, 28, 0) };
            var open = new Button { Content = filename, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(9, 7),
                Background = Ink("#223040") };
            ToolTip.SetTip(open, captured);
            open.Click += async (_, _) => await OpenRecipeFromPathAsync(captured);
            entry.Children.Add(open);
            var forget = new Button { Content = "×", Padding = new Thickness(7, 5),
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0), Background = Ink("#283E50") };
            ToolTip.SetTip(forget, "Rimuovi dall'elenco, senza eliminare il file");
            forget.Click += async (_, _) =>
            {
                _preferences = _preferences.RemoveRecentRecipe(captured);
                PopulateRecentRecipes();
                await PersistPreferencesAsync();
            };
            // Il pulsante Apri occupa tutta la larghezza; Rimuovi e sovrapposto a destra.
            entry.Children.Add(forget);
            _recentItems.Children.Add(entry);
        }
        if (_recentItems.Children.Count == 0)
            _recentItems.Children.Add(new TextBlock { Text = "Nessuna ricetta recente",
                FontSize = 11, Foreground = Ink("#9EAEC0") });
    }

    private async Task PersistPreferencesAsync()
    {
        await _preferencesGate.WaitAsync();
        try
        {
            await _preferencesFile.SaveAsync(_preferences);
            if (!_closed) _preferencesWarning.IsVisible = false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            if (!_closed)
            {
                _preferencesWarning.Text = "Preferenze locali non salvate: " + ex.Message;
                _preferencesWarning.IsVisible = true;
                SetStatus("Impossibile salvare le preferenze locali: " + ex.Message, error: true);
            }
        }
        finally { _preferencesGate.Release(); }
    }

    private async Task RecordRecentAsync(string localPath)
    {
        _preferences = _preferences.AddRecentRecipe(localPath);
        PopulateRecentRecipes();
        await PersistPreferencesAsync();
    }

}
