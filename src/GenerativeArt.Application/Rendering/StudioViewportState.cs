namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Trasformazione 2D dell'anteprima in coordinate logiche (DIP), senza dipendere da Avalonia.
/// Lo zoom mantiene fermo il punto indicato dall'utente; il pan non forza un render.
/// </summary>
public sealed class StudioViewportState
{
    // Necessario per adattare anche un raster 4096 px a una finestra stretta.
    public const double MinimumZoom = 0.02;
    public const double MaximumZoom = 8.0;
    private const double MinimumVisiblePixels = 32;

    public double Zoom { get; private set; } = 0.75;
    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }
    public double ViewportWidth { get; private set; }
    public double ViewportHeight { get; private set; }
    public double ImageWidth { get; private set; }
    public double ImageHeight { get; private set; }
    public bool HasImage => ImageWidth > 0 && ImageHeight > 0;
    public double ScaledWidth => ImageWidth * Zoom;
    public double ScaledHeight => ImageHeight * Zoom;

    public void Resize(double width, double height)
    {
        EnsureNonNegativeFinite(width, nameof(width));
        EnsureNonNegativeFinite(height, nameof(height));
        OffsetX += (width - ViewportWidth) / 2;
        OffsetY += (height - ViewportHeight) / 2;
        ViewportWidth = width;
        ViewportHeight = height;
        ClampPan();
    }

    public void SetImageSize(double width, double height)
    {
        EnsureNonNegativeFinite(width, nameof(width));
        EnsureNonNegativeFinite(height, nameof(height));
        // Un nuovo frame con dimensioni identiche deve conservare il pan/zoom corrente.
        if (width == ImageWidth && height == ImageHeight) return;
        ImageWidth = width;
        ImageHeight = height;
        Center();
    }

    public void Center()
    {
        OffsetX = (ViewportWidth - ScaledWidth) / 2;
        OffsetY = (ViewportHeight - ScaledHeight) / 2;
        ClampPan();
    }

    /// <summary>
    /// Mostra l'intera immagine senza deformarla e senza tagliare i bordi.
    /// Solo trasformazione della vista: il bitmap e la ricetta non cambiano.
    /// </summary>
    public bool FitImage(double margin = 12)
    {
        if (!HasImage || ViewportWidth <= 0 || ViewportHeight <= 0 ||
            !double.IsFinite(margin) || margin < 0) return false;
        double availableWidth = Math.Max(1, ViewportWidth - 2 * margin);
        double availableHeight = Math.Max(1, ViewportHeight - 2 * margin);
        Zoom = Math.Clamp(Math.Min(availableWidth / ImageWidth, availableHeight / ImageHeight),
            MinimumZoom, MaximumZoom);
        Center();
        return true;
    }

    public void Pan(double deltaX, double deltaY)
    {
        if (!HasImage) return;
        if (!double.IsFinite(deltaX) || !double.IsFinite(deltaY)) return;
        OffsetX += deltaX;
        OffsetY += deltaY;
        ClampPan();
    }

    public bool ZoomAt(double requestedZoom, double anchorX, double anchorY)
    {
        if (!double.IsFinite(requestedZoom) || !double.IsFinite(anchorX) || !double.IsFinite(anchorY))
            return false;
        double nextZoom = Math.Clamp(requestedZoom, MinimumZoom, MaximumZoom);
        if (nextZoom == Zoom) return false;
        double ratio = nextZoom / Zoom;
        OffsetX = anchorX - (anchorX - OffsetX) * ratio;
        OffsetY = anchorY - (anchorY - OffsetY) * ratio;
        Zoom = nextZoom;
        if (HasImage) ClampPan();
        return true;
    }

    public (double X, double Y) ImagePointAt(double screenX, double screenY) =>
        ((screenX - OffsetX) / Zoom, (screenY - OffsetY) / Zoom);

    private void ClampPan()
    {
        if (!HasImage) return;
        OffsetX = ClampAxis(OffsetX, ViewportWidth, ScaledWidth);
        OffsetY = ClampAxis(OffsetY, ViewportHeight, ScaledHeight);
    }

    private static double ClampAxis(double offset, double viewport, double image)
    {
        // Consenti di trascinare immagini più piccole della vista, ma evita di perderle
        // completamente fuori schermo. Con vista non ancora misurata non applicare limiti.
        if (viewport <= 0 || image <= 0) return offset;
        double visible = Math.Min(MinimumVisiblePixels, Math.Min(viewport, image) / 2);
        return Math.Clamp(offset, visible - image, viewport - visible);
    }

    private static void EnsureNonNegativeFinite(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(name, "La dimensione deve essere finita e non negativa.");
    }
}
