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

/// <summary>Timeline e riproduzione, senza accumulare frame.</summary>
public sealed partial class StudioWindow
{
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

}
