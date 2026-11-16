using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Studio;

/// <summary>
/// Editor sRGB senza librerie esterne: modifica HSV, canali RGB, alpha e HEX.
/// Il valore viene applicato alla ricetta soltanto quando si preme Conferma.
/// </summary>
internal sealed class StudioColorPickerWindow : Window
{
    private readonly Slider _hue = new() { Minimum = 0, Maximum = 360 };
    private readonly Slider _saturation = new() { Minimum = 0, Maximum = 100 };
    private readonly Slider _brightness = new() { Minimum = 0, Maximum = 100 };
    private readonly Slider _opacity = new() { Minimum = 0, Maximum = 255, TickFrequency = 1 };
    private readonly TextBox _red = new() { Width = 66 };
    private readonly TextBox _green = new() { Width = 66 };
    private readonly TextBox _blue = new() { Width = 66 };
    private readonly TextBox _alpha = new() { Width = 66 };
    private readonly TextBox _hex = new() { MinWidth = 150 };
    private readonly Border _sample = new() { Height = 72, CornerRadius = new CornerRadius(8),
        BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
    private readonly TextBlock _message = new() { Foreground = Brushes.OrangeRed,
        TextWrapping = TextWrapping.Wrap };
    private StudioColorValue _color;
    private bool _syncing;

    public string? SelectedHex { get; private set; }

    internal StudioColorPickerWindow(string title, string initialHex)
    {
        _color = StudioColorValue.Parse(initialHex);
        Title = "Colore · " + title;
        Width = 520;
        Height = 620;
        MinWidth = 440;
        MinHeight = 280;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Opened += (_, _) =>
        {
            var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
            if (screen is null) return;
            // WorkingArea è in pixel fisici, le dimensioni della finestra in DIP.
            MaxHeight = Math.Max(280, screen.WorkingArea.Height / screen.Scaling - 48);
            Height = Math.Min(Height, MaxHeight);
        };
        var body = new StackPanel { Spacing = 10 };
        body.Children.Add(new TextBlock { Text = title, FontSize = 19, FontWeight = FontWeight.SemiBold });
        body.Children.Add(new TextBlock { Text = "HSV · RGB · Alpha · HEX (#RRGGBB / #AARRGGBB)",
            FontSize = 12, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(_sample);
        body.Children.Add(SliderField("Tonalità H · 0–360°", _hue));
        body.Children.Add(SliderField("Saturazione S · 0–100%", _saturation));
        body.Children.Add(SliderField("Luminosità V · 0–100%", _brightness));
        body.Children.Add(SliderField("Opacità Alpha · 0–255", _opacity));

        var channels = new WrapPanel { Orientation = Orientation.Horizontal };
        channels.Children.Add(Field("R", _red));
        channels.Children.Add(Field("G", _green));
        channels.Children.Add(Field("B", _blue));
        channels.Children.Add(Field("A", _alpha));
        var rgbApply = new Button { Content = "Applica RGB", VerticalAlignment = VerticalAlignment.Bottom };
        rgbApply.Click += (_, _) => ApplyRgb();
        channels.Children.Add(rgbApply);
        foreach (Control channel in channels.Children) channel.Margin = new Thickness(0, 0, 8, 6);
        body.Children.Add(channels);
        var hexRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        hexRow.Children.Add(Field("Colore esadecimale", _hex));
        var hexApply = new Button { Content = "Applica HEX", VerticalAlignment = VerticalAlignment.Bottom };
        hexApply.Click += (_, _) => ApplyHex();
        hexRow.Children.Add(hexApply);
        body.Children.Add(hexRow);
        body.Children.Add(_message);
        var footer = new StackPanel { Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10,
            Margin = new Thickness(0, 12, 0, 0) };
        var cancel = new Button { Content = "Annulla", Padding = new Thickness(18, 8), IsCancel = true };
        cancel.Click += (_, _) => Close(false);
        var accept = new Button { Content = "Conferma colore", Padding = new Thickness(18, 8), IsDefault = true };
        accept.Click += (_, _) =>
        {
            // Testo RGB/HEX non confermato non modifica implicitamente il colore.
            SelectedHex = _color.ToHex();
            Close(true);
        };
        footer.Children.Add(cancel);
        footer.Children.Add(accept);
        // Scorre solo l'editor: Annulla e Conferma restano sempre visibili.
        var layout = new Grid { RowDefinitions = new RowDefinitions("*,Auto"), Margin = new Thickness(16) };
        layout.Children.Add(new ScrollViewer { Content = body,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled });
        Grid.SetRow(footer, 1);
        layout.Children.Add(footer);
        Content = layout;

        _hue.ValueChanged += (_, _) => ChangeHsv();
        _saturation.ValueChanged += (_, _) => ChangeHsv();
        _brightness.ValueChanged += (_, _) => ChangeHsv();
        _opacity.ValueChanged += (_, _) =>
        {
            if (_syncing) return;
            _color = _color with { A = (byte)Math.Round(_opacity.Value) };
            SyncControls(preserveHsv: true);
        };
        SyncControls();
    }

    private void ChangeHsv()
    {
        if (_syncing) return;
        _color = StudioColorValue.FromHsv(_hue.Value, _saturation.Value / 100d,
            _brightness.Value / 100d, _color.A);
        SyncControls(preserveHsv: true);
    }

    private void ApplyRgb()
    {
        if (byte.TryParse(_red.Text, NumberStyles.None, CultureInfo.InvariantCulture, out byte r) &&
            byte.TryParse(_green.Text, NumberStyles.None, CultureInfo.InvariantCulture, out byte g) &&
            byte.TryParse(_blue.Text, NumberStyles.None, CultureInfo.InvariantCulture, out byte b) &&
            byte.TryParse(_alpha.Text, NumberStyles.None, CultureInfo.InvariantCulture, out byte a))
        {
            _color = new StudioColorValue(r, g, b, a);
            SyncControls();
        }
        else _message.Text = "RGB e Alpha devono contenere interi da 0 a 255.";
    }

    private void ApplyHex()
    {
        try
        {
            _color = StudioColorValue.Parse(_hex.Text ?? "");
            SyncControls();
        }
        catch (FormatException ex) { _message.Text = ex.Message; }
    }

    private void SyncControls(bool preserveHsv = false)
    {
        _syncing = true;
        try
        {
            // Con S=0 o V=0 il colore RGB non conserva la tonalità selezionata.
            // Non azzerare quindi H/S mentre si usa lo slider HSV o Alpha.
            if (!preserveHsv)
            {
                (double hue, double saturation, double value) = _color.ToHsv();
                _hue.Value = hue;
                _saturation.Value = saturation * 100;
                _brightness.Value = value * 100;
            }
            _opacity.Value = _color.A;
            _red.Text = _color.R.ToString(CultureInfo.InvariantCulture);
            _green.Text = _color.G.ToString(CultureInfo.InvariantCulture);
            _blue.Text = _color.B.ToString(CultureInfo.InvariantCulture);
            _alpha.Text = _color.A.ToString(CultureInfo.InvariantCulture);
            _hex.Text = _color.ToHex();
            _sample.Background = new SolidColorBrush(Color.FromArgb(_color.A, _color.R, _color.G, _color.B));
            _message.Text = "";
        }
        finally { _syncing = false; }
    }

    private static StackPanel Field(string caption, Control editor)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock { Text = caption, FontSize = 11 });
        panel.Children.Add(editor);
        return panel;
    }

    private static Control SliderField(string caption, Slider slider) => Field(caption, slider);
}
