using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Rendering;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class RenderTests
{
    [Fact]
    public void RejectsNullCanvasAndInvalidFrameRate()
    {
        Assert.Throws<ArgumentNullException>(() => new RenderSettings(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderSettings(new CanvasSize(2, 2), 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderSettings(new CanvasSize(2, 2), 1, 241));
    }

    [Fact]
    public void FrameTimeIsIndependentOfClock()
    {
        var settings = new RenderSettings(new CanvasSize(100, 100), 123, 60);
        var time = settings.TimeAt(120);
        Assert.Equal(120L, time.FrameIndex);
        Assert.Equal(2d, time.Seconds);
        Assert.Equal(1d / 60, time.DeltaSeconds);
    }

    [Fact]
    public void NegativeFrameIndexIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FrameTime.FromFrameIndex(-1, 60));
    }
}
