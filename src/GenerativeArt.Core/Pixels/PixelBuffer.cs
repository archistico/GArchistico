using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Core.Pixels;

/// <summary>
/// Buffer raster gestito e proprietario, indipendente da SkiaSharp e Avalonia.
/// Ogni pixel è un uint ARGB straight (0xAARRGGBB); lo stride è espresso in pixel.
/// Il padding di riga non fa parte dell'immagine e non viene restituito dalle API di riga.
/// L'istanza non è thread-safe per scritture sovrapposte: i chiamanti possono
/// parallelizzare soltanto regioni disgiunte, coordinando la propria esecuzione.
/// </summary>
public sealed class PixelBuffer
{
    private readonly uint[] _pixels;

    public CanvasSize Size { get; }
    public int Width => Size.Width;
    public int Height => Size.Height;
    public int StridePixels { get; }
    public PixelStorageFormat StorageFormat => PixelStorageFormat.PackedArgb32Straight;
    public long AllocatedBytes => (long)_pixels.Length * sizeof(uint);

    /// <param name="size">Dimensioni già validate dal Core.</param>
    /// <param name="stridePixels">Passo di riga in pixel, almeno pari alla larghezza.</param>
    public PixelBuffer(CanvasSize size, int? stridePixels = null)
    {
        ArgumentNullException.ThrowIfNull(size);
        int stride = stridePixels ?? size.Width;
        if (stride < size.Width)
            throw new ArgumentOutOfRangeException(nameof(stridePixels), "Lo stride non può essere inferiore alla larghezza.");

        long allocatedPixels = (long)stride * size.Height;
        if (allocatedPixels > CanvasSize.MaximumPixelCount)
            throw new ArgumentOutOfRangeException(nameof(stridePixels), "Il buffer, compreso il padding, supera il budget di memoria.");

        Size = size;
        StridePixels = stride;
        _pixels = new uint[checked((int)allocatedPixels)];
    }

    /// <summary>Legge un pixel; per coordinate esterne lancia un'eccezione.</summary>
    public Color32 GetPixel(int x, int y)
    {
        EnsureInside(x, y);
        return Color32.FromArgb(_pixels[y * StridePixels + x]);
    }

    /// <summary>Legge senza eccezioni; fuori area restituisce false e Transparent.</summary>
    public bool TryGetPixel(int x, int y, out Color32 color)
    {
        if (!Size.Contains(x, y))
        {
            color = Color32.Transparent;
            return false;
        }

        color = Color32.FromArgb(_pixels[y * StridePixels + x]);
        return true;
    }

    public void SetPixel(int x, int y, Color32 color)
    {
        EnsureInside(x, y);
        _pixels[y * StridePixels + x] = color.PackedArgb;
    }

    /// <summary>Ignora coordinate esterne senza poter scrivere in righe adiacenti.</summary>
    public bool TrySetPixel(int x, int y, Color32 color)
    {
        if (!Size.Contains(x, y)) return false;
        _pixels[y * StridePixels + x] = color.PackedArgb;
        return true;
    }

    /// <summary>Riempie esclusivamente i pixel visibili; il padding resta invariato.</summary>
    public void Clear(Color32 color)
    {
        uint packed = color.PackedArgb;
        for (int y = 0; y < Height; y++)
            _pixels.AsSpan(y * StridePixels, Width).Fill(packed);
    }

    /// <summary>
    /// Riempie l'intersezione con il canvas, restituendo i pixel scritti.
    /// Il clipping usa 64 bit per non far andare in overflow i bordi.
    /// </summary>
    public long FillRectangle(PixelRectangle rectangle, Color32 color)
    {
        long left = Math.Max(0L, rectangle.X);
        long top = Math.Max(0L, rectangle.Y);
        long right = Math.Min((long)Width, rectangle.RightExclusive);
        long bottom = Math.Min((long)Height, rectangle.BottomExclusive);
        if (left >= right || top >= bottom) return 0;

        int start = (int)left;
        int length = (int)(right - left);
        uint packed = color.PackedArgb;
        for (long y = top; y < bottom; y++)
            _pixels.AsSpan((int)y * StridePixels + start, length).Fill(packed);

        return (right - left) * (bottom - top);
    }

    /// <summary>Restituisce una vista dei soli pixel visibili della riga, non del padding.</summary>
    public ReadOnlySpan<uint> GetPackedArgbRow(int y)
    {
        EnsureRow(y);
        return _pixels.AsSpan(y * StridePixels, Width);
    }

    /// <summary>
    /// Vista scrivibile sul contenuto ARGB straight. Non scrivere in questa vista
    /// valori premoltiplicati o byte reinterpretati come ARGB senza conversione.
    /// </summary>
    public Span<uint> GetWritablePackedArgbRow(int y)
    {
        EnsureRow(y);
        return _pixels.AsSpan(y * StridePixels, Width);
    }

    /// <summary>
    /// Copia i pixel logici in un buffer di uint 0xAARRGGBB con stride di destinazione.
    /// Non effettua conversione di endianness né premoltiplicazione alpha.
    /// Il contenuto del padding della destinazione resta invariato.
    /// </summary>
    public void CopyPackedArgbTo(Span<uint> destination, int destinationStridePixels)
    {
        if (destinationStridePixels < Width)
            throw new ArgumentOutOfRangeException(nameof(destinationStridePixels));

        long required = (long)(Height - 1) * destinationStridePixels + Width;
        if (required > destination.Length)
            throw new ArgumentException("Buffer di destinazione insufficiente.", nameof(destination));

        for (int y = 0; y < Height; y++)
        {
            _pixels.AsSpan(y * StridePixels, Width)
                .CopyTo(destination.Slice(y * destinationStridePixels, Width));
        }
    }

    private void EnsureInside(int x, int y)
    {
        if ((uint)x >= (uint)Width)
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinata X esterna al canvas.");
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y), "Coordinata Y esterna al canvas.");
    }

    private void EnsureRow(int y)
    {
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));
    }
}
