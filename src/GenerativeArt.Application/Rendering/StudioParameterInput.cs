using System.Globalization;
using GenerativeArt.Application.Catalog;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Adatta solo l'input dell'editor desktop ai descrittori canonici.
/// Accetta il separatore decimale italiano per i Number, ma conserva in ricetta
/// sempre la notazione invariante e la precisione round-trip del double.
/// </summary>
public static class StudioParameterInput
{
    public static double NumericStep(ArtworkParameterSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        if (spec.Kind == ParameterKind.Integer) return 1;
        if (spec.Kind != ParameterKind.Number) throw new ArgumentException("Parametro non numerico.", nameof(spec));
        double range = double.Parse(spec.Maximum!, CultureInfo.InvariantCulture)
            - double.Parse(spec.Minimum!, CultureInfo.InvariantCulture);
        return range > 0 && double.IsFinite(range)
            ? Math.Pow(10, Math.Clamp(Math.Floor(Math.Log10(range / 1000)), -12, 12)) : 1;
    }

    public static bool TryNormalize(ArtworkParameterSpec spec, string? input, out string canonical)
    {
        ArgumentNullException.ThrowIfNull(spec);
        canonical = string.Empty;
        if (input is null) return false;
        string candidate = input.Trim();
        if (spec.Kind == ParameterKind.Number)
            candidate = candidate.Replace(',', '.');
        try
        {
            canonical = spec.ValidateValue(candidate);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
