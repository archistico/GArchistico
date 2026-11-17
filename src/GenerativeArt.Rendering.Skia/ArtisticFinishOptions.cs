using System.Globalization;
using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Opzioni di finitura condivise da CLI e Studio. I parametri successivi a M4C.1
/// sono facoltativi: default spenti, ricette v1 e rendering storico invariati.
/// </summary>
public readonly record struct ArtisticFinishOptions(
    double GrainAmount, int GrainSize, string? DitherMode, int DitherLevels,
    string PaperTexture = "none", double PaperStrength = 0,
    double PigmentGranulation = 0, double InkAbsorption = 0)
{
    // Fino a 4K rimane il percorso storico PixelBuffer; oltre 4K i quattro
    // renderer geometrici 8K usano il nuovo postprocess a righe (O(larghezza)).
    // Il limite del generatore viene controllato prima dell'allocazione Skia.
    public const long LegacyFullFrameFinishPixels = 16_777_216;
    public const long MaximumFinishPixels = RasterCanvasBudgetPolicy.EightKMaximumPixels;

    public bool HasMaterial => (PaperTexture != "none" && PaperStrength > 0)
        || PigmentGranulation > 0 || InkAbsorption > 0;

    public bool IsEnabled => GrainAmount > 0 || (DitherMode is not null and not "none") || HasMaterial;

    public static ArtisticFinishOptions FromRecipe(ArtworkRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        IReadOnlyDictionary<string, string> p = recipe.Parameters;
        double Number(string key, double fallback) =>
            p.TryGetValue(key, out string? raw) && raw is not null
                ? double.Parse(raw, CultureInfo.InvariantCulture) : fallback;
        int Integer(string key, int fallback) =>
            p.TryGetValue(key, out string? raw) && raw is not null
                ? int.Parse(raw, CultureInfo.InvariantCulture) : fallback;
        string Choice(string key, string fallback) =>
            p.TryGetValue(key, out string? raw) && raw is not null ? raw : fallback;

        var result = new ArtisticFinishOptions(
            Number("grain-amount", 0), Integer("grain-size", 1),
            Choice("dither-mode", "none"), Integer("dither-levels", 8),
            Choice("paper-texture", "none"), Number("paper-strength", 0),
            Number("pigment-granulation", 0), Number("ink-absorption", 0));
        result.Validate();
        return result;
    }

    public void Validate()
    {
        if (!double.IsFinite(GrainAmount) || GrainAmount < 0 || GrainAmount > 0.35)
            throw new ArgumentOutOfRangeException(nameof(GrainAmount));
        if (GrainSize is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(GrainSize));
        if (DitherMode is not ("none" or "bayer4" or "bayer8"))
            throw new ArgumentException("Dithering sconosciuto.", nameof(DitherMode));
        if (DitherLevels is < 2 or > 32)
            throw new ArgumentOutOfRangeException(nameof(DitherLevels));
        if (PaperTexture is not ("none" or "cotton" or "rough"))
            throw new ArgumentException("Carta sconosciuta.", nameof(PaperTexture));
        if (!double.IsFinite(PaperStrength) || PaperStrength < 0 || PaperStrength > .4)
            throw new ArgumentOutOfRangeException(nameof(PaperStrength));
        if (!double.IsFinite(PigmentGranulation) || PigmentGranulation < 0 || PigmentGranulation > .4)
            throw new ArgumentOutOfRangeException(nameof(PigmentGranulation));
        if (!double.IsFinite(InkAbsorption) || InkAbsorption < 0 || InkAbsorption > .4)
            throw new ArgumentOutOfRangeException(nameof(InkAbsorption));
    }
}
