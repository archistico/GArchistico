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

/// <summary>Comunicazioni di stato dello Studio.</summary>
public sealed partial class StudioWindow
{
    private void SetStatus(string message, bool error = false)
    {
        _status.Text = message;
        _status.Foreground = error ? Ink("#FF9A9A") : Ink("#A9E6D7");
    }
}
