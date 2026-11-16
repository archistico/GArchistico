using GenerativeArt.Core.Rendering;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class LoopFrameTests
{
    [Fact]
    public void TimelineDoesNotDuplicateFirstFrame()
    {
        LoopFrame start = LoopFrame.At(0, 24, 24);
        LoopFrame last = LoopFrame.At(23, 24, 24);
        Assert.Equal(0d, start.Phase);
        Assert.Equal(23d / 24d, last.Phase);
        Assert.True(last.Phase < 1d);
        Assert.Equal(23d / 24d, last.Seconds);
    }

    [Fact]
    public void PhaseIsDeterministicAndUniform()
    {
        for (int i = 0; i < 60; i++)
            Assert.InRange(Math.Abs(LoopFrame.At(i, 60, 30).Phase - i / 60d), 0, 1e-15);
    }

    [Fact]
    public void RejectsInvalidTimeline()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoopFrame.At(-1, 24, 24));
        Assert.Throws<ArgumentOutOfRangeException>(() => LoopFrame.At(24, 24, 24));
        Assert.Throws<ArgumentOutOfRangeException>(() => LoopFrame.At(0, 1, 24));
        Assert.Throws<ArgumentOutOfRangeException>(() => LoopFrame.At(0, 24, 0));
    }
}
