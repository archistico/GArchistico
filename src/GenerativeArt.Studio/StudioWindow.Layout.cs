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

/// <summary>Composizione visuale e temi: layout invariato.</summary>
public sealed partial class StudioWindow
{
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
        right.Children.Add(SectionLabel("ANTEPRIMA  /  PIXEL REALI"));
        right.Children.Add(new TextBlock
        {
            Text = "Anteprima e PNG esportato usano sempre la risoluzione indicata sopra. " +
                "Il riquadro è indipendente: Adatta mostra tutta l'opera, 100% mostra i pixel reali. " +
                "Non viene generata una versione raster ridotta.",
            FontSize = 11, TextWrapping = TextWrapping.Wrap, Foreground = Ink("#9EAEC0")
        });
        var previewAreaSizes = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,*"), ColumnSpacing = 10 };
        previewAreaSizes.Children.Add(Field("RIQUADRO MAX  LARGHEZZA DIP", _previewAreaWidth));
        var areaHeightGroup = Field("RIQUADRO MAX  ALTEZZA DIP", _previewAreaHeight);
        Grid.SetColumn(areaHeightGroup, 1);
        previewAreaSizes.Children.Add(areaHeightGroup);
        right.Children.Add(previewAreaSizes);
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

}
