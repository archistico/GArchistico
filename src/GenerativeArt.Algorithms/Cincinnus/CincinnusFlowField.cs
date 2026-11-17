using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>Vortice tangenziale in coordinate canvas; uno spin negativo ruota in senso antiorario.</summary>
public readonly record struct CincinnusVortex(Vector2 Center, float Spin, float Reach);

/// <summary>
/// Primo kernel geometrico Cincinnus: campo ottenuto sommando vortici esterni alla tela.
/// Riprende la legge di VectorField.vortex e il criterio maximin di Cincinnus.kt (Gȧrt),
/// ma usa gli stream SplitMix64 del progetto e NON riproduce i seed Kotlin bit per bit.
/// Dopo la costruzione è immutabile, senza dipendenze grafiche né stato globale.
/// </summary>
public sealed class CincinnusFlowField : IVectorField2D
{
    private const int MaximumCanvasSide = 32_768;
    private const int MaximumOffPageAttempts = 4096;
    private readonly CincinnusVortex[] _vortices;
    private readonly IReadOnlyList<CincinnusVortex> _view;

    private CincinnusFlowField(CincinnusVortex[] vortices)
    {
        _vortices = vortices;
        _view = Array.AsReadOnly(vortices);
    }

    public IReadOnlyList<CincinnusVortex> Vortices => _view;

    /// <summary>
    /// Seleziona per ciascun vortice il candidato più lontano dai centri già scelti,
    /// su 12 candidati esterni alla tela per default. Originale: intervallo
    /// [-.4, 1.4] del lato, distanza esterna minima .08 del lato e reach .3 del lato.
    /// Per canvas non quadrati le dimensioni X/Y sono indipendenti e reach e margine
    /// usano il lato minore. Non forza alcun campo di drift aggiuntivo.
    /// </summary>
    public static CincinnusFlowField Create(int width, int height, long seed = 3,
        int vortexCount = 5, int candidatesPerVortex = 12,
        CancellationToken cancellationToken = default)
    {
        if (width is < 1 or > MaximumCanvasSide) throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > MaximumCanvasSide) throw new ArgumentOutOfRangeException(nameof(height));
        if (vortexCount is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(vortexCount));
        if (candidatesPerVortex is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(candidatesPerVortex));
        cancellationToken.ThrowIfCancellationRequested();

        var root = new SplitMix64Random(unchecked((ulong)seed));
        IRandomSource positions = root.Fork("cincinnus/positions/v1");
        IRandomSource dynamics = root.Fork("cincinnus/dynamics/v1");
        var selected = new List<Vector2>(vortexCount);
        var result = new CincinnusVortex[vortexCount];
        float side = Math.Min(width, height);

        for (int i = 0; i < vortexCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Vector2 winner = default;
            double bestSeparation = double.NegativeInfinity;
            for (int j = 0; j < candidatesPerVortex; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Vector2 candidate = PickOutside(positions, width, height, side);
                double distance = double.PositiveInfinity;
                foreach (Vector2 earlier in selected)
                {
                    double dx = (double)candidate.X - earlier.X;
                    double dy = (double)candidate.Y - earlier.Y;
                    distance = Math.Min(distance, dx * dx + dy * dy);
                }
                // Per il primo centro tutti i candidati sono equivalenti: vince il primo.
                if (j == 0 || distance > bestSeparation)
                {
                    winner = candidate;
                    bestSeparation = distance;
                }
            }
            selected.Add(winner);
            float spin = -(0.6f + 0.8f * dynamics.NextFloat());
            float reach = 0.3f * side * (0.7f + 0.6f * dynamics.NextFloat());
            result[i] = new CincinnusVortex(winner, spin, reach);
        }

        return new CincinnusFlowField(result);
    }

    /// <summary>Costruisce un campo con vortici espliciti, utile per test e studi di forma.</summary>
    public static CincinnusFlowField FromVortices(IEnumerable<CincinnusVortex> vortices)
    {
        ArgumentNullException.ThrowIfNull(vortices);
        CincinnusVortex[] snapshot = vortices.ToArray();
        if (snapshot.Length is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(vortices), "Occorrono da 1 a 64 vortici.");
        foreach (CincinnusVortex vortex in snapshot)
        {
            if (!float.IsFinite(vortex.Center.X) || !float.IsFinite(vortex.Center.Y)
                || !float.IsFinite(vortex.Spin) || !float.IsFinite(vortex.Reach) || vortex.Reach <= 0)
                throw new ArgumentOutOfRangeException(nameof(vortices), "Centro, spin e reach devono essere finiti; reach > 0.");
        }
        return new CincinnusFlowField(snapshot);
    }

    public Vector2 Sample(float x, float y)
    {
        if (!float.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!float.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));

        double sx = 0d, sy = 0d;
        foreach (CincinnusVortex vortex in _vortices)
        {
            double dx = (double)x - vortex.Center.X;
            double dy = (double)y - vortex.Center.Y;
            double radius2 = dx * dx + dy * dy;
            if (radius2 == 0d) continue;
            double reach2 = (double)vortex.Reach * vortex.Reach;
            double weight = 1d / (1d + radius2 / reach2) / Math.Sqrt(radius2);
            sx += -dy * vortex.Spin * weight;
            sy += dx * vortex.Spin * weight;
        }
        return new Vector2((float)sx, (float)sy);
    }

    private static Vector2 PickOutside(IRandomSource random, int width, int height, float side)
    {
        for (int i = 0; i < MaximumOffPageAttempts; i++)
        {
            float x = (float)((1.8d * random.NextDouble() - .4d) * width);
            float y = (float)((1.8d * random.NextDouble() - .4d) * height);
            double outside = Math.Max(Math.Max(-x, (double)x - width),
                                      Math.Max(-y, (double)y - height));
            if (outside > .08d * side) return new Vector2(x, y);
        }
        throw new InvalidOperationException("Impossibile selezionare un vortice esterno: budget di tentativi esaurito.");
    }
}
