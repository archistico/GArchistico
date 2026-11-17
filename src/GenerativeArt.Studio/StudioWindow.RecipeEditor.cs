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

/// <summary>Selezione opera, controlli e convalida ricetta.</summary>
public sealed partial class StudioWindow
{
    private void SelectArtwork(ArtworkDescriptor descriptor, ArtworkRecipe? restore = null)
    {
        StopPlayback();
        _loadingFields = true;
        try
        {
            _selected = descriptor;
            _parameters.Clear();
            _invalidNumericInputs.Clear();
            ArtworkRecipe initial = restore ?? StudioRecipeDefaults.RecipeFor(descriptor.Id);
            foreach (ArtworkParameterSpec item in descriptor.Parameters)
                _parameters.Add(item.Id, initial.Parameters[item.Id]);
            _seed.Text = initial.Seed.ToString(CultureInfo.InvariantCulture);
            _width.Text = initial.Size.Width.ToString(CultureInfo.InvariantCulture);
            _height.Text = initial.Size.Height.ToString(CultureInfo.InvariantCulture);
            _frames.Text = (initial.Animation?.FrameCount ?? StudioRecipeDefaults.FrameCount).ToString(CultureInfo.InvariantCulture);
            _fps.Text = (initial.Animation?.FramesPerSecond ?? StudioRecipeDefaults.FramesPerSecond).ToString(CultureInfo.InvariantCulture);
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

    private ArtworkRecipe BuildRecipe() => StudioRecipeFormParser.Parse(
        _selected ?? throw new InvalidOperationException("Nessuna opera selezionata."),
        _seed.Text, _width.Text, _height.Text, _parameters, _invalidNumericInputs,
        _frames.Text, _fps.Text);

}
