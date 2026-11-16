using System.Runtime.InteropServices;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Conversione esplicita fra l'ARGB numerico del Core e la rappresentazione BGRA8888
/// in byte di Skia. Non reinterpreta mai un uint del Core come un pixel nativo.
/// Il Core conserva alpha straight; la bitmap restituita usa Unpremul.
/// Il chiamante diventa proprietario della SKBitmap e deve chiamare Dispose().
/// </summary>
public static class SkiaPixelAdapter
{
    public static SKBitmap ToBitmap(PixelBuffer source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var info = new SKImageInfo(source.Width, source.Height,
            SKColorType.Bgra8888, SKAlphaType.Unpremul);
        var bitmap = new SKBitmap(info);
        try
        {
            IntPtr pointer = bitmap.GetPixels();
            if (pointer == IntPtr.Zero || bitmap.RowBytes < checked(source.Width * 4))
                throw new InvalidOperationException("Impossibile allocare la bitmap nativa Skia.");

            // Un buffer per riga: nessun grande array temporaneo per l'intera immagine.
            byte[] bytes = new byte[checked(source.Width * 4)];
            for (int y = 0; y < source.Height; y++)
            {
                ReadOnlySpan<uint> row = source.GetPackedArgbRow(y);
                for (int x = 0; x < row.Length; x++)
                {
                    uint argb = row[x];
                    int o = x * 4;
                    bytes[o] = (byte)argb;          // B
                    bytes[o + 1] = (byte)(argb >> 8);  // G
                    bytes[o + 2] = (byte)(argb >> 16); // R
                    bytes[o + 3] = (byte)(argb >> 24); // A
                }
                Marshal.Copy(bytes, 0, IntPtr.Add(pointer, checked(y * bitmap.RowBytes)), bytes.Length);
            }
            bitmap.NotifyPixelsChanged();
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Legge qualsiasi bitmap Skia supportata da GetPixel; privilegia la correttezza
    /// rispetto alle prestazioni. Una lettura bulk verrà valutata dopo i benchmark.
    /// </summary>
    public static PixelBuffer ToPixelBuffer(SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        // IsDisposed non è accessibile pubblicamente in SkiaSharp 4.153.2.
        // Un handle nullo evita di interrogare una bitmap nativa già rilasciata.
        if (bitmap.Handle == IntPtr.Zero || !bitmap.ReadyToDraw)
            throw new ArgumentException("Bitmap Skia non valida o già rilasciata.", nameof(bitmap));

        var result = new PixelBuffer(new CanvasSize(bitmap.Width, bitmap.Height));
        for (int y = 0; y < result.Height; y++)
        {
            Span<uint> row = result.GetWritablePackedArgbRow(y);
            for (int x = 0; x < row.Length; x++)
            {
                SKColor c = bitmap.GetPixel(x, y);
                row[x] = new Color32(c.Alpha, c.Red, c.Green, c.Blue).PackedArgb;
            }
        }
        return result;
    }
}
