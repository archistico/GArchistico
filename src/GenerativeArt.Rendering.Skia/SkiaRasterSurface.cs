using System.Numerics;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Superficie CPU headless. Possiede SKSurface, il canvas è in prestito e non va
/// distrutto separatamente. Disegno e Dispose non sono thread-safe.
/// Il raster Skia usa alpha premoltiplicato; la conversione dal Core è esplicita.
/// </summary>
public sealed class SkiaRasterSurface : IDisposable
{
    private readonly SKSurface _surface;
    private bool _disposed;

    public CanvasSize Size { get; }
    public int RasterScale { get; }

    public SkiaRasterSurface(CanvasSize size, int rasterScale = 1)
    {
        ArgumentNullException.ThrowIfNull(size);
        if (rasterScale is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(rasterScale), "Scale raster ammesse: 1 e 2.");
        if (rasterScale == 2 && size.PixelCount * 4L > RasterQualityPolicy.MaximumFineRasterPixels)
            throw new ArgumentOutOfRangeException(nameof(size), "Il canvas 2x supera il budget raster.");
        Size = size;
        RasterScale = rasterScale;
        _surface = SKSurface.Create(new SKImageInfo(size.Width * rasterScale, size.Height * rasterScale,
            SKColorType.Bgra8888, SKAlphaType.Premul))
            ?? throw new InvalidOperationException("Creazione della superficie raster Skia fallita.");
        _surface.Canvas.Clear(SKColors.Transparent);
        if (rasterScale == 2) _surface.Canvas.Scale(2f);
    }

    public void Clear(Color32 color)
    {
        ThrowIfDisposed();
        _surface.Canvas.Clear(ToSkColor(color));
    }

    public void DrawLine(float x1, float y1, float x2, float y2, Color32 color, float strokeWidth = 1)
    {
        ThrowIfDisposed();
        EnsureFinite(x1, nameof(x1));
        EnsureFinite(y1, nameof(y1));
        EnsureFinite(x2, nameof(x2));
        EnsureFinite(y2, nameof(y2));
        EnsureFinitePositive(strokeWidth, nameof(strokeWidth));
        using var paint = new SKPaint
        {
            Color = ToSkColor(color),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            StrokeCap = SKStrokeCap.Round
        };
        _surface.Canvas.DrawLine(x1, y1, x2, y2, paint);
    }

    public void DrawCircle(float x, float y, float radius, Color32 color, bool fill = false,
        float strokeWidth = 1)
    {
        ThrowIfDisposed();
        EnsureFinite(x, nameof(x));
        EnsureFinite(y, nameof(y));
        EnsureFinitePositive(radius, nameof(radius));
        EnsureFinitePositive(strokeWidth, nameof(strokeWidth));
        using var paint = new SKPaint
        {
            Color = ToSkColor(color),
            IsAntialias = true,
            Style = fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth
        };
        _surface.Canvas.DrawCircle(x, y, radius, paint);
    }

