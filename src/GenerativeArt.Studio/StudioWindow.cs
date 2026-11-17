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

/// <summary>
/// Primo Studio code-only. I render avvengono fuori dal thread UI e sono serializzati;
/// ogni modifica invalida le richieste precedenti. Nessun frame viene memorizzato
/// nella timeline: è generato, visualizzato e rilasciato a ogni avanzamento.
/// </summary>
public sealed partial class StudioWindow : Window
{
    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();
    private readonly IArtworkRasterRenderer _renderer;
    private readonly StudioRenderCoordinator _renderCoordinator;
    private readonly DispatcherTimer _playTimer = new();
    private readonly StackPanel _catalogItems = new() { Spacing = 7 };
    private readonly StackPanel _recentItems = new() { Spacing = 6 };
    private readonly StackPanel _parameterItems = new() { Spacing = 12 };
    private readonly TextBox _search = new() { PlaceholderText = "Cerca opera o categoria..." };
    private readonly CheckBox _favoritesOnly = new() { Content = "Solo preferiti" };
    private readonly Button _favoriteToggle = new() { Padding = new Thickness(9, 5) };
    private readonly Button _themeToggle = new() { Padding = new Thickness(12, 8) };
    private readonly TextBlock _preferencesWarning = new() { IsVisible = false, FontSize = 11,
        TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _seed = new() { Text = "42" };
    private readonly TextBox _width = new() { Text = "1920" };
    private readonly TextBox _height = new() { Text = "1080" };
    // Il riquadro rimane regolabile, ma il raster è sempre quello dell'export.
    private readonly TextBox _previewAreaWidth = new() { Text = "1200" };
    private readonly TextBox _previewAreaHeight = new() { Text = "900" };
    private readonly TextBox _frames = new() { Text = "16" };
    private readonly TextBox _fps = new() { Text = "18" };
    private readonly TextBlock _status = new() { Text = "Pronto", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _activeTitle = new() { FontSize = 24, FontWeight = FontWeight.SemiBold };
    private readonly TextBlock _activeDescription = new() { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.LightGray };
    private readonly TextBlock _phaseLabel = new() { Text = "Frame 1 / 16" };
    private readonly TextBlock _zoomLabel = new() { Text = "75%", Foreground = Brushes.LightGray };
    private readonly PreviewViewport _preview = new();
    private readonly Slider _phase = new() { Minimum = 0, Maximum = 15, Value = 0, TickFrequency = 1 };
    private readonly Slider _zoom = new()
    {
        Minimum = StudioViewportState.MinimumZoom,
        Maximum = StudioViewportState.MaximumZoom, Value = 0.75
    };
    private readonly Button _play = new() { Content = "▶ Riproduci" };
    private readonly Button _export;
    private readonly Button _cancelExport;
    private readonly ProgressBar _exportProgress = new()
    {
        IsIndeterminate = true, IsVisible = false, Width = 104, Height = 5,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly StackPanel _animationPanel = new() { Spacing = 10 };
    private readonly Dictionary<string, string> _parameters = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Action> _swatchRefresh = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SolidColorBrush> _inkBrushes = new(StringComparer.Ordinal);
    private readonly StudioPreferencesFile _preferencesFile = new(StudioPreferencesFile.DefaultPath);
    private readonly SemaphoreSlim _preferencesGate = new(1, 1);
    private StudioPreferences _preferences = new();
    private bool _lightTheme;
    private readonly HashSet<string> _invalidNumericInputs = new(StringComparer.Ordinal);
    private ArtworkDescriptor? _selected;
    private CancellationTokenSource? _previewCancellation;
    private Bitmap? _bitmap;
    private int _currentFrame;
    private long _revision;
    private bool _loadingFields;
    private long _parameterEditorRevision;
    private bool _playbackFrameBusy;
    private CancellationTokenSource? _exportCancellation;
    private bool _exportRunning;
    private bool _previewDeferred;
    private bool _closed;
    private bool _updatingZoom;
    private Border? _previewStage;

    public StudioWindow()
    {
        string? preferencesError = null;
        try { _preferences = _preferencesFile.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            preferencesError = ex.Message;
        }
        _lightTheme = _preferences.UseLightTheme;
        if (global::Avalonia.Application.Current is { } app)
            app.RequestedThemeVariant = _lightTheme ? ThemeVariant.Light : ThemeVariant.Dark;
        _activeDescription.Foreground = Ink("#9EAEC0");
        _zoomLabel.Foreground = Ink("#9EAEC0");
        _preferencesWarning.Foreground = Ink("#FF9A9A");
        if (preferencesError is not null)
        {
            _preferencesWarning.Text = "Preferenze locali non caricate: " + preferencesError;
            _preferencesWarning.IsVisible = true;
        }
        _renderer = new ArtworkRenderEngine(_catalog);
        _renderCoordinator = new StudioRenderCoordinator(_renderer);
        _export = ActionButton("Esporta PNG", ExportPngAsync);
        _cancelExport = ActionButton("Annulla export", () =>
        {
            _exportCancellation?.Cancel();
            return Task.CompletedTask;
        });
        _cancelExport.IsEnabled = false;
        Title = "GenerativeArt Studio · M4E.1-R2";
        Width = 1510;
        Height = 850;
        MinWidth = 1120;
        MinHeight = 650;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        // Avvio massimizzato (non modalità kiosk): cornice e barra applicazioni conservate.
        WindowState = Avalonia.Controls.WindowState.Maximized;
        Background = Ink("#10151E");

        _playTimer.Interval = TimeSpan.FromMilliseconds(1000d / 18d);
        _playTimer.Tick += (_, _) => AdvanceFrame();
        _search.TextChanged += (_, _) => PopulateCatalog();
        _favoritesOnly.IsCheckedChanged += (_, _) => PopulateCatalog();
        _favoriteToggle.Click += async (_, _) =>
        {
            if (_selected is null) return;
            _preferences = _preferences.ToggleFavorite(_selected.Id);
            UpdateFavoriteButton();
            PopulateCatalog();
            await PersistPreferencesAsync();
        };
        _themeToggle.Click += async (_, _) =>
        {
            _preferences = _preferences.WithTheme(!_preferences.UseLightTheme);
            ApplyTheme(_preferences.UseLightTheme);
            await PersistPreferencesAsync();
        };
        foreach (TextBox box in new[] { _seed, _width, _height, _frames, _fps })
            box.TextChanged += (_, _) => OnRecipeChanged();
        foreach (TextBox box in new[] { _previewAreaWidth, _previewAreaHeight })
            box.TextChanged += (_, _) => UpdatePreviewArea();
        _phase.ValueChanged += (_, _) =>
        {
            if (_loadingFields) return;
            _currentFrame = (int)Math.Round(_phase.Value);
            UpdateFrameLabel();
            SchedulePreview();
        };
        _zoom.ValueChanged += (_, _) => { if (!_updatingZoom) ApplyZoom(); };
        _preview.ZoomChanged += zoom =>
        {
            // Rotella e slider condividono un'unica scala; non si genera un nuovo render.
            _updatingZoom = true;
            try { _zoom.Value = zoom; _zoomLabel.Text = $"{zoom * 100:0.#}%"; }
            finally { _updatingZoom = false; }
        };
        _play.Click += (_, _) => TogglePlayback();
        Closed += (_, _) =>
        {
            _closed = true;
            StopPlayback();
            _previewCancellation?.Cancel();
            _exportCancellation?.Cancel();
            _preview.SetBitmap(null);
            _bitmap?.Dispose();
        };

        Content = BuildLayout();
        ApplyTheme(_lightTheme);
        PopulateCatalog();
        PopulateRecentRecipes();
        SelectArtwork(_catalog.Get("plasma-noise"));
    }

}
