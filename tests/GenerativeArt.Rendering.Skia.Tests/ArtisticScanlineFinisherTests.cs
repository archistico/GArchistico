using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class ArtisticScanlineFinisherTests
{
    // Test piccoli: le immagini 8K effettive si verificano in smoke manuale.
    // Il test di equivalenza esercita il kernel delle righe identico a quello 8K.
    private static SKBitmap Pattern(bool transparent = false)
    {
        var bitmap = new SKBitmap(new SKImageInfo(23, 19, SKColorType.Bgra8888, SKAlphaType.Premul));
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
            {
                byte a = transparent ? (byte)((x + y) % 5 == 0 ? 0 : 128) : (byte)255;
                bitmap.SetPixel(x, y, new SKColor((byte)(35 + (x * 7) % 190),
                    (byte)(22 + (y * 9) % 200), (byte)(50 + (x + y) * 3), a));
            }
        return bitmap;
    }

    [Theory]
    [InlineData("grain")]
    [InlineData("dither")]
    [InlineData("cotton")]
    [InlineData("rough")]
    [InlineData("absorb")]
    [InlineData("combined")]
    public void ScanlineAndLegacyPixelBufferProduceSameOpaquePixels(string mode)
    {
        ArtisticFinishOptions options = mode switch
        {
            "grain" => new ArtisticFinishOptions(.12, 3, "none", 8),
            "dither" => new ArtisticFinishOptions(0, 1, "bayer8", 6),
            "cotton" => new ArtisticFinishOptions(0, 1, "none", 8, "cotton", .18, .10),
            "rough" => new ArtisticFinishOptions(0, 1, "none", 8, "rough", .27, .13),
            "absorb" => new ArtisticFinishOptions(0, 1, "none", 8, InkAbsorption: .25),
            _ => new ArtisticFinishOptions(.12, 2, "bayer4", 5, "rough", .11, .13, .2)
        };
        using SKBitmap bitmap = Pattern();
        PixelBuffer expected = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        ArtisticPostProcessor.Apply(expected, options, 9387);
        ArtisticScanlineFinisher.Apply(bitmap, options, 9387);
        PixelBuffer actual = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        for (int y = 0; y < bitmap.Height; y++)
            Assert.True(expected.GetPackedArgbRow(y).SequenceEqual(actual.GetPackedArgbRow(y)),
                $"{mode}: riga {y}, kernel streaming diverso dal percorso storico");
    }

    [Fact]
    public void TransparentAndSemiTransparentPixelsRetainAlpha()
    {
        using SKBitmap bitmap = Pattern(transparent: true);
        var original = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        var options = new ArtisticFinishOptions(.13, 2, "bayer8", 5,
            "cotton", .1, .12, .2);
        ArtisticScanlineFinisher.Apply(bitmap, options, 300);
        PixelBuffer finished = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                Assert.Equal(original.GetPixel(x, y).A, finished.GetPixel(x, y).A);
    }

    [Fact]
    public void ScanlineFinisherRejectsCancellationBeforeModification()
    {
        using SKBitmap bitmap = Pattern();
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        uint[] before = SkiaPixelAdapter.ToPixelBuffer(bitmap).GetPackedArgbRow(0).ToArray();
        Assert.Throws<OperationCanceledException>(() => ArtisticScanlineFinisher.Apply(bitmap,
            new ArtisticFinishOptions(.1, 2, "bayer4", 8), 42, cancelled.Token));
        Assert.Equal(before, SkiaPixelAdapter.ToPixelBuffer(bitmap).GetPackedArgbRow(0).ToArray());
    }

    [Fact]
    public void FullScanlinePngEncodingProducesDeterministicCorrectSizedImage()
    {
        byte[] Encode()
        {
            using var surface = SKSurface.Create(new SKImageInfo(37, 23,
                SKColorType.Bgra8888, SKAlphaType.Premul))
                ?? throw new InvalidOperationException("Skia surface non disponibile.");
            surface.Canvas.Clear(new SKColor(125, 65, 170, 180));
            using SKImage image = surface.Snapshot();
            using var output = new MemoryStream();
            ArtisticScanlineFinisher.WritePng(image, output,
                new ArtisticFinishOptions(.07, 2, "bayer4", 8,
                    "cotton", .12, .07, .13), 9387, CancellationToken.None);
            return output.ToArray();
        }
        byte[] first = Encode();
        Assert.Equal(first, Encode());
        using SKBitmap image = SKBitmap.Decode(first)
            ?? throw new InvalidOperationException("PNG non decodificabile.");
        Assert.Equal(37, image.Width);
        Assert.Equal(23, image.Height);
        Assert.Equal((byte)180, image.GetPixel(12, 9).Alpha);
    }

    [Fact]
    public void ScanlinePngCopiesSourcePixelsWithoutResampling()
    {
        // Il nuovo overload di SKCanvas.DrawImage deve copiare alla stessa
        // risoluzione, senza introdurre interpolazione tra pixel adiacenti.
        using SKBitmap source = Pattern();
        using SKImage image = SKImage.FromBitmap(source);
        using var output = new MemoryStream();
        ArtisticScanlineFinisher.WritePng(image, output,
            new ArtisticFinishOptions(0, 1, "none", 8), 42, CancellationToken.None);

        using SKBitmap result = SKBitmap.Decode(output.ToArray())
            ?? throw new InvalidOperationException("PNG non decodificabile.");
        Assert.Equal(source.Width, result.Width);
        Assert.Equal(source.Height, result.Height);
        for (int y = 0; y < source.Height; y++)
            for (int x = 0; x < source.Width; x++)
                Assert.Equal(source.GetPixel(x, y), result.GetPixel(x, y));
    }
}
