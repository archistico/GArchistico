using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class DepthLayerTests
{
    private static byte[] Png(SkiaRasterSurface s)
    { using var stream = new MemoryStream(); s.WritePng(stream); return stream.ToArray(); }

    [Fact]
    public void NeutralLayerHasExactlyTheSamePngAsDirectDrawing()
    {
        using var direct = new SkiaRasterSurface(new CanvasSize(64, 64));
        using var layered = new SkiaRasterSurface(direct.Size);
        void Draw(SkiaRasterSurface s) => s.DrawCircle(30, 30, 12, new Color32(150, 40, 180, 200), fill: true);
        Assert.False(default(LayerEffects).IsEnabled);
        Draw(direct); layered.DrawLayer(Draw, default);
        Assert.Equal(Png(direct), Png(layered));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GaussianBlurSpreadsAlphaWithoutDarkFringesAtBothRasterScales(int scale)
    {
        using var s = new SkiaRasterSurface(new CanvasSize(96, 96), scale);
        s.DrawLayer(layer => layer.DrawCircle(48, 48, 10, Color32.Opaque(240, 120, 60), fill: true), new LayerEffects(Blur: 4));
        using var bitmap = SKBitmap.Decode(Png(s));
        SKColor center = bitmap.GetPixel(48, 48), halo = bitmap.GetPixel(62, 48);
        Assert.InRange(center.Alpha, (byte)230, (byte)255);
        Assert.InRange(halo.Alpha, (byte)8, (byte)65);
        Assert.InRange(halo.Red, (byte)230, (byte)250);
        Assert.InRange(halo.Green, (byte)110, (byte)130);
        Assert.Equal((byte)0, bitmap.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void ShadowIsOffsetBehindSourceAndDoesNotTintIt()
    {
        using var s = new SkiaRasterSurface(new CanvasSize(96, 64));
        s.DrawLayer(layer => layer.DrawCircle(25, 30, 8, Color32.Opaque(255, 80, 40), fill: true),
            new LayerEffects(ShadowOpacity: .6, ShadowX: 20, ShadowSoftness: 2));
        using var image = SKBitmap.Decode(Png(s));
        Assert.Equal(new SKColor(255, 80, 40), image.GetPixel(25, 30));
        Assert.InRange(image.GetPixel(45, 30).Alpha, (byte)140, (byte)160);
        Assert.Equal((byte)0, image.GetPixel(5, 30).Alpha);
    }

    [Fact]
    public void GroupOpacityIsAppliedOnceToOverlappingShapes()
    {
        using var s = new SkiaRasterSurface(new CanvasSize(64, 64));
        s.DrawLayer(layer =>
        {
            layer.DrawCircle(25, 32, 15, Color32.Opaque(255, 255, 255), fill: true);
            layer.DrawCircle(40, 32, 15, Color32.Opaque(255, 255, 255), fill: true);
        }, new LayerEffects(Opacity: .5));
        using var image = SKBitmap.Decode(Png(s));
        Assert.InRange(image.GetPixel(32, 32).Alpha, (byte)127, (byte)128);
        Assert.Equal(image.GetPixel(18, 32).Alpha, image.GetPixel(32, 32).Alpha);
    }

    [Fact]
    public void LayerRestoresCanvasWhenCallbackFailsOrCancels()
    {
        using var s = new SkiaRasterSurface(new CanvasSize(64, 64));
        Assert.Throws<InvalidOperationException>(() => s.DrawLayer(_ => throw new InvalidOperationException(), new LayerEffects(Blur: 2)));
        using var cts = new CancellationTokenSource();
        Assert.Throws<OperationCanceledException>(() => s.DrawLayer(_ => cts.Cancel(), new LayerEffects(Blur: 2), cts.Token));
        s.DrawCircle(32, 32, 10, Color32.Opaque(80, 150, 200), fill: true);
        using var image = SKBitmap.Decode(Png(s));
        Assert.Equal(new SKColor(80, 150, 200), image.GetPixel(32, 32));
        Assert.Equal((byte)0, image.GetPixel(45, 32).Alpha);
    }

    [Fact]
    public void InvalidEffectsDoNotInvokeDrawing()
    {
        using var s = new SkiaRasterSurface(new CanvasSize(64, 64));
        bool drawn = false;
        Assert.Throws<ArgumentOutOfRangeException>(() => s.DrawLayer(_ => drawn = true, new LayerEffects(Blur: double.NaN)));
        Assert.Throws<ArgumentOutOfRangeException>(() => s.DrawLayer(_ => drawn = true, new LayerEffects(ShadowOpacity: 2)));
        Assert.False(drawn);
    }

    [Fact]
    public void AperturePreservesPreviousLayersInsideItsHole()
    {
        using var s = new SkiaRasterSurface(new CanvasSize(64, 64));
        s.Clear(Color32.Opaque(30, 100, 200));
        s.DrawLayer(layer => layer.DrawAperture([
            new(20, 20), new(44, 20), new(44, 44), new(20, 44), new(20, 20)], Color32.Opaque(240, 220, 190)),
            new LayerEffects(ShadowOpacity: .4, ShadowX: 3, ShadowSoftness: 2));
        using var image = SKBitmap.Decode(Png(s));
        Assert.Equal(new SKColor(30, 100, 200), image.GetPixel(32, 32));
        Assert.Equal(new SKColor(240, 220, 190), image.GetPixel(4, 4));
        Assert.Equal((byte)255, image.GetPixel(24, 32).Alpha);
        Assert.True(image.GetPixel(24, 32).Blue < 200); // ombra all'interno dell'apertura
    }
}
