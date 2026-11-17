using System.Numerics;
using GenerativeArt.Algorithms.Flow;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>Singola linea in ordine di flusso, con clearance dagli altri tracciati.</summary>
public sealed class CincinnusStreamline
{
    private readonly IReadOnlyList<Vector2> _points;
    private readonly IReadOnlyList<float> _clearance;
    private readonly float _separation;
    private readonly float _testDistance;

    internal CincinnusStreamline(Vector2[] points, float[] clearance, float length,
        float separation, float testDistance)
    {
        _points = Array.AsReadOnly(points);
        _clearance = Array.AsReadOnly(clearance);
        Length = length;
        _separation = separation;
        _testDistance = testDistance;
    }

    public IReadOnlyList<Vector2> Points => _points;
    public IReadOnlyList<float> Clearance => _clearance;
    public float Length { get; }
    /// <summary>Distanza nominale fra le streamlines, in pixel logici.</summary>
    public float Separation => _separation;

    /// <summary>Fattore da 0 a 1 per assottigliare una linea vicino ad altre linee.</summary>
    public float Taper(int index)
    {
        float distance = _clearance[index];
        return _separation > _testDistance
            ? Math.Clamp((distance - _testDistance) / (_separation - _testDistance), 0f, 1f)
            : 1f;
    }
}

/// <summary>
/// Geometria C2 pronta per C3, non un'immagine. Un limite di sicurezza raggiunto
/// è segnalato e non viene scambiato per distribuzione completata.
/// </summary>
public sealed class CincinnusSimpleLayout
{
    internal CincinnusSimpleLayout(IVectorField2D field, CincinnusStreamline[] lines,
        bool safetyLimitReached, int attemptedSeeds)
    {
        Field = field;
        Lines = Array.AsReadOnly(lines);
        SafetyLimitReached = safetyLimitReached;
        AttemptedSeeds = attemptedSeeds;
    }

    public IVectorField2D Field { get; }
    public IReadOnlyList<CincinnusStreamline> Lines { get; }
    public bool SafetyLimitReached { get; }
    public int AttemptedSeeds { get; }
}
