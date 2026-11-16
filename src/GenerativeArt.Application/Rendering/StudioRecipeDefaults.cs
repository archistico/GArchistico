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

    /// <summary>Impostazioni per nuovi lavori dello Studio; le ricette importate conservano i loro valori.</summary>
    public static IReadOnlyDictionary<string, string> ParametersFor(string artworkId) => artworkId switch
    {
        "parametric-curves" => new Dictionary<string, string>
        {
            ["style"] = "woven", ["layers"] = "28", ["samples"] = "4800", ["frequency-x"] = "7",
            ["frequency-y"] = "5", ["phase"] = "1.1", ["twist"] = "45", ["layer-scale"] = "0.55",
            ["line-width"] = "0.9", ["orbit-width"] = "0", ["background-color"] = "#10192B",
            ["curve-color"] = "#B07DCACB", ["orbit-color"] = "#D0E8C88E"
        },
        "noise-flow" => new Dictionary<string, string>
        {
            ["field"] = "curl", ["frequency"] = "0.003", ["start-layout"] = "grid", ["lines"] = "1200",
            ["steps"] = "420", ["step-size"] = "2.6", ["opacity"] = "0.65", ["line-width"] = "0.9"
        },
        "spaced-streamlines" => new Dictionary<string, string>
        {
            ["max-lines"] = "3000", ["separation"] = "6.5", ["line-width"] = "0.8",
            ["frequency"] = "0.0035", ["max-steps"] = "300"
        },
        "plasma-noise" => new Dictionary<string, string>
        {
            ["pattern"] = "marble", ["frequency"] = "0.0028", ["octaves"] = "5", ["persistence"] = "0.48",
            ["bands"] = "6", ["contrast"] = "1.3", ["gamma"] = "0.9", ["contour-strength"] = "0.12"
        },
        "looping-flow" => new Dictionary<string, string>
        {
            ["lines"] = "220", ["steps"] = "240", ["frequency"] = "0.004", ["step-size"] = "2.5",
            ["line-width"] = "1.0", ["opacity"] = "0.65"
        },
        _ => new Dictionary<string, string>()
    };
}
