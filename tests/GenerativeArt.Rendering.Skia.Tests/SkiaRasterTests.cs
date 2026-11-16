using System.Numerics;
using System.Runtime.InteropServices;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class SkiaRasterTests
{
    [Fact]
    public void BitmapHasExplicitBgraUnpremulFormatAndCorrectBytes()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 1));
        pixels.SetPixel(0, 0, new Color32(128, 17, 34, 51));
        pixels.SetPixel(1, 0, new Color32(0, 200, 100, 50));
        using SKBitmap bitmap = SkiaPixelAdapter.ToBitmap(pixels);
        Assert.Equal(SKColorType.Bgra8888, bitmap.ColorType);
        Assert.Equal(SKAlphaType.Unpremul, bitmap.AlphaType);
        var bytes = new byte[8];
        Marshal.Copy(bitmap.GetPixels(), bytes, 0, 8);
        Assert.Equal(new byte[] { 51, 34, 17, 128, 50, 100, 200, 0 }, bytes);
    }

    [Fact]
    public void BitmapRoundTripKeepsStrideAndStraightAlpha()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2), stridePixels: 5);
        pixels.SetPixel(0, 0, new Color32(128, 17, 34, 51));
        pixels.SetPixel(1, 0, Color32.Opaque(5, 6, 7));
        pixels.SetPixel(0, 1, Color32.Transparent);
        pixels.SetPixel(1, 1, Color32.Opaque(200, 100, 50));
        using SKBitmap bitmap = SkiaPixelAdapter.ToBitmap(pixels);
        PixelBuffer restored = SkiaPixelAdapter.ToPixelBuffer(bitmap);
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
                Assert.Equal(pixels.GetPixel(x, y), restored.GetPixel(x, y));
    }

    [Fact]
    public void FromBitmapRejectsNullOrInvalidNativeBitmap()
    {
        Assert.Throws<ArgumentNullException>(() => SkiaPixelAdapter.ToPixelBuffer(null!));
        using var empty = new SKBitmap();
        Assert.Throws<ArgumentException>(() => SkiaPixelAdapter.ToPixelBuffer(empty));
    }

    [Fact]
    public void FromBitmapRejectsDisposedBitmapWithoutReadingNativePixels()
    {
        var pixels = new PixelBuffer(new CanvasSize(1, 1));
        SKBitmap bitmap = SkiaPixelAdapter.ToBitmap(pixels);
        bitmap.Dispose();
        Assert.Throws<ArgumentException>(() => SkiaPixelAdapter.ToPixelBuffer(bitmap));
    }

    [Fact]
    public void ToBitmapRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => SkiaPixelAdapter.ToBitmap(null!));
    }

    [Fact]
    public void PngEncoderProducesDecodableCorrectDimensionsAndOpaquePixels()
    {
        var pixels = new PixelBuffer(new CanvasSize(3, 2), stridePixels: 5);
        pixels.Clear(Color32.Opaque(14, 35, 90));
        pixels.SetPixel(1, 1, Color32.Opaque(200, 18, 70));
        using var png = new MemoryStream();
        SkiaPngEncoder.Write(pixels, png);
        byte[] encoded = png.ToArray();
        Assert.True(encoded.Length > 8);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, encoded[..8]);
        using SKBitmap decoded = SKBitmap.Decode(encoded)
            ?? throw new InvalidOperationException("Il PNG non è decodificabile.");
        Assert.Equal(3, decoded.Width);
        Assert.Equal(2, decoded.Height);
        SKColor c = decoded.GetPixel(1, 1);
        Assert.Equal((byte)255, c.Alpha);
        Assert.Equal((byte)200, c.Red);
        Assert.Equal((byte)18, c.Green);
        Assert.Equal((byte)70, c.Blue);
    }

    [Fact]
    public void PngEncodingKeepsTranslucentAlpha()
    {
        var pixels = new PixelBuffer(new CanvasSize(1, 1));
        pixels.SetPixel(0, 0, new Color32(128, 200, 100, 50));
        using var png = new MemoryStream();
        SkiaPngEncoder.Write(pixels, png);
        using SKBitmap decoded = SKBitmap.Decode(png.ToArray())
            ?? throw new InvalidOperationException();
        SKColor color = decoded.GetPixel(0, 0);
        Assert.Equal((byte)128, color.Alpha);
        // I valori RGB possono variare di 1 per il passaggio premul/unpremul nativo.
        Assert.InRange(color.Red, (byte)198, (byte)202);
    }

    [Fact]
    public void PngWriterLeavesStreamOpenAndValidatesInput()
    {
        var pixels = new PixelBuffer(new CanvasSize(1, 1));
        Assert.Throws<ArgumentNullException>(() => SkiaPngEncoder.Write(null!, new MemoryStream()));
        Assert.Throws<ArgumentNullException>(() => SkiaPngEncoder.Write(pixels, null!));
        using var readOnly = new MemoryStream(Array.Empty<byte>(), writable: false);
        Assert.Throws<ArgumentException>(() => SkiaPngEncoder.Write(pixels, readOnly));
        using var stream = new MemoryStream();
        SkiaPngEncoder.Write(pixels, stream);
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public void RasterSurfaceDrawsPixelsAndPrimitivesToPng()
    {
        var pixels = new PixelBuffer(new CanvasSize(8, 8));
        pixels.Clear(Color32.Opaque(12, 30, 50));
        using var surface = new SkiaRasterSurface(new CanvasSize(8, 8));
        surface.DrawPixels(pixels);
        surface.DrawCircle(4, 4, 2, Color32.Opaque(240, 10, 20), fill: true);
        using var stream = new MemoryStream();
        surface.WritePng(stream);
        using SKBitmap bitmap = SKBitmap.Decode(stream.ToArray())
            ?? throw new InvalidOperationException("PNG vuoto.");
        Assert.Equal((byte)240, bitmap.GetPixel(4, 4).Red);
        Assert.Equal((byte)12, bitmap.GetPixel(0, 0).Red);
        Assert.Equal((byte)255, bitmap.GetPixel(4, 4).Alpha);
    }

    [Fact]
    public void SurfaceDrawPolylineConnectsSegmentsAndChecksCoordinates()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(20, 20));
        surface.Clear(Color32.Opaque(0, 0, 0));
        surface.DrawPolyline([new Vector2(2, 2), new Vector2(10, 2), new Vector2(10, 10)],
            1, 1, Color32.Opaque(255, 200, 100), 2);
        using var output = new MemoryStream();
        surface.WritePng(output);
        using SKBitmap bitmap = SKBitmap.Decode(output.ToArray()) ?? throw new InvalidOperationException();
        Assert.Equal((byte)255, bitmap.GetPixel(8, 2).Red);
        Assert.Equal((byte)255, bitmap.GetPixel(10, 8).Red);
        Assert.Equal((byte)0, bitmap.GetPixel(0, 0).Red);
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.DrawPolyline(
            [new Vector2(float.NaN, 0), new Vector2(1, 1)], 1, 1, Color32.Opaque(255, 0, 0)));
    }

    [Fact]
    public void SurfaceDrawPolylineScalesPointsWithoutClosingPath()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(32, 32));
        surface.Clear(Color32.Opaque(0, 0, 0));
        surface.DrawPolyline([new Vector2(2, 2), new Vector2(10, 2), new Vector2(10, 10)],
            2, 2, Color32.Opaque(255, 0, 0), 2);
        using var output = new MemoryStream();
        surface.WritePng(output);
        using SKBitmap bitmap = SKBitmap.Decode(output.ToArray()) ?? throw new InvalidOperationException();

        Assert.Equal((byte)255, bitmap.GetPixel(12, 4).Red);  // segmento orizzontale
        Assert.Equal((byte)255, bitmap.GetPixel(20, 12).Red); // segmento verticale
        Assert.Equal((byte)0, bitmap.GetPixel(12, 12).Red);   // niente chiusura diagonale
        Assert.Equal((byte)0, bitmap.GetPixel(6, 2).Red);     // niente segmento non scalato
    }

    [Fact]
    public void SurfaceCanBeDisposedTwiceAndRefusesFurtherCalls()
    {
        var surface = new SkiaRasterSurface(new CanvasSize(2, 2));
        surface.Dispose();
        surface.Dispose();
        Assert.Throws<ObjectDisposedException>(() => surface.Clear(Color32.Transparent));
        Assert.Throws<ObjectDisposedException>(() => surface.DrawCircle(1, 1, 1, Color32.Transparent));
        Assert.Throws<ObjectDisposedException>(() => surface.WritePng(new MemoryStream()));
    }

    [Fact]
    public void SurfaceRejectsInvalidCoordinatesOrRadius()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(2, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            surface.DrawCircle(float.NaN, 1, 1, Color32.Transparent));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            surface.DrawCircle(1, 1, 0, Color32.Transparent));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            surface.DrawLine(0, 0, 1, float.PositiveInfinity, Color32.Transparent));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            surface.DrawLine(0, 0, 1, 1, Color32.Transparent, 0));
    }

    [Fact]
    public void SurfaceDrawsLineAndProducesPng()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(20, 20));
        surface.Clear(Color32.Opaque(0, 0, 0));
        surface.DrawLine(0, 10, 19, 10, Color32.Opaque(255, 255, 255), 3);
        using var output = new MemoryStream();
        surface.WritePng(output);
        using SKBitmap decoded = SKBitmap.Decode(output.ToArray())
            ?? throw new InvalidOperationException();
        Assert.Equal((byte)255, decoded.GetPixel(10, 10).Red);
    }
    [Fact]
    public void FilledPolygonIsClosedAndRendersAntialiasedStroke()
    {
        using var surface = new SkiaRasterSurface(new CanvasSize(40, 40));
        surface.Clear(Color32.Opaque(0, 0, 0));
        surface.DrawPolygon(
            [new Vector2(5, 5), new Vector2(35, 5), new Vector2(35, 35), new Vector2(5, 35)],
            Color32.Opaque(200, 90, 40), Color32.Opaque(20, 200, 210), 2);
        using var output = new MemoryStream();
        surface.WritePng(output);
        using SKBitmap decoded = SKBitmap.Decode(output.ToArray()) ??
            throw new InvalidOperationException("Il PNG della cella non è decodificabile.");
        SKColor center = decoded.GetPixel(20, 20);
        Assert.Equal((byte)200, center.Red);
        Assert.Equal((byte)90, center.Green);
        Assert.Equal((byte)40, center.Blue);
        Assert.Equal((byte)0, decoded.GetPixel(2, 2).Red);
        Assert.Throws<ArgumentException>(() => surface.DrawPolygon(
            [Vector2.Zero, Vector2.One], Color32.Opaque(1, 2, 3), Color32.Opaque(3, 2, 1)));
    }

}
