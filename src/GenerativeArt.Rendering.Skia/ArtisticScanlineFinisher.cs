using System.Runtime.InteropServices;
using GenerativeArt.Core.Colors;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Finitura 8K a righe: usa una bitmap Skia di output e quattro righe di uint,
/// senza un PixelBuffer ARGB grande quanto l'immagine. La conversione BGRA
/// premoltiplicata è esplicita e non interpreta mai i byte nativi come ARGB.
/// Il vicinato dell'assorbimento rimane quello ORIGINALE, anche ai confini
/// tra le righe. Grana/carta usano sempre coordinate globali (x, y).
/// </summary>
internal static class ArtisticScanlineFinisher
{
    public static void WritePng(SKImage source, Stream destination, ArtisticFinishOptions options,
        ulong seed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Stream di uscita non scrivibile.", nameof(destination));
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        // La bitmap è l'unica copia a piena risoluzione. Non viene allocato
        // un altro PixelBuffer da width * height * 4 byte.
        using var bitmap = new SKBitmap(new SKImageInfo(source.Width, source.Height,
            SKColorType.Bgra8888, SKAlphaType.Premul));
        if (!bitmap.ReadyToDraw || bitmap.GetPixels() == IntPtr.Zero)
            throw new InvalidOperationException("Allocazione bitmap per finitura a righe fallita.");
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            // Copia 1:1 senza ricampionare il raster originale. L'overload
            // esplicito con SKSamplingOptions evita l'API obsoleta di SkiaSharp.
            canvas.DrawImage(source,
                new SKRect(0, 0, source.Width, source.Height),
                new SKSamplingOptions(SKFilterMode.Nearest));
            canvas.Flush();
        }
        Apply(bitmap, options, seed, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        using SKImage output = SKImage.FromBitmap(bitmap)
            ?? throw new InvalidOperationException("Snapshot del raster rifinito non disponibile.");
        using SKData data = output.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Codifica PNG 8K fallita.");
        data.SaveTo(destination);
    }

    /// <summary>
    /// Percorso riutilizzabile nei test: bitmap BGRA8888 premul/unpremul.
    /// Memoria di lavoro O(larghezza), anche con assorbimento e Bayer 8x8.
    /// </summary>
    internal static void Apply(SKBitmap bitmap, ArtisticFinishOptions options,
        ulong seed, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.IsEnabled) return;
        if (bitmap.ColorType != SKColorType.Bgra8888 ||
            bitmap.AlphaType is not (SKAlphaType.Premul or SKAlphaType.Unpremul))
            throw new ArgumentException("La bitmap deve essere BGRA8888 premoltiplicata o straight.", nameof(bitmap));
        if (!bitmap.ReadyToDraw || bitmap.GetPixels() == IntPtr.Zero)
            throw new ArgumentException("Bitmap non valida.", nameof(bitmap));
        if ((long)bitmap.Width * bitmap.Height > ArtisticFinishOptions.MaximumFinishPixels)
            throw new ArgumentOutOfRangeException(nameof(bitmap), "Superato il budget della finitura 8K.");
        if (bitmap.RowBytes < checked(bitmap.Width * 4))
            throw new ArgumentException("Stride bitmap insufficiente.", nameof(bitmap));

        int width = bitmap.Width, height = bitmap.Height;
        byte[] bytes = new byte[checked(width * 4)];
        uint[]? previous = null;
        uint[] current = new uint[width];
        uint[]? next = height > 1 ? new uint[width] : null;
        uint[] result = new uint[width];
        bool premul = bitmap.AlphaType == SKAlphaType.Premul;
        IntPtr pixels = bitmap.GetPixels();
        ReadRow(pixels, bitmap.RowBytes, 0, bytes, current, premul);
        if (next is not null) ReadRow(pixels, bitmap.RowBytes, 1, bytes, next, premul);

        for (int y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current.AsSpan().CopyTo(result);
            if (options.HasMaterial)
                MaterialPostProcessor.ApplyMaterialRow(result, current, previous, next, options, seed, y);
            ArtisticPostProcessor.ApplyGrainAndDitherRow(result, options, seed, y);
            WriteRow(pixels, bitmap.RowBytes, y, bytes, result, premul);

            if (y + 1 == height) break;
            uint[] reuse = previous ?? new uint[width];
            previous = current;
            current = next!;
            next = y + 2 < height ? reuse : null;
            if (next is not null)
                ReadRow(pixels, bitmap.RowBytes, y + 2, bytes, next, premul);
        }
        bitmap.NotifyPixelsChanged();
    }

    private static void ReadRow(IntPtr pixels, int stride, int y,
        byte[] bytes, uint[] result, bool premul)
    {
        Marshal.Copy(IntPtr.Add(pixels, checked(y * stride)), bytes, 0, bytes.Length);
        for (int x = 0; x < result.Length; x++)
        {
            int offset = 4 * x;
            int b = bytes[offset], g = bytes[offset + 1], r = bytes[offset + 2], a = bytes[offset + 3];
            if (premul && a != 255)
            {
                r = Unpremultiply(r, a);
                g = Unpremultiply(g, a);
                b = Unpremultiply(b, a);
            }
            result[x] = new Color32((byte)a, (byte)r, (byte)g, (byte)b).PackedArgb;
        }
    }

    private static void WriteRow(IntPtr pixels, int stride, int y,
        byte[] bytes, ReadOnlySpan<uint> row, bool premul)
    {
        for (int x = 0; x < row.Length; x++)
        {
            uint argb = row[x];
            int a = (int)(argb >> 24);
            int r = (int)((argb >> 16) & 255);
            int g = (int)((argb >> 8) & 255);
            int b = (int)(argb & 255);
            if (premul && a != 255)
            {
                r = Premultiply(r, a);
                g = Premultiply(g, a);
                b = Premultiply(b, a);
            }
            int offset = x * 4;
            bytes[offset] = (byte)b;
            bytes[offset + 1] = (byte)g;
            bytes[offset + 2] = (byte)r;
            bytes[offset + 3] = (byte)a;
        }
        Marshal.Copy(bytes, 0, IntPtr.Add(pixels, checked(y * stride)), bytes.Length);
    }

    private static int Unpremultiply(int channel, int alpha) =>
        alpha == 0 ? 0 : Math.Min(255, (channel * 255 + alpha / 2) / alpha);

    private static int Premultiply(int channel, int alpha) =>
        (channel * alpha + 127) / 255;
}
