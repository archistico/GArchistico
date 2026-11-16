namespace GenerativeArt.Core.Pixels;

/// <summary>
/// Rettangolo a coordinate intere con origine inclusa e bordo finale escluso.
/// L'origine può essere negativa; larghezza e altezza non possono esserlo.
/// Un lato di lunghezza zero identifica una regione vuota.
/// </summary>
public readonly record struct PixelRectangle
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public PixelRectangle(int x, int y, int width, int height)
    {
        if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>Il risultato a 64 bit evita overflow di X + Width.</summary>
    public long RightExclusive => (long)X + Width;
    public long BottomExclusive => (long)Y + Height;
    public bool IsEmpty => Width == 0 || Height == 0;
}
