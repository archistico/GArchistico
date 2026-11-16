namespace GenerativeArt.Core.Rendering;

/// <summary>Tempo derivato dall'indice del frame, non da wall-clock o callback della UI.</summary>
public readonly record struct FrameTime(long FrameIndex, double Seconds, double DeltaSeconds)
{
    public static FrameTime FromFrameIndex(long frameIndex, int framesPerSecond)
    {
        if (frameIndex < 0) throw new ArgumentOutOfRangeException(nameof(frameIndex));
        if (framesPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
        return new FrameTime(frameIndex, frameIndex / (double)framesPerSecond, 1d / framesPerSecond);
    }
}
