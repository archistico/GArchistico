using System.Numerics;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>Campione su una spina pennellata: coordinate, normale, arc-length e semilarghezza.</summary>
public readonly record struct CincinnusSpineSample(Vector2 Position, Vector2 Normal, float Arc, float HalfWidth);

/// <summary>
/// Pennellata C3 immutabile. LandingOffset(t) fornisce la forma della testata
/// nell'intervallo trasversale [-1,1], con 0 lungo la spina all'inizio.
/// </summary>
public sealed class CincinnusBrushStroke
{
    private readonly IReadOnlyList<CincinnusSpineSample> _spine;
    private readonly float[] _landing;

    internal CincinnusBrushStroke(CincinnusSpineSample[] spine, float[] landing,
        float bend, ulong seed, float dryExponent)
    {
        _spine = Array.AsReadOnly(spine);
        _landing = landing;
        Bend = bend;
        Seed = seed;
        DryExponent = dryExponent;
    }

    public IReadOnlyList<CincinnusSpineSample> Spine => _spine;
    public float Bend { get; }
    public ulong Seed { get; }
    public float DryExponent { get; }
    public float Length => _spine[^1].Arc;

    /// <summary>Carico di pigmento 1-t^DryExponent: il tratto si esaurisce verso la coda.</summary>
    public float Load(float t)
    {
        if (!float.IsFinite(t)) throw new ArgumentOutOfRangeException(nameof(t));
        return 1f - MathF.Pow(Math.Clamp(t, 0f, 1f), DryExponent);
    }

    public float LandingOffset(float t)
    {
        if (!float.IsFinite(t)) throw new ArgumentOutOfRangeException(nameof(t));
        float u = (Math.Clamp(t, -1f, 1f) + 1f) * .5f * (_landing.Length - 1);
        int k = Math.Min((int)u, _landing.Length - 2);
        return _landing[k] + (_landing[k + 1] - _landing[k]) * (u - k);
    }
}

/// <summary>Risultato C3 ordinato per curvatura crescente; limite esplicito se incompleto.</summary>
public sealed class CincinnusStrokePlan
{
    internal CincinnusStrokePlan(CincinnusBrushStroke[] strokes, bool safetyLimitReached, int points)
    {
        Strokes = Array.AsReadOnly(strokes);
        SafetyLimitReached = safetyLimitReached;
        SpinePointCount = points;
    }
    public IReadOnlyList<CincinnusBrushStroke> Strokes { get; }
    public bool SafetyLimitReached { get; }
    public int SpinePointCount { get; }
}
