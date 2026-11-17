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
    private bool _layerActive;
    private readonly bool _production;

    public CanvasSize Size { get; }
    public int RasterScale { get; }

    public SkiaRasterSurface(CanvasSize size, int rasterScale = 1, bool production = false)
    {
        ArgumentNullException.ThrowIfNull(size);
        if (rasterScale is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(rasterScale), "Scale raster ammesse: 1 e 2.");
        RasterCanvasBudgetPolicy.ValidateEightKDimensions(size);
        long maximum = production ? GenerativeArt.Application.Rendering.ProductionRenderProfile.MaximumNativePixels : RasterQualityPolicy.MaximumFineRasterPixels;
        if (production && (size.Width > 4096 || size.Height > 4096 || size.PixelCount > maximum / 4))
            throw new ArgumentOutOfRangeException(nameof(size), "Superficie di produzione oltre il formato 4K.");
        if (rasterScale == 2 && size.PixelCount * 4L > maximum)
            throw new ArgumentOutOfRangeException(nameof(size), "Il canvas 2x supera il budget raster.");
        Size = size;
        RasterScale = rasterScale;
        _production = production;
        _surface = SKSurface.Create(new SKImageInfo(size.Width * rasterScale, size.Height * rasterScale,
            SKColorType.Bgra8888, SKAlphaType.Premul))
            ?? throw new InvalidOperationException("Creazione della superficie raster Skia fallita.");
        _surface.Canvas.Clear(SKColors.Transparent);
        if (rasterScale == 2) _surface.Canvas.Scale(2f);
    }

    internal void PaintCincinnus(IReadOnlyList<GenerativeArt.Algorithms.Cincinnus.CincinnusBrushStroke> strokes,
        long seed, CincinnusPaintOptions options, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        CincinnusBristlePainter.Paint(_surface.Canvas, strokes, Size.Width, Size.Height,
            seed, options, cancellationToken, _production);
    }

    public void Clear(Color32 color)
    {
        ThrowIfDisposed();
        _surface.Canvas.Clear(ToSkColor(color));
    }

    /// <summary>Un solo livello temporaneo alla volta, rilasciato anche in caso di errore/cancellazione.</summary>
    public void DrawLayer(Action<SkiaRasterSurface> draw, LayerEffects effects,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(draw);
        effects.Validate(_production);
        cancellationToken.ThrowIfCancellationRequested();
        if (!effects.IsEnabled) { draw(this); cancellationToken.ThrowIfCancellationRequested(); return; }
        if (_layerActive) throw new InvalidOperationException("Livelli filtrati annidati non ammessi.");
        if (Size.PixelCount * RasterScale * RasterScale > (_production
            ? GenerativeArt.Application.Rendering.ProductionRenderProfile.MaximumNativePixels : 8_388_608))
            throw new ArgumentOutOfRangeException(nameof(effects), "Livelli filtrati oltre il budget nativo del profilo selezionato.");
        using var blur = effects.Blur > 0 ? SKImageFilter.CreateBlur((float)effects.Blur, (float)effects.Blur) : null;
        using var shadow = effects.ShadowOpacity > 0 ? SKImageFilter.CreateDropShadow(
            (float)effects.ShadowX, (float)effects.ShadowY,
            (float)effects.ShadowSoftness, (float)effects.ShadowSoftness,
            ToSkColor(ArtworkPaint.Alpha(effects.ShadowColor ?? Color32.Opaque(0, 0, 0), effects.ShadowOpacity)), blur) : null;
        using var paint = new SKPaint
        {
            ImageFilter = shadow ?? blur,
            Color = SKColors.White.WithAlpha((byte)Math.Round(255 * (effects.Opacity ?? 1)))
        };
        int saved = _surface.Canvas.SaveLayer(paint);
        _layerActive = true;
        try { draw(this); cancellationToken.ThrowIfCancellationRequested(); }
        finally { _surface.Canvas.RestoreToCount(saved); _layerActive = false; }
    }

    /// <summary>Foglio con apertura trasparente; riempimento even-odd, senza cancellare gli strati sotto.</summary>
    public void DrawAperture(IReadOnlyList<Vector2> hole, Color32 color)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(hole);
        if (hole.Count < 4) throw new ArgumentException("Apertura incompleta.", nameof(hole));
        using var builder = new SKPathBuilder { FillType = SKPathFillType.EvenOdd };
        builder.AddRect(new SKRect(-1024, -1024, Size.Width + 1024, Size.Height + 1024));
        for (int i = 0; i < hole.Count; i++)
        {
            EnsureFinite(hole[i].X, nameof(hole)); EnsureFinite(hole[i].Y, nameof(hole));
            if (i == 0) builder.MoveTo(hole[i].X, hole[i].Y);
            else builder.LineTo(hole[i].X, hole[i].Y);
        }
        builder.Close();
        using var path = builder.Detach();
        using var paint = new SKPaint { Color = ToSkColor(color), IsAntialias = true };
        _surface.Canvas.DrawPath(path, paint);
    }

    /// <summary>Disco illuminato proceduralmente; il gradiente conserva alpha e colore del pigmento.</summary>
    public void DrawLitDisc(float x, float y, float radius, Color32 color, double lightAngle, double strength)
    {
        ThrowIfDisposed();
        EnsureFinite(x, nameof(x)); EnsureFinite(y, nameof(y)); EnsureFinitePositive(radius, nameof(radius));
        if (!double.IsFinite(lightAngle) || !double.IsFinite(strength) || strength is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(strength));
        var light = new SKPoint(x + radius * .38f * (float)Math.Cos(lightAngle),
            y + radius * .38f * (float)Math.Sin(lightAngle));
        Color32 white = new(color.A, 255, 255, 255), dark = new(color.A, 8, 12, 25);
        using var shader = SKShader.CreateRadialGradient(light, radius * 1.5f,
            [ToSkColor(ArtworkPaint.Mix(color, white, strength * .7)), ToSkColor(color),
             ToSkColor(ArtworkPaint.Mix(color, dark, strength * .7))], [0f, .4f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint { IsAntialias = true, Shader = shader };
        _surface.Canvas.DrawCircle(x, y, radius, paint);
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

    /// <summary>Faccia illuminata e smussi convessi; alpha applicato una volta alle regioni disgiunte.</summary>
    public void DrawBeveledConvexPolygon(IReadOnlyList<Vector2> vertices, Color32 fill, Color32 outline,
        float outlineWidth, float bevelWidth, double relief, double lightAngle)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Count < 3) throw new ArgumentException("Poligono incompleto.", nameof(vertices));
        EnsureFinitePositive(outlineWidth, nameof(outlineWidth));
        if (!float.IsFinite(bevelWidth) || bevelWidth < 0 || !double.IsFinite(relief) || relief is < 0 or > 1 ||
            !double.IsFinite(lightAngle)) throw new ArgumentOutOfRangeException(nameof(relief));
        Vector2 center = Vector2.Zero;
        double area = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 a = vertices[i], b = vertices[(i + 1) % vertices.Count];
            EnsureFinite(a.X, nameof(vertices)); EnsureFinite(a.Y, nameof(vertices));
            center += a;
            area += (double)a.X * b.Y - (double)b.X * a.Y;
        }
        if (Math.Abs(area) < 1e-8) throw new ArgumentException("Poligono degenere.", nameof(vertices));
        if (relief == 0) { DrawPolygon(vertices, fill, outline, outlineWidth); return; }
        center /= vertices.Count;
        var light = new Vector2((float)Math.Cos(lightAngle), (float)Math.Sin(lightAngle));
        float radius = float.MaxValue, span = 1;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 a = vertices[i], b = vertices[(i + 1) % vertices.Count], edge = b - a;
            float length = edge.Length();
            if (length > 1e-6f)
                radius = Math.Min(radius, Math.Abs(edge.X * (center.Y - a.Y) - edge.Y * (center.X - a.X)) / length);
            span = Math.Max(span, Math.Abs(Vector2.Dot(a - center, light)));
        }
        float inset = Math.Min(bevelWidth, radius * .38f);
        float scale = radius > 1e-6f ? 1 - inset / radius : 1;
        Vector2[] inner = vertices.Select(p => center + (p - center) * scale).ToArray();
        static SKPath Path(IReadOnlyList<Vector2> points)
        {
            using var builder = new SKPathBuilder();
            builder.MoveTo(points[0].X, points[0].Y);
            for (int i = 1; i < points.Count; i++) builder.LineTo(points[i].X, points[i].Y);
            builder.Close();
            return builder.Detach();
        }
        Color32 Lit(double amount) => ArtworkPaint.Mix(fill,
            amount >= 0 ? new Color32(fill.A, 255, 255, 255) : new Color32(fill.A, 0, 0, 0), Math.Abs(amount));
        using var outerPath = Path(vertices);
        using var facePath = Path(inset > 0 ? inner : vertices);
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(center.X + light.X * span, center.Y + light.Y * span),
            new SKPoint(center.X - light.X * span, center.Y - light.Y * span),
            [ToSkColor(Lit(.2 * relief)), ToSkColor(Lit(-.24 * relief))], [0f, 1f], SKShaderTileMode.Clamp);
        using var face = new SKPaint { Shader = shader, IsAntialias = inset <= 0 };
        int saved = _surface.Canvas.Save();
        try
        {
            if (inset > 0) _surface.Canvas.ClipPath(outerPath, SKClipOperation.Intersect, true);
            // Niente antialias sui bordi interni condivisi: evita doppio alpha o fughe scure.
            _surface.Canvas.DrawPath(facePath, face);
            if (inset > 0)
                for (int i = 0; i < vertices.Count; i++)
                {
                    int j = (i + 1) % vertices.Count;
                    Vector2 edge = vertices[j] - vertices[i];
                    float length = edge.Length();
                    if (length <= 1e-6f) continue;
                    Vector2 outward = new(edge.Y, -edge.X);
                    if (area < 0) outward = -outward;
                    double facing = Vector2.Dot(outward / length, light);
                    using var strip = Path([vertices[i], vertices[j], inner[j], inner[i]]);
                    using var paint = new SKPaint
                    { Color = ToSkColor(Lit(facing * relief * (facing >= 0 ? .48 : .62))), IsAntialias = false };
                    _surface.Canvas.DrawPath(strip, paint);
                }
        }
        finally { _surface.Canvas.RestoreToCount(saved); }
        if (outline.A > 0)
        {
            using var edgePaint = new SKPaint { Color = ToSkColor(outline), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = outlineWidth, StrokeJoin = SKStrokeJoin.Round };
            _surface.Canvas.DrawPath(outerPath, edgePaint);
        }
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
        // Il percorso storico rimane byte-identico fino a 4096² pixel.
        // Oltre tale budget si applicano le stesse funzioni sulle righe, evitando
        // una copia completa aggiuntiva in Core.PixelBuffer.
        if ((long)image.Width * image.Height > ArtisticFinishOptions.LegacyFullFrameFinishPixels)
        {
            ArtisticScanlineFinisher.WritePng(image, destination, finishing, seed, cancellationToken);
            return;
        }
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
