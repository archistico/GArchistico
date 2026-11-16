using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class PixelBufferTests
{
    private static readonly Color32 Red = Color32.Opaque(255, 0, 0);
    private static readonly Color32 Blue = Color32.Opaque(0, 0, 255);

    private static int ReadRowLength(PixelBuffer pixels, int y) => pixels.GetPackedArgbRow(y).Length;
    private static int WritableRowLength(PixelBuffer pixels, int y) => pixels.GetWritablePackedArgbRow(y).Length;

    [Fact]
    public void IsManagedStraightArgbWithExplicitPixelStride()
    {
        var pixels = new PixelBuffer(new CanvasSize(3, 2), stridePixels: 5);

        Assert.Equal(3, pixels.Width);
        Assert.Equal(2, pixels.Height);
        Assert.Equal(5, pixels.StridePixels);
        Assert.Equal(40L, pixels.AllocatedBytes);
        Assert.Equal(PixelStorageFormat.PackedArgb32Straight, pixels.StorageFormat);
        Assert.Equal(Color32.Transparent, pixels.GetPixel(2, 1));
    }

    [Fact]
    public void RejectsNullSizeOrStrideBelowWidth()
    {
        Assert.Throws<ArgumentNullException>(() => new PixelBuffer(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelBuffer(new CanvasSize(3, 2), 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelBuffer(new CanvasSize(3, 2), -1));
    }

    [Fact]
    public void LimitsAllocationIncludingPaddingWithoutOverflow()
    {
        var size = new CanvasSize(2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelBuffer(size, int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelBuffer(size, 22_369_622));
    }

    [Fact]
    public void DistinctRowsDoNotAliasEvenWithNegativeX()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2), 4);
        pixels.SetPixel(1, 0, Blue);

        Assert.False(pixels.TrySetPixel(-1, 1, Red));
        Assert.False(pixels.TrySetPixel(2, 0, Red));
        Assert.False(pixels.TrySetPixel(0, -1, Red));
        Assert.Equal(Blue, pixels.GetPixel(1, 0));
        Assert.Equal(Color32.Transparent, pixels.GetPixel(0, 1));
    }

    [Fact]
    public void StrictAccessRejectsOutsideCoordinates()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => pixels.SetPixel(-1, 1, Red));
        Assert.Throws<ArgumentOutOfRangeException>(() => pixels.SetPixel(0, 2, Red));
        Assert.Throws<ArgumentOutOfRangeException>(() => pixels.GetPixel(2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ReadRowLength(pixels, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WritableRowLength(pixels, 2));
    }

    [Fact]
    public void TryGetReturnsFalseAndTransparentForInvalidCoordinates()
    {
        var pixels = new PixelBuffer(new CanvasSize(1, 1));
        Assert.False(pixels.TryGetPixel(-1, 0, out var color));
        Assert.Equal(Color32.Transparent, color);
        Assert.True(pixels.TryGetPixel(0, 0, out color));
        Assert.Equal(Color32.Transparent, color);
    }

    [Fact]
    public void PreservesStraightAlphaIncludingHiddenRgb()
    {
        var pixels = new PixelBuffer(new CanvasSize(1, 1));
        var original = new Color32(0, 200, 100, 50);
        pixels.SetPixel(0, 0, original);
        Assert.Equal(original, pixels.GetPixel(0, 0));
        Assert.Equal(0x00C86432u, pixels.GetPackedArgbRow(0)[0]);
    }

    [Fact]
    public void ExposesOnlyLogicalRowLengthAndAllowsExplicitPackedWrites()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2), 4);
        Span<uint> row = pixels.GetWritablePackedArgbRow(1);
        Assert.Equal(2, row.Length);
        row[0] = 0x80402010u;
        row[1] = Blue.PackedArgb;
        Assert.Equal(new Color32(128, 64, 32, 16), pixels.GetPixel(0, 1));
        Assert.Equal(Blue, pixels.GetPixel(1, 1));
        Assert.Equal(Color32.Transparent, pixels.GetPixel(0, 0));
    }

    [Fact]
    public void ClearFillsLogicalImageWithoutChangingAlphaSemantics()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 3), 5);
        var color = new Color32(64, 255, 128, 17);
        pixels.Clear(color);
        for (int y = 0; y < pixels.Height; y++)
            for (int x = 0; x < pixels.Width; x++)
                Assert.Equal(color, pixels.GetPixel(x, y));
    }

    [Fact]
    public void RectangleClipsNegativeOriginWithoutCorruptingPreviousRow()
    {
        var pixels = new PixelBuffer(new CanvasSize(3, 3));
        pixels.Clear(Blue);
        Assert.Equal(1L, pixels.FillRectangle(new PixelRectangle(-1, -1, 2, 2), Red));
        Assert.Equal(Red, pixels.GetPixel(0, 0));
        Assert.Equal(Blue, pixels.GetPixel(1, 0));
        Assert.Equal(Blue, pixels.GetPixel(0, 1));
        Assert.Equal(Blue, pixels.GetPixel(2, 2));
    }

    [Fact]
    public void RectangleHandlesHugeCoordinatesWithoutOverflow()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2));
        Assert.Equal(0L, pixels.FillRectangle(new PixelRectangle(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue), Red));
        Assert.Equal(0L, pixels.FillRectangle(new PixelRectangle(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue), Red));
        Assert.Equal(4L, pixels.FillRectangle(new PixelRectangle(0, 0, int.MaxValue, int.MaxValue), Blue));
        Assert.Equal(Blue, pixels.GetPixel(1, 1));
    }

    [Fact]
    public void EmptyRectangleIsNoOpAndNegativeDimensionsAreRejected()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2));
        Assert.True(default(PixelRectangle).IsEmpty);
        Assert.Equal(0L, pixels.FillRectangle(new PixelRectangle(0, 0, 0, 2), Red));
        Assert.Equal(0L, pixels.FillRectangle(new PixelRectangle(0, 0, 2, 0), Red));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelRectangle(0, 0, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelRectangle(0, 0, 1, -1));
    }

    [Fact]
    public void CopyPackedArgbToRespectsDestinationStrideAndPreservesPadding()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2), 4);
        pixels.SetPixel(0, 0, Red);
        pixels.SetPixel(1, 1, Blue);

        uint[] target = { 77, 77, 77, 77, 77, 77 };
        pixels.CopyPackedArgbTo(target, 3);
        Assert.Equal(new[]
        {
            Red.PackedArgb, Color32.Transparent.PackedArgb, 77u,
            Color32.Transparent.PackedArgb, Blue.PackedArgb, 77u
        }, target);
    }

    [Fact]
    public void CopyFailsBeforeMutationIfDestinationInsufficient()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2));
        pixels.Clear(Red);
        uint[] shortTarget = { 77, 77, 77, 77 };
        Assert.Throws<ArgumentException>(() => pixels.CopyPackedArgbTo(shortTarget, 3));
        Assert.Equal(new uint[] { 77, 77, 77, 77 }, shortTarget);
        Assert.Throws<ArgumentOutOfRangeException>(() => pixels.CopyPackedArgbTo(new uint[4], 1));
    }

    [Fact]
    public void CopySupportsMinimalLastRowLengthWithoutTrailingPadding()
    {
        var pixels = new PixelBuffer(new CanvasSize(2, 2));
        pixels.Clear(Blue);
        uint[] target = new uint[5]; // (2-1)*stride3+width2
        pixels.CopyPackedArgbTo(target, 3);
        Assert.Equal(Blue.PackedArgb, target[0]);
        Assert.Equal(0u, target[2]);
        Assert.Equal(Blue.PackedArgb, target[4]);
    }
}
