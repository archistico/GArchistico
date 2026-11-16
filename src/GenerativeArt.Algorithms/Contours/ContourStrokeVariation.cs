using System.Numerics;

namespace GenerativeArt.Algorithms.Contours;

/// <summary>
/// Rifinitura deterministica di isolinee: tratto rigoroso, vibrazione organica
/// continua o addolcimento geometrico Chaikin. Non dipende da SkiaSharp.
/// Le coordinate in entrata sono celle; roughnessPixels è espressa in pixel
/// del rendering finale e non altera il campo scalare sottostante.
/// </summary>
public static class ContourStrokeVariation
{
    public static IReadOnlyList<Vector2> Apply(IReadOnlyList<Vector2> points, bool closed,
        string style, float roughnessPixels, float scaleX, float scaleY, ulong seed)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (style is not ("precise" or "handdrawn" or "soft"))
            throw new ArgumentException("Stile del tratto sconosciuto.", nameof(style));
        if (!float.IsFinite(roughnessPixels) || roughnessPixels < 0 ||
            !float.IsFinite(scaleX) || !float.IsFinite(scaleY) || scaleX <= 0 || scaleY <= 0)
            throw new ArgumentOutOfRangeException(nameof(roughnessPixels));
        if (points.Count < 2 || style == "precise" || (style == "handdrawn" && roughnessPixels == 0))
            return points;
        foreach (Vector2 p in points)
            if (!float.IsFinite(p.X) || !float.IsFinite(p.Y))
                throw new ArgumentException("Coordinate non finite.", nameof(points));

        if (style == "handdrawn")
        {
            // Variazione continua nello spazio: un vertice condiviso riceve
            // sempre lo stesso spostamento. Non usa un RNG di frame.
            float phase = (float)(seed % 9973UL) * .0137f;
            float magnitude = roughnessPixels / MathF.Min(scaleX, scaleY);
            var result = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                float dx = MathF.Sin(p.X * .29f + p.Y * .13f + phase) *
                           MathF.Cos(p.Y * .21f - phase * .7f);
                float dy = MathF.Sin(p.Y * .33f - p.X * .19f + phase * .83f) *
                           MathF.Cos(p.X * .17f + phase * .34f);
                result[i] = p + new Vector2(dx, dy) * magnitude;
            }
            return result;
        }

        // Chaikin: inserisce due punti per segmento. Le curve chiuse restano
        // chiuse, le aperte mantengono gli estremi originali.
        int uniqueCount = closed && points.Count > 2 && points[0] == points[^1]
            ? points.Count - 1 : points.Count;
        if (uniqueCount < 2) return points;
        var smooth = new List<Vector2>(uniqueCount * 2 + 2);
        if (!closed) smooth.Add(points[0]);
        int segments = closed ? uniqueCount : uniqueCount - 1;
        for (int i = 0; i < segments; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % uniqueCount];
            smooth.Add(.75f * a + .25f * b);
            smooth.Add(.25f * a + .75f * b);
        }
        if (closed) smooth.Add(smooth[0]);
        else smooth.Add(points[uniqueCount - 1]);
        return smooth;
    }
}
