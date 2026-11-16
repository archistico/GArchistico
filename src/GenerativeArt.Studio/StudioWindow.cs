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
public sealed class StudioWindow : Window
{
    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();
    private readonly IArtworkRasterRenderer _renderer;
    private readonly SemaphoreSlim _renderGate = new(1, 1);
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
    // Indipendenti dalle dimensioni esportate: un'anteprima può essere
    // leggera oppure a risoluzione piena, sempre adattabile al riquadro.
    private readonly TextBox _previewRasterWidth = new() { Text = "800" };
    private readonly TextBox _previewRasterHeight = new() { Text = "600" };
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
        _export = ActionButton("Esporta PNG", ExportPngAsync);
        _cancelExport = ActionButton("Annulla export", () =>
        {
            _exportCancellation?.Cancel();
            return Task.CompletedTask;
        });
        _cancelExport.IsEnabled = false;
        Title = "GenerativeArt Studio · M4D.2-R1";
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
        foreach (TextBox box in new[] { _previewRasterWidth, _previewRasterHeight })
            box.TextChanged += (_, _) => SchedulePreview();
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

    private Control BuildLayout()
    {
        var open = ActionButton("Apri ricetta", async () => await OpenRecipeAsync());
        var save = ActionButton("Salva ricetta", async () => await SaveRecipeAsync());
        var regenerate = ActionButton("Rigenera", () => { SchedulePreview(); return Task.CompletedTask; });
        var seedNew = ActionButton("Nuovo seed", () =>
        {
            _seed.Text = ((ulong)Random.Shared.NextInt64(1, long.MaxValue)).ToString(CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });
        var header = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
            Background = Ink("#192330"), Margin = new Thickness(0, 0, 0, 1) };
        var branding = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(22, 17) };
        branding.Children.Add(new TextBlock { Text = "◉", FontSize = 24, Foreground = Ink("#7DD9C2") });
        branding.Children.Add(new TextBlock { Text = "GENERATIVEART", FontSize = 20,
            FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center });
        branding.Children.Add(new TextBlock { Text = "STUDIO  /  LABORATORIO", FontSize = 11,
            Foreground = Ink("#97A9BB"), VerticalAlignment = VerticalAlignment.Center });
        header.Children.Add(branding);
        var headerTools = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9,
            Margin = new Thickness(12), VerticalAlignment = VerticalAlignment.Center };
        headerTools.Children.Add(open);
        headerTools.Children.Add(save);
        headerTools.Children.Add(_export);
        headerTools.Children.Add(_cancelExport);
        Grid.SetColumn(headerTools, 1);
        header.Children.Add(headerTools);

        var left = new StackPanel { Spacing = 14, Margin = new Thickness(16) };
        left.Children.Add(SectionLabel($"CATALOGO  /  {_catalog.All.Count:D2} OPERE"));
        left.Children.Add(_search);
        left.Children.Add(_favoritesOnly);
        left.Children.Add(new TextBlock { Text = "Seleziona un generatore per iniziare.",
            TextWrapping = TextWrapping.Wrap, Foreground = Ink("#9EAEC0"), FontSize = 12 });
        left.Children.Add(_catalogItems);
        left.Children.Add(SectionLabel("RICETTE RECENTI  /  LOCALI"));
        left.Children.Add(new TextBlock { Text = "Solo file JSON locali aperti o salvati dallo Studio.",
            FontSize = 11, TextWrapping = TextWrapping.Wrap, Foreground = Ink("#9EAEC0") });
        left.Children.Add(_recentItems);

        var titleArea = new StackPanel { Spacing = 7, Margin = new Thickness(0, 0, 0, 10) };
        titleArea.Children.Add(_activeTitle);
        titleArea.Children.Add(_favoriteToggle);
        titleArea.Children.Add(_activeDescription);

        var previewHeader = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), Margin = new Thickness(3, 0, 3, 10) };
        previewHeader.Children.Add(SectionLabel("ANTEPRIMA  /  RASTER"));
        var zoomRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        zoomRow.Children.Add(new TextBlock { Text = "Zoom", VerticalAlignment = VerticalAlignment.Center,
            Foreground = Ink("#9EAEC0") });
        _zoom.Width = 110;
        zoomRow.Children.Add(_zoom);
        zoomRow.Children.Add(_zoomLabel);
        var fitButton = new Button { Content = "Adatta", Padding = new Thickness(8, 5) };
        fitButton.Click += (_, _) => _preview.FitImage();
        zoomRow.Children.Add(fitButton);
        var oneToOneButton = new Button { Content = "100%", Padding = new Thickness(8, 5) };
        oneToOneButton.Click += (_, _) => _preview.SetZoom(1);
        zoomRow.Children.Add(oneToOneButton);
        var centerButton = new Button { Content = "Centra", Padding = new Thickness(8, 5) };
        centerButton.Click += (_, _) => _preview.CenterImage();
        zoomRow.Children.Add(centerButton);
        Grid.SetColumn(zoomRow, 1);
        previewHeader.Children.Add(zoomRow);

        var previewStage = new Border { Background = Ink("#0A0E16"), CornerRadius = new CornerRadius(14),
            BorderBrush = Ink("#29394C"), BorderThickness = new Thickness(1), ClipToBounds = true,
            MaxWidth = 1200, MaxHeight = 900 };
        _previewStage = previewStage;
        previewStage.Child = _preview;
        var previewColumn = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,Auto,*,Auto"),
            Margin = new Thickness(18, 16, 18, 18) };
        previewColumn.Children.Add(titleArea);
        Grid.SetRow(previewHeader, 1);
        previewColumn.Children.Add(previewHeader);
        Grid.SetRow(previewStage, 2);
        previewColumn.Children.Add(previewStage);

        _animationPanel.Children.Add(SectionLabel("TIMELINE  /  LOOP"));
        var player = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9 };
        player.Children.Add(_play);
        player.Children.Add(new Button { Content = "↺ Primo frame" });
        // Il secondo pulsante viene collegato esplicitamente, senza binding nascosti.
        if (player.Children[1] is Button firstFrame)
            firstFrame.Click += (_, _) => { StopPlayback(); _phase.Value = 0; };
        player.Children.Add(_phaseLabel);
        _phase.Width = 270;
        _animationPanel.Children.Add(player);
        _animationPanel.Children.Add(_phase);
        Grid.SetRow(_animationPanel, 3);
        _animationPanel.Margin = new Thickness(0, 12, 0, 0);
        previewColumn.Children.Add(_animationPanel);

        var right = new StackPanel { Spacing = 14, Margin = new Thickness(16) };
        right.Children.Add(SectionLabel("ASPETTO  /  STUDIO"));
        right.Children.Add(_themeToggle);
        right.Children.Add(_preferencesWarning);
        right.Children.Add(SectionLabel("PARAMETRI  /  RICETTA"));
        right.Children.Add(Field("SEED  ·  UInt64", _seed));
        right.Children.Add(seedNew);
        var sizes = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,*"), ColumnSpacing = 10 };
        sizes.Children.Add(Field("LARGHEZZA  PX", _width));
        var heightGroup = Field("ALTEZZA  PX", _height);
        Grid.SetColumn(heightGroup, 1);
        sizes.Children.Add(heightGroup);
        right.Children.Add(sizes);
        right.Children.Add(SectionLabel("ANTEPRIMA  /  DIMENSIONI INDIPENDENTI"));
        right.Children.Add(new TextBlock
        {
            Text = "L'output mantiene le dimensioni sopra. L'anteprima usa un raster ridotto o pieno, " +
                "mentre il riquadro si adatta allo spazio disponibile. Adatta mostra l'intera opera.",
            FontSize = 11, TextWrapping = TextWrapping.Wrap, Foreground = Ink("#9EAEC0")
        });
        var previewRasterSizes = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,*"), ColumnSpacing = 10 };
        previewRasterSizes.Children.Add(Field("RASTER MAX  LARGHEZZA PX", _previewRasterWidth));
        var rasterHeightGroup = Field("RASTER MAX  ALTEZZA PX", _previewRasterHeight);
        Grid.SetColumn(rasterHeightGroup, 1);
        previewRasterSizes.Children.Add(rasterHeightGroup);
        right.Children.Add(previewRasterSizes);
        var previewAreaSizes = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,*"), ColumnSpacing = 10 };
        previewAreaSizes.Children.Add(Field("RIQUADRO MAX  LARGHEZZA DIP", _previewAreaWidth));
        var areaHeightGroup = Field("RIQUADRO MAX  ALTEZZA DIP", _previewAreaHeight);
        Grid.SetColumn(areaHeightGroup, 1);
        previewAreaSizes.Children.Add(areaHeightGroup);
        right.Children.Add(previewAreaSizes);
        right.Children.Add(ActionButton("Raster = output", () =>
        {
            try
            {
                ArtworkRecipe recipe = BuildRecipe();
                _previewRasterWidth.Text = recipe.Size.Width.ToString(CultureInfo.InvariantCulture);
                _previewRasterHeight.Text = recipe.Size.Height.ToString(CultureInfo.InvariantCulture);
                _preview.FitImage();
            }
            catch (Exception ex) { SetStatus("Anteprima: " + ex.Message, error: true); }
            return Task.CompletedTask;
        }));
        right.Children.Add(SectionLabel("CONTROLLO ARTISTICO  /  TUTTI I PARAMETRI"));
        right.Children.Add(new TextBlock { Text = "Auto mantiene la palette originale. Scegli… apre HSV/RGB/Alpha; modifica un solo colore senza cambiare gli altri.",
            FontSize = 11, TextWrapping = TextWrapping.Wrap, Foreground = Ink("#9EAEC0") });
        right.Children.Add(_parameterItems);
        right.Children.Add(SectionLabel("ANIMAZIONE"));
        right.Children.Add(Field("FRAME  ·  2..120", _frames));
        right.Children.Add(Field("FPS  ·  1..120", _fps));
        right.Children.Add(regenerate);

        var columns = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("260,*,420") };
        var leftScroll = new ScrollViewer { Content = left,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
        columns.Children.Add(Panel(leftScroll, "#151F2B"));
        var middle = Panel(previewColumn, "#10151E");
        Grid.SetColumn(middle, 1);
        columns.Children.Add(middle);
        var rightScroll = new ScrollViewer { Content = right,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
        var rightBorder = Panel(rightScroll, "#151F2B");
        Grid.SetColumn(rightBorder, 2);
        columns.Children.Add(rightBorder);

        var footer = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
            Background = Ink("#182431"), Margin = new Thickness(0, 1, 0, 0) };
        _status.Margin = new Thickness(18, 11);
        _status.Foreground = Ink("#A9E6D7");
        footer.Children.Add(_status);
        var footerBrand = new TextBlock { Text = "C#  ·  .NET 10  ·  AVALONIA  ·  SKIA", Foreground = Ink("#8196AB"),
            Margin = new Thickness(18, 11), FontSize = 11 };
        var footerTools = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        footerTools.Children.Add(_exportProgress);
        footerTools.Children.Add(footerBrand);
        Grid.SetColumn(footerTools, 1);
        footer.Children.Add(footerTools);

        var root = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,*,Auto") };
        root.Children.Add(header);
        Grid.SetRow(columns, 1);
        root.Children.Add(columns);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);
        return root;
    }

    // Ogni token possiede un pennello stabile: il cambio tema aggiorna quelli
    // già assegnati ai controlli, senza ricreare la UI o perdere focus, pan e zoom.
    private SolidColorBrush Ink(string hex)
    {
        if (!_inkBrushes.TryGetValue(hex, out SolidColorBrush? brush))
        {
            brush = new SolidColorBrush(Color.Parse(ThemeColor(hex)));
            _inkBrushes.Add(hex, brush);
        }
        return brush;
    }

    private string ThemeColor(string darkHex) => StudioThemePalette.Resolve(darkHex, _lightTheme);

    private void ApplyTheme(bool light)
    {
        _lightTheme = light;
        if (global::Avalonia.Application.Current is { } app)
            app.RequestedThemeVariant = light ? ThemeVariant.Light : ThemeVariant.Dark;
        foreach ((string key, SolidColorBrush brush) in _inkBrushes)
            brush.Color = Color.Parse(ThemeColor(key));
        _themeToggle.Content = light ? "☾ Tema scuro" : "☀ Tema chiaro";
    }

    private Border Panel(Control control, string color) => new()
    {
        Child = control,
        Background = Ink(color),
        BorderBrush = Ink("#263546"),
        BorderThickness = new Thickness(0, 0, 1, 0)
    };
    private TextBlock SectionLabel(string title) => new()
    {
        Text = title, FontSize = 11, FontWeight = FontWeight.Bold, LetterSpacing = 1.1,
        Foreground = Ink("#7DD9C2")
    };
    private StackPanel Field(string caption, Control editor)
    {
        var panel = new StackPanel { Spacing = 5 };
        panel.Children.Add(new TextBlock { Text = caption, FontSize = 11,
            Foreground = Ink("#9EAEC0") });
        panel.Children.Add(editor);
        return panel;
    }
    private Button ActionButton(string title, Func<Task> action)
    {
        var button = new Button { Content = title, Padding = new Thickness(12, 8),
            Background = Ink("#283E50"), CornerRadius = new CornerRadius(7) };
        button.Click += async (_, _) => await action();
        return button;
    }

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
                FontSize = 14, FontWeight = FontWeight.SemiBold };
            var detail = new TextBlock { Text = artwork.Category +
                (artwork.SupportsSeamlessLoop ? "  ·  LOOP" : "  ·  STATIC"), FontSize = 11,
                Foreground = Ink("#A8BDCE") };
            var labels = new StackPanel { Spacing = 3 };
            labels.Children.Add(heading);
            labels.Children.Add(detail);
            var card = new Button { Content = labels, HorizontalContentAlignment = HorizontalAlignment.Left,
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
            var entry = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), ColumnSpacing = 4 };
            var open = new Button { Content = Path.GetFileName(path),
                HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(7, 5),
                Background = Ink("#223040") };
            ToolTip.SetTip(open, captured);
            open.Click += async (_, _) => await OpenRecipeFromPathAsync(captured);
            entry.Children.Add(open);
            var forget = new Button { Content = "×", Padding = new Thickness(7, 5),
                Background = Ink("#283E50") };
            ToolTip.SetTip(forget, "Rimuovi dall'elenco, senza eliminare il file");
            forget.Click += async (_, _) =>
            {
                _preferences = _preferences.RemoveRecentRecipe(captured);
                PopulateRecentRecipes();
                await PersistPreferencesAsync();
            };
            Grid.SetColumn(forget, 1);
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

    private void SelectArtwork(ArtworkDescriptor descriptor, ArtworkRecipe? restore = null)
    {
        StopPlayback();
        _loadingFields = true;
        try
        {
            _selected = descriptor;
            _parameters.Clear();
            _invalidNumericInputs.Clear();
            foreach (ArtworkParameterSpec item in descriptor.Parameters)
                _parameters.Add(item.Id, restore is null ? item.DefaultValue : restore.Parameters[item.Id]);
            if (restore is null)
                foreach (var pair in descriptor.ValidateParameters(StudioRecipeDefaults.ParametersFor(descriptor.Id)))
                    _parameters[pair.Key] = pair.Value;
            _seed.Text = (restore?.Seed ?? 42UL).ToString(CultureInfo.InvariantCulture);
            _width.Text = (restore?.Size.Width ?? StudioRecipeDefaults.Width).ToString(CultureInfo.InvariantCulture);
            _height.Text = (restore?.Size.Height ?? StudioRecipeDefaults.Height).ToString(CultureInfo.InvariantCulture);
            _frames.Text = (restore?.Animation?.FrameCount ?? StudioRecipeDefaults.FrameCount).ToString(CultureInfo.InvariantCulture);
            _fps.Text = (restore?.Animation?.FramesPerSecond ?? StudioRecipeDefaults.FramesPerSecond).ToString(CultureInfo.InvariantCulture);
            _animationPanel.IsVisible = descriptor.SupportsAnimation;
            _frames.IsEnabled = descriptor.SupportsAnimation;
            _fps.IsEnabled = descriptor.SupportsAnimation;
            _activeTitle.Text = descriptor.Title;
            _activeDescription.Text = descriptor.Description;
            UpdateFavoriteButton();
            _currentFrame = 0;
            _phase.Value = 0;
            PopulateParameters();
            UpdateFrameLimits();
            PopulateCatalog();
        }
        finally { _loadingFields = false; }
        _preview.CenterImage();
        SchedulePreview();
    }

    private void PopulateParameters()
    {
        long editorRevision = ++_parameterEditorRevision;
        // TextChanged può arrivare dal dispatcher dopo la sostituzione del pannello.
        bool IsCurrentEditor() => !_loadingFields && editorRevision == _parameterEditorRevision;
        _parameterItems.Children.Clear();
        _swatchRefresh.Clear();
        string? currentGroup = null;
        foreach (ArtworkParameterSpec spec in _selected!.Parameters)
        {
            if (currentGroup != spec.Group)
            {
                currentGroup = spec.Group;
                _parameterItems.Children.Add(SectionLabel(spec.Group.ToUpperInvariant()));
            }
            switch (spec.Kind)
            {
                case ParameterKind.Choice:
                {
                    var choice = new ComboBox { ItemsSource = spec.Choices, SelectedItem = _parameters[spec.Id],
                        HorizontalAlignment = HorizontalAlignment.Stretch };
                    choice.SelectionChanged += (_, _) =>
                    {
                        if (!IsCurrentEditor() || choice.SelectedItem is not string value) return;
                        _parameters[spec.Id] = value;
                        if (spec.Id == "palette")
                            foreach (Action refresh in _swatchRefresh.Values) refresh();
                        OnRecipeChanged();
                    };
                    _parameterItems.Children.Add(Field(spec.DisplayName, choice));
                    break;
                }
                case ParameterKind.Boolean:
                {
                    var checkbox = new CheckBox { Content = spec.DisplayName,
                        IsChecked = bool.Parse(_parameters[spec.Id]) };
                    checkbox.IsCheckedChanged += (_, _) =>
                    {
                        if (!IsCurrentEditor()) return;
                        _parameters[spec.Id] = checkbox.IsChecked == true ? "true" : "false";
                        OnRecipeChanged();
                    };
                    _parameterItems.Children.Add(checkbox);
                    break;
                }
                case ParameterKind.Integer:
                case ParameterKind.Number:
                {
                    double numericStep = StudioParameterInput.NumericStep(spec);
                    int digits = Math.Clamp(-(int)Math.Floor(Math.Log10(numericStep)), 0, 12);
                    var slider = new Slider
                    {
                        Minimum = double.Parse(spec.Minimum!, CultureInfo.InvariantCulture),
                        Maximum = double.Parse(spec.Maximum!, CultureInfo.InvariantCulture),
                        Value = double.Parse(_parameters[spec.Id], CultureInfo.InvariantCulture),
                        TickFrequency = numericStep, SmallChange = numericStep, LargeChange = numericStep * 10,
                    };
                    var exact = new TextBox { Text = _parameters[spec.Id], Width = 112,
                        HorizontalAlignment = HorizontalAlignment.Right };
                    ToolTip.SetTip(slider, $"Intervallo {spec.Minimum} … {spec.Maximum}. " +
                        $"Passo frecce: {numericStep.ToString("G", CultureInfo.InvariantCulture)}. " +
                        "Il campo a destra accetta valori esatti.");
                    bool syncing = false;
                    slider.ValueChanged += (_, _) =>
                    {
                        if (!IsCurrentEditor() || syncing) return;
                        string candidate = spec.Kind == ParameterKind.Integer
                            ? ((int)Math.Round(slider.Value)).ToString(CultureInfo.InvariantCulture)
                            : Math.Round(slider.Value, digits).ToString("R", CultureInfo.InvariantCulture);
                        string canonical = spec.ValidateValue(candidate);
                        syncing = true;
                        try
                        {
                            _parameters[spec.Id] = canonical;
                            exact.Text = canonical;
                            exact.Foreground = Ink("#D7E5EE");
                        }
                        finally { syncing = false; }
                        OnRecipeChanged();
                    };
                    exact.TextChanged += (_, _) =>
                    {
                        if (!IsCurrentEditor() || syncing) return;
                        if (!StudioParameterInput.TryNormalize(spec, exact.Text, out string canonical))
                        {
                            // Non pubblicare valori intermedi o non validi nella ricetta.
                            _invalidNumericInputs.Add(spec.Id);
                            exact.Foreground = Ink("#FF9A9A");
                            return;
                        }
                        _invalidNumericInputs.Remove(spec.Id);
                        exact.Foreground = Ink("#D7E5EE");
                        if (canonical == _parameters[spec.Id]) return;
                        syncing = true;
                        try
                        {
                            _parameters[spec.Id] = canonical;
                            slider.Value = double.Parse(canonical, CultureInfo.InvariantCulture);
                        }
                        finally { syncing = false; }
                        OnRecipeChanged();
                    };
                    exact.LostFocus += (_, _) =>
                    {
                        if (!IsCurrentEditor() || syncing) return;
                        // Alla perdita del focus ripristina l'ultimo valore valido.
                        // Le ricette non contengono mai input parziali.
                        syncing = true;
                        try
                        {
                            exact.Text = _parameters[spec.Id];
                            _invalidNumericInputs.Remove(spec.Id);
                            exact.Foreground = Ink("#D7E5EE");
                        }
                        finally { syncing = false; }
                    };
                    var group = new StackPanel { Spacing = 4 };
                    group.Children.Add(new TextBlock { Text = spec.DisplayName, Foreground = Ink("#D7E5EE") });
                    var editRow = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
                        ColumnSpacing = 8 };
                    editRow.Children.Add(slider);
                    Grid.SetColumn(exact, 1);
                    editRow.Children.Add(exact);
                    group.Children.Add(editRow);
                    _parameterItems.Children.Add(group);
                    break;
                }
                case ParameterKind.Color:
                {
                    var color = new TextBox { Text = _parameters[spec.Id], MinWidth = 115 };
                    var swatch = new Border { Width = 31, Height = 30,
                        BorderBrush = Ink("#9EAEC0"), BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(5) };
                    var autoButton = new Button { Content = "Auto", Padding = new Thickness(8, 4),
                        IsVisible = spec.AllowAutoColor };
                    string ResolveVisibleColor(string value)
                    {
                        if (value != "auto") return value;
                        return ArtworkRolePalettePreview.SuggestHex(
                            _selected!.Id, _parameters.GetValueOrDefault("palette", "default"), spec.Id);
                    }
                    void UpdateSwatch(string value)
                    {
                        swatch.Background = new SolidColorBrush(Color.Parse(ResolveVisibleColor(value)));
                    }
                    UpdateSwatch(_parameters[spec.Id]);
                    _swatchRefresh[spec.Id] = () => UpdateSwatch(_parameters[spec.Id]);
                    color.TextChanged += (_, _) =>
                    {
                        if (!IsCurrentEditor()) return;
                        string candidate;
                        try { candidate = spec.ValidateValue(color.Text); }
                        catch (ArgumentException)
                        {
                            _invalidNumericInputs.Add(spec.Id);
                            color.Foreground = Ink("#FF9A9A");
                            return;
                        }
                        _invalidNumericInputs.Remove(spec.Id);
                        color.Foreground = Ink("#D7E5EE");
                        UpdateSwatch(candidate);
                        if (_parameters[spec.Id] == candidate) return;
                        _parameters[spec.Id] = candidate;
                        OnRecipeChanged();
                    };
                    color.LostFocus += (_, _) =>
                    {
                        if (!IsCurrentEditor()) return;
                        if (_invalidNumericInputs.Contains(spec.Id))
                            color.Text = _parameters[spec.Id];
                    };
                    autoButton.Click += (_, _) => { if (IsCurrentEditor()) color.Text = "auto"; };
                    var pickButton = new Button { Content = "Scegli…", Padding = new Thickness(8, 4) };
                    pickButton.Click += async (_, _) =>
                    {
                        if (!IsCurrentEditor()) return;
                        try
                        {
                            string initial = ResolveVisibleColor(_parameters[spec.Id]);
                            var dialog = new StudioColorPickerWindow(spec.DisplayName, initial);
                            if (await dialog.ShowDialog<bool>(this) && IsCurrentEditor() && dialog.SelectedHex is { } selected)
                                color.Text = selected;
                        }
                        catch (Exception ex) { SetStatus("Editor colore: " + ex.Message, error: true); }
                    };
                    var colorRow = new StackPanel { Orientation = Orientation.Horizontal,
                        Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
                    colorRow.Children.Add(swatch);
                    colorRow.Children.Add(color);
                    colorRow.Children.Add(pickButton);
                    colorRow.Children.Add(autoButton);
                    _parameterItems.Children.Add(Field(spec.DisplayName + " · #RRGGBB", colorRow));
                    break;
                }
            }
        }
    }

    private void OnRecipeChanged()
    {
        if (_loadingFields || _selected is null) return;
        StopPlayback();
        UpdateFrameLimits();
        SchedulePreview();
    }

    private void UpdateFrameLimits()
    {
        int count = int.TryParse(_frames.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed)
            && parsed is >= 2 and <= 120 ? parsed : StudioRecipeDefaults.FrameCount;
        _phase.Maximum = count - 1;
        if (_currentFrame >= count) _currentFrame = count - 1;
        _currentFrame = Math.Max(_currentFrame, 0);
        _phase.Value = _currentFrame;
        UpdateFrameLabel();
    }

    private void UpdateFrameLabel() => _phaseLabel.Text =
        $"Frame {_currentFrame + 1} / {Math.Max(2, (int)_phase.Maximum + 1)}";

    private ArtworkRecipe BuildRecipe()
    {
        ArtworkDescriptor descriptor = _selected ?? throw new InvalidOperationException("Nessuna opera selezionata.");
        if (_invalidNumericInputs.Count > 0)
            throw new ArgumentException("Correggere i valori evidenziati in rosso prima di salvare o esportare.");
        if (!ulong.TryParse(_seed.Text, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed))
            throw new ArgumentException("Seed non valido: inserire un UInt64 decimale da 0 a 18446744073709551615.");
        if (!int.TryParse(_width.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int width) ||
            !int.TryParse(_height.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int height))
            throw new ArgumentException("Larghezza e altezza devono essere interi positivi.");
        RecipeAnimation? animation = null;
        if (descriptor.SupportsAnimation)
        {
            if (!int.TryParse(_frames.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int frames) ||
                !int.TryParse(_fps.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int fps))
                throw new ArgumentException("Frame e FPS devono essere interi.");
            animation = new RecipeAnimation(frames, fps);
        }
        return ArtworkRecipe.Create(descriptor, new CanvasSize(width, height), seed, _parameters, animation);
    }

    private static int ParsePreviewDimension(TextBox field, string label, int minimum, int maximum)
    {
        if (!int.TryParse(field.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) ||
            value < minimum || value > maximum)
            throw new ArgumentException($"{label}: inserire un intero fra {minimum} e {maximum}.");
        return value;
    }

    private ArtworkRecipe BuildPreviewRecipe(ArtworkRecipe original)
    {
        int width = ParsePreviewDimension(_previewRasterWidth, "Larghezza raster anteprima", 64, 4096);
        int height = ParsePreviewDimension(_previewRasterHeight, "Altezza raster anteprima", 64, 4096);
        return StudioPreviewPolicy.CreatePreview(original, _catalog, width, height, (long)width * height);
    }

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
        if (_exportCancellation is not null)
        {
            // La preview non deve accodarsi dietro un export potenzialmente lungo.
            // Riprendiamo dall'ultima ricetta solo se è stata modificata.
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
            await _renderGate.WaitAsync(ct);
            byte[] png;
            try
            {
                png = await Task.Run(() =>
                {
                    using var stream = new MemoryStream();
                    _renderer.Render(previewRecipe, stream, frameIndex, ct);
                    return stream.ToArray();
                }, ct);
            }
            finally { _renderGate.Release(); }
            if (_closed || ct.IsCancellationRequested || revision != _revision) return;
            using var data = new MemoryStream(png, writable: false);
            Bitmap replacement = new(data);
            Bitmap? previous = _bitmap;
            _bitmap = replacement;
            _preview.SetBitmap(replacement);
            previous?.Dispose();
            SetStatus($"Anteprima: {previewRecipe.Size.Width} × {previewRecipe.Size.Height} " +
                $"· output: {original.Size.Width} × {original.Size.Height}  ·  seed {previewRecipe.Seed}" +
                (frameIndex is null ? "" : $"  ·  frame {frameIndex + 1}"));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!_closed && !ct.IsCancellationRequested && revision == _revision)
                SetStatus("Errore anteprima: " + ex.Message, error: true);
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

    private void TogglePlayback()
    {
        if (_playTimer.IsEnabled) { StopPlayback(); return; }
        try
        {
            ArtworkRecipe recipe = BuildRecipe();
            if (recipe.Animation is null) throw new ArgumentException("Questa opera non è animata.");
            _renderer.Validate(BuildPreviewRecipe(recipe), _currentFrame);
            _playTimer.Interval = TimeSpan.FromSeconds(1d / recipe.Animation.FramesPerSecond);
            _playTimer.Start();
            _play.Content = "❚❚ Pausa";
        }
        catch (Exception ex) { SetStatus(ex.Message, error: true); }
    }

    private void AdvanceFrame()
    {
        if (_selected?.SupportsAnimation != true) { StopPlayback(); return; }
        int count = (int)_phase.Maximum + 1;
        _currentFrame = (_currentFrame + 1) % count;
        _phase.Value = _currentFrame;
        UpdateFrameLabel();
        // ValueChanged programma l'anteprima senza bloccare il thread UI.
    }

    private void StopPlayback()
    {
        _playTimer.Stop();
        _play.Content = "▶ Riproduci";
    }

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
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true);
            await LoadRecipeStreamAsync(stream);
            await RecordRecentAsync(path);
            SetStatus("Ricetta caricata: " + Path.GetFileName(path));
        }
        catch (Exception ex) { SetStatus("Impossibile aprire la ricetta recente: " + ex.Message, error: true); }
    }

    private async Task LoadRecipeStreamAsync(Stream stream)
    {
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[4096];
        int count;
        while ((count = await stream.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + count > ArtworkRecipeJson.MaximumUtf8Bytes)
                throw new InvalidDataException("Ricetta oltre 64 KiB.");
            buffer.Write(chunk, 0, count);
        }
        ArtworkRecipe loaded = ArtworkRecipeJson.Deserialize(buffer.ToArray(), _catalog);
        _renderer.Validate(loaded, loaded.Animation is null ? null : 0);
        SelectArtwork(_catalog.Get(loaded.ArtworkId), loaded);
    }

    private async Task SaveRecipeAsync()
    {
        try
        {
            ArtworkRecipe recipe = BuildRecipe();
            byte[] payload = ArtworkRecipeJson.Serialize(recipe, _catalog);
            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salva ricetta", SuggestedFileName = recipe.ArtworkId + ".json",
                DefaultExtension = "json", ShowOverwritePrompt = true,
                FileTypeChoices = [new FilePickerFileType("Ricette JSON") { Patterns = ["*.json"] }]
            });
            if (file is null) return;
            await WritePickedFileAsync(file, payload);
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
            _previewCancellation?.Cancel();
            CancellationToken ct = source.Token;
            SetStatus("Export PNG: attesa del renderer…  ·  Annulla export per interrompere");
            await _renderGate.WaitAsync(ct);
            try
            {
                if (file.Path.IsFile)
                {
                    // Scrittura a file temporaneo: lo stream è passato direttamente
                    // al renderer, senza creare un byte[] del PNG a piena risoluzione.
                    SetStatus("Export PNG: rendering e scrittura su file temporaneo…");
                    await AtomicOutputFiles.WriteGeneratedAsync(file.Path.LocalPath,
                        (stream, token) => _renderer.Render(recipe, stream, frame, token),
                        overwrite: true, cancellationToken: ct);
                }
                else
                {
                    // I provider non-file non offrono un rename atomico: compatibilità
                    // mediante buffer singolo. Il token viene verificato anche prima
                    // di aprire lo stream remoto e passato alla scrittura.
                    SetStatus("Export PNG: rendering per provider virtuale (non atomico)…");
                    byte[] png = await Task.Run(() =>
                    {
                        using var buffer = new MemoryStream();
                        _renderer.Render(recipe, buffer, frame, ct);
                        return buffer.ToArray();
                    }, ct);
                    ct.ThrowIfCancellationRequested();
                    await using Stream output = await file.OpenWriteAsync();
                    await output.WriteAsync(png, ct);
                    await output.FlushAsync(ct);
                }
            }
            finally { _renderGate.Release(); }
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

    private static async Task WritePickedFileAsync(IStorageFile selected, byte[] data)
    {
        // Il file picker è l'unica autorizzazione a sovrascrivere; sul file system
        // l'output viene pubblicato con rename atomico invece di troncare prima.
        if (selected.Path.IsFile)
        {
            await AtomicOutputFiles.WriteAsync(selected.Path.LocalPath, data, overwrite: true);
        }
        else
        {
            // Provider virtuali: non garantiscono rename atomico.
            await using Stream stream = await selected.OpenWriteAsync();
            await stream.WriteAsync(data);
        }
    }

    private void SetStatus(string message, bool error = false)
    {
        _status.Text = message;
        _status.Foreground = error ? Ink("#FF9A9A") : Ink("#A9E6D7");
    }
}
