namespace GenerativeArt.Application.Rendering;

/// <summary>
/// La ricetta è Full HD per impostazione iniziale, ma la preview conserva
/// un budget ridotto. Le animazioni usano 16 frame per rispettare il tetto
/// cumulativo di 40 milioni di pixel a 1920×1080.
/// </summary>
public static class StudioRecipeDefaults
{
    public const int Width = 1920;
    public const int Height = 1080;
    public const int FrameCount = 16;
    public const int FramesPerSecond = 18;
}