    /// <summary>
    /// Un'unica draw call Skia per una curva di livello connessa; le coordinate
    /// della curva sono nella griglia e vengono scalate sul canvas.
    /// </summary>
    public void DrawPolyline(IReadOnlyList<Vector2> points, float scaleX, float scaleY,
        Color32 color, float strokeWidth = 1)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(points);
        EnsureFinitePositive(scaleX, nameof(scaleX));
        EnsureFinitePositive(scaleY, nameof(scaleY));
        EnsureFinitePositive(strokeWidth, nameof(strokeWidth));
        if (points.Count < 2) return;
        // SkiaSharp 4.x mantiene SKPath immutabile: costruire la geometria
        // con SKPathBuilder evita le API SKPath.MoveTo/LineTo obsolete (CS0618).
        using var builder = new SKPathBuilder();
        bool first = true;
        foreach (Vector2 point in points)
        {
            EnsureFinite(point.X, nameof(points));
            EnsureFinite(point.Y, nameof(points));
            if (first) { builder.MoveTo(point.X * scaleX, point.Y * scaleY); first = false; }
            else builder.LineTo(point.X * scaleX, point.Y * scaleY);
        }
        using var path = builder.Detach();
        using var paint = new SKPaint
        {
            Color = ToSkColor(color),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        _surface.Canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Poligono chiuso antialias con riempimento e bordo; usa SKPathBuilder
    /// compatibile con SkiaSharp 4.x, senza API SKPath obsolete.
    /// </summary>
    public void DrawPolygon(IReadOnlyList<Vector2> vertices, Color32 fill, Color32 outline,
        float outlineWidth = 1)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Count < 3)
            throw new ArgumentException("Un poligono richiede almeno tre vertici.", nameof(vertices));
        EnsureFinitePositive(outlineWidth, nameof(outlineWidth));
        using var builder = new SKPathBuilder();
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 p = vertices[i];
            EnsureFinite(p.X, nameof(vertices));
            EnsureFinite(p.Y, nameof(vertices));
            if (i == 0) builder.MoveTo(p.X, p.Y);
            else builder.LineTo(p.X, p.Y);
        }
        builder.LineTo(vertices[0].X, vertices[0].Y);
        using var path = builder.Detach();
        using var fillPaint = new SKPaint
        {
            Color = ToSkColor(fill), IsAntialias = true, Style = SKPaintStyle.Fill
        };
        using var edgePaint = new SKPaint
        {
            Color = ToSkColor(outline), IsAntialias = true,
            Style = SKPaintStyle.Stroke, StrokeWidth = outlineWidth,
            StrokeJoin = SKStrokeJoin.Round
        };
        _surface.Canvas.DrawPath(path, fillPaint);
        _surface.Canvas.DrawPath(path, edgePaint);
    }

    /// <summary>Copia i pixel managed nella superficie nativa con compositing SrcOver.</summary>
    public void DrawPixels(PixelBuffer pixels, float x = 0, float y = 0, bool nativeResolution = false)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(pixels);
        EnsureFinite(x, nameof(x));
        EnsureFinite(y, nameof(y));
        if (nativeResolution && (pixels.Size.Width != Size.Width * RasterScale ||
                                 pixels.Size.Height != Size.Height * RasterScale))
            throw new ArgumentException("Buffer nativo non corrispondente alla superficie.", nameof(pixels));
        using SKBitmap bitmap = SkiaPixelAdapter.ToBitmap(pixels);
        // Per i renderer per-pixel il buffer è già alla risoluzione nativa:
        // si annulla solo temporaneamente la trasformazione del canvas.
        int saved = _surface.Canvas.Save();
        try
        {
            if (nativeResolution && RasterScale == 2) _surface.Canvas.Scale(.5f);
            _surface.Canvas.DrawBitmap(bitmap, x, y, new SKSamplingOptions(SKFilterMode.Nearest));
        }
        finally { _surface.Canvas.RestoreToCount(saved); }
    }

    /// <summary>Lo stream rimane aperto; nessuna dipendenza da file system o GUI.</summary>
    public void WritePng(Stream destination, ArtisticFinishOptions finishing = default,
        ulong seed = 0, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Lo stream non è scrivibile.", nameof(destination));
        cancellationToken.ThrowIfCancellationRequested();
        if (finishing.IsEnabled)
        {
            finishing.Validate();
            if (Size.PixelCount > ArtisticFinishOptions.MaximumFinishPixels)
                throw new ArgumentOutOfRangeException(nameof(finishing),
                    $"Finitura artistica oltre il budget di {ArtisticFinishOptions.MaximumFinishPixels:N0} pixel.");
        }
        using SKImage source = _surface.Snapshot();
        if (RasterScale == 1)
        {
            WriteFinished(source, destination, finishing, seed, cancellationToken);
            return;
        }

        // Riduzione premoltiplicata prima della finitura, che usa SEMPRE
        // coordinate finali: grana e dithering non dipendono dal raster scale.
        using SKSurface reduced = SKSurface.Create(new SKImageInfo(Size.Width, Size.Height,
            SKColorType.Bgra8888, SKAlphaType.Premul))
            ?? throw new InvalidOperationException("Creazione della superficie di riduzione fallita.");
        reduced.Canvas.Clear(SKColors.Transparent);
        reduced.Canvas.DrawImage(source, new SKRect(0, 0, Size.Width, Size.Height),
            new SKSamplingOptions(SKFilterMode.Linear));
        using SKImage reducedImage = reduced.Snapshot();
        WriteFinished(reducedImage, destination, finishing, seed, cancellationToken);
    }

    private static void WriteFinished(SKImage image, Stream destination,
        ArtisticFinishOptions finishing, ulong seed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!finishing.IsEnabled)
        {
            Encode(image, destination);
            return;
        }
        // Conversione solo se richiesta: la via legacy rimane byte-identica.
        using SKBitmap bitmap = SKBitmap.FromImage(image)
            ?? throw new InvalidOperationException("Impossibile leggere lo snapshot per la finitura.");
        PixelBuffer pixels = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        ArtisticPostProcessor.Apply(pixels, finishing, seed, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        SkiaPngEncoder.Write(pixels, destination);
    }

    private static void Encode(SKImage image, Stream destination)
    {
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Codifica PNG della superficie fallita.");
        data.SaveTo(destination);
    }

    public void SavePng(string path)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        WritePng(stream);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _surface.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SkiaRasterSurface));
    }

    private static SKColor ToSkColor(Color32 c) => new(c.R, c.G, c.B, c.A);

    private static void EnsureFinite(float v, string name)
    {
        if (!float.IsFinite(v)) throw new ArgumentOutOfRangeException(name);
    }

    private static void EnsureFinitePositive(float v, string name)
    {
        if (!float.IsFinite(v) || v <= 0) throw new ArgumentOutOfRangeException(name);
    }
}
