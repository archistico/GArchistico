namespace GenerativeArt.Application.Recipes;

/// <summary>Impostazioni temporali serializzabili, indipendenti dai codec video.</summary>
public sealed record RecipeAnimation
{
    public int FrameCount { get; }
    public int FramesPerSecond { get; }

    public RecipeAnimation(int frameCount, int framesPerSecond)
    {
        if (frameCount is < 2 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Fotogrammi ammessi: 2..10000.");
        if (framesPerSecond is < 1 or > 240)
            throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "FPS ammessi: 1..240.");
        FrameCount = frameCount;
        FramesPerSecond = framesPerSecond;
    }
}
