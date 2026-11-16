namespace GenerativeArt.Core.Rendering;

/// <summary>
/// Fotogramma di un ciclo: la fase è [0,1), senza duplicare il primo frame alla fine.
/// La funzione campionata sulla fase deve essere periodica per avere un loop continuo.
/// </summary>
public readonly record struct LoopFrame
{
    public int Index { get; }
    public int FrameCount { get; }
    public int FramesPerSecond { get; }
    public double Phase { get; }
    public double Seconds => Index / (double)FramesPerSecond;
    public double AngleRadians => Phase * Math.Tau;

    private LoopFrame(int index, int frameCount, int framesPerSecond)
    {
        Index = index;
        FrameCount = frameCount;
        FramesPerSecond = framesPerSecond;
        Phase = index / (double)frameCount;
    }

    public static LoopFrame At(int index, int frameCount, int framesPerSecond)
    {
        if (frameCount < 2 || frameCount > 10_000)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Frame ammessi: 2..10000.");
        if (index < 0 || index >= frameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (framesPerSecond < 1 || framesPerSecond > 240)
            throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
        return new(index, frameCount, framesPerSecond);
    }
}
