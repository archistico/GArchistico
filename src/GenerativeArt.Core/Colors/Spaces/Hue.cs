namespace GenerativeArt.Core.Colors.Spaces;

internal static class Hue
{
    internal static double Normalize(double degrees)
    {
        if (!double.IsFinite(degrees)) throw new ArgumentOutOfRangeException(nameof(degrees));
        double normalized = degrees % 360d;
        return normalized < 0d ? normalized + 360d : normalized;
    }

    /// <summary>Percorso angolare minimo: in caso di 180° esatti privilegia quello negativo.</summary>
    internal static double Mix(double fromDegrees, double toDegrees, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0d || amount > 1d)
            throw new ArgumentOutOfRangeException(nameof(amount));
        double from = Normalize(fromDegrees);
        double to = Normalize(toDegrees);
        double delta = (to - from + 540d) % 360d - 180d;
        return Normalize(from + delta * amount);
    }
}
