namespace GenerativeArt.Algorithms.Geometry;

/// <summary>Copertura analitica di un bordo o tratto locale, convoluti con un pixel box.</summary>
public static class SdfPixelCoverage
{
    public static float Fill(float distance, float footprint)
    {
        Validate(distance, footprint);
        return Math.Clamp(.5f - distance / footprint, 0, 1);
    }

    public static float Stroke(float distance, float width, float footprint)
    {
        Validate(distance, footprint);
        if (!float.IsFinite(width) || width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        float half = footprint * .5f, radius = width * .5f;
        return Math.Clamp((Math.Min(distance + half, radius) - Math.Max(distance - half, -radius)) / footprint, 0, 1);
    }

    private static void Validate(float distance, float footprint)
    {
        if (!float.IsFinite(distance) || !float.IsFinite(footprint) || footprint <= 0)
            throw new ArgumentOutOfRangeException(nameof(footprint));
    }
}
