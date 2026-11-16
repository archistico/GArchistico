using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Core.Rendering;

/// <summary>Parametri minimi immutabili della sessione generativa.</summary>
public sealed record RenderSettings
{
    public CanvasSize Size { get; }
    public long Seed { get; }
    public int FramesPerSecond { get; }

    public RenderSettings(CanvasSize size, long seed, int framesPerSecond = 60)
    {
        ArgumentNullException.ThrowIfNull(size);
        if (framesPerSecond is < 1 or > 240)
            throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "FPS ammessi: 1..240.");
        Size = size;
        Seed = seed;
        FramesPerSecond = framesPerSecond;
    }

    public FrameTime TimeAt(long frameIndex) => FrameTime.FromFrameIndex(frameIndex, FramesPerSecond);
}
