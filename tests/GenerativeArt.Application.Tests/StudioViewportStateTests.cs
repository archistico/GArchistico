using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioViewportStateTests
{
    [Theory]
    [InlineData(800, 500, 2500, 2000)]
    [InlineData(500, 700, 4096, 2048)]
    [InlineData(350, 240, 800, 600)]
    public void FitShowsWholeImageWithoutChangingAspectRatio(double viewportWidth,
        double viewportHeight, double imageWidth, double imageHeight)
    {
        var view = new StudioViewportState();
        view.Resize(viewportWidth, viewportHeight);
        view.SetImageSize(imageWidth, imageHeight);
        Assert.True(view.FitImage());
        Assert.True(view.OffsetX >= 0);
        Assert.True(view.OffsetY >= 0);
        Assert.True(view.OffsetX + view.ScaledWidth <= viewportWidth + 0.001);
        Assert.True(view.OffsetY + view.ScaledHeight <= viewportHeight + 0.001);
        Assert.Equal(imageWidth / imageHeight, view.ScaledWidth / view.ScaledHeight, 8);
    }

    [Fact]
    public void FitHandlesMissingImageAndCanBeReappliedAfterResize()
    {
        var view = new StudioViewportState();
        Assert.False(view.FitImage());
        view.Resize(600, 400);
        view.SetImageSize(2500, 2000);
        Assert.True(view.FitImage());
        double previous = view.Zoom;
        view.Resize(1000, 700);
        Assert.True(view.FitImage());
        Assert.True(view.Zoom > previous);
    }
    [Fact]
    public void ImageStartsCenteredAndPanningWorksWhenImageIsSmallerThanViewport()
    {
        var view = new StudioViewportState();
        view.Resize(900, 600);
        view.SetImageSize(400, 200);
        Assert.Equal(300, view.OffsetX, 8);
        Assert.Equal(225, view.OffsetY, 8);

        view.Pan(64, -35);
        Assert.Equal(364, view.OffsetX, 8);
        Assert.Equal(190, view.OffsetY, 8);
    }

    [Fact]
    public void WheelZoomKeepsImagePixelUnderPointerFixed()
    {
        var view = new StudioViewportState();
        view.Resize(800, 600);
        view.SetImageSize(1000, 700);
        view.Pan(-60, 35);
        var before = view.ImagePointAt(410, 270);

        Assert.True(view.ZoomAt(1.15 * view.Zoom, 410, 270));
        var after = view.ImagePointAt(410, 270);
        Assert.Equal(before.X, after.X, 8);
        Assert.Equal(before.Y, after.Y, 8);
    }

    [Fact]
    public void ReplacingFrameWithSameSizeKeepsPanAndZoom()
    {
        var view = new StudioViewportState();
        view.Resize(600, 500);
        view.SetImageSize(512, 512);
        view.Pan(100, 30);
        view.ZoomAt(1.25, 220, 200);
        var x = view.OffsetX;
        var y = view.OffsetY;
        var zoom = view.Zoom;
        view.SetImageSize(512, 512);
        Assert.Equal(x, view.OffsetX);
        Assert.Equal(y, view.OffsetY);
        Assert.Equal(zoom, view.Zoom);
    }

    [Fact]
    public void SliderZoomUsesProvidedAnchorAndViewportResizePreservesCenterOffset()
    {
        var view = new StudioViewportState();
        view.Resize(800, 600);
        view.SetImageSize(600, 500);
        view.Pan(20, 30);
        var point = view.ImagePointAt(400, 300);
        view.ZoomAt(1, 400, 300);
        Assert.Equal(point.X, view.ImagePointAt(400, 300).X, 8);
        Assert.Equal(point.Y, view.ImagePointAt(400, 300).Y, 8);

        var offsetX = view.OffsetX;
        var offsetY = view.OffsetY;
        view.Resize(900, 700);
        Assert.Equal(offsetX + 50, view.OffsetX, 8);
        Assert.Equal(offsetY + 50, view.OffsetY, 8);
    }

    [Fact]
    public void ZoomHasHardLimitsAndInvalidInputsDoNotCorruptView()
    {
        var view = new StudioViewportState();
        view.Resize(800, 600);
        view.SetImageSize(400, 300);
        Assert.True(view.ZoomAt(100, 200, 200));
        Assert.Equal(StudioViewportState.MaximumZoom, view.Zoom);
        Assert.False(view.ZoomAt(200, 200, 200));
        Assert.True(view.ZoomAt(0.001, 200, 200));
        Assert.Equal(StudioViewportState.MinimumZoom, view.Zoom);
        Assert.False(view.ZoomAt(double.NaN, 200, 200));
        Assert.False(view.ZoomAt(1, double.PositiveInfinity, 200));
        Assert.Equal(StudioViewportState.MinimumZoom, view.Zoom);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.Resize(-1, 600));
    }

    [Fact]
    public void DragCannotHideAllOfImageOutsideViewport()
    {
        var view = new StudioViewportState();
        view.Resize(600, 400);
        view.SetImageSize(300, 300);
        view.Pan(100_000, -100_000);
        Assert.True(view.OffsetX < 600);
        Assert.True(view.OffsetX + view.ScaledWidth > 0);
        Assert.True(view.OffsetY < 400);
        Assert.True(view.OffsetY + view.ScaledHeight > 0);
    }
}
