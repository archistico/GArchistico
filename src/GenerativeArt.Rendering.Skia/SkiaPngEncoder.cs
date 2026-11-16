using GenerativeArt.Core.Pixels;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Codifica PNG headless: lo stream è del chiamante e resta aperto.</summary>
public static class SkiaPngEncoder
{
    public static void Write(PixelBuffer pixels, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Lo stream di destinazione deve essere scrivibile.", nameof(destination));

        using SKBitmap bitmap = SkiaPixelAdapter.ToBitmap(pixels);
        using SKImage image = SKImage.FromBitmap(bitmap)
            ?? throw new InvalidOperationException("Impossibile ottenere uno snapshot Skia.");
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Codifica PNG fallita.");
        data.SaveTo(destination);
    }

    public static void Save(PixelBuffer pixels, string path)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Write(pixels, stream);
    }
}
