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
        RefreshParameterApplicability();
        SchedulePreview();
    }

    private void PopulateParameters()
    {
        long editorRevision = ++_parameterEditorRevision;
        // TextChanged può arrivare dal dispatcher dopo la sostituzione del pannello.
        bool IsCurrentEditor() => !_loadingFields && editorRevision == _parameterEditorRevision;
        _parameterItems.Children.Clear();
        _swatchRefresh.Clear();
        _applicabilityEditors.Clear();
        _resetEditorOnDisable.Clear();
        string? currentGroup = null;
        foreach (ArtworkParameterSpec spec in _selected!.Parameters)
        {
            if (currentGroup != spec.Group)
            {
                currentGroup = spec.Group;
                _parameterItems.Children.Add(SectionLabel(spec.Group.ToUpperInvariant()));
            }
            Control editor = CreateParameterControl(spec, IsCurrentEditor);
            var container = new StackPanel { Spacing = 3 };
            container.Children.Add(editor);
            var note = new TextBlock
            {
                FontSize = 11, TextWrapping = TextWrapping.Wrap,
                Foreground = Ink("#9EAEC0"), IsVisible = false
            };
            container.Children.Add(note);
            _applicabilityEditors[spec.Id] = (editor, note);
            _parameterItems.Children.Add(container);
        }
    }

    private void OnRecipeChanged()
    {
        if (_loadingFields || _selected is null) return;
        StopPlayback();
        RefreshParameterApplicability();
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
