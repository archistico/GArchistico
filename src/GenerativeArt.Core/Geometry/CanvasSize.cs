namespace GenerativeArt.Core.Geometry;

/// <summary>
/// Dimensioni positive del canvas. La superficie non viene allocata dal costruttore.
/// Un limite prudenziale evita overflow e richieste manifestamente eccessive.
/// </summary>
public sealed record CanvasSize
{
    public const long MaximumPixelCount = 67_108_864; // 256 MiB a 4 byte/pixel

    public int Width { get; }
    public int Height { get; }
    public long PixelCount { get; }
    public float AspectRatio => (float)Width / Height;
    public float CenterX => Width / 2f;
    public float CenterY => Height / 2f;

    public CanvasSize(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "La larghezza deve essere positiva.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "L'altezza deve essere positiva.");

        long pixels = (long)width * height;
        if (pixels > MaximumPixelCount)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"La superficie supera il limite di {MaximumPixelCount} pixel.");

        Width = width;
        Height = height;
        PixelCount = pixels;
    }

    /// <summary>Controlla coordinate continue senza troncamento verso zero (errore di Gȧrt Dimension.isInside).</summary>
    public bool Contains(float x, float y) =>
        float.IsFinite(x) && float.IsFinite(y) &&
        x >= 0 && y >= 0 && x < Width && y < Height;

    public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
}
