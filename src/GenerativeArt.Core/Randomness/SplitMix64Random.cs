using System.Text;

namespace GenerativeArt.Core.Randomness;

/// <summary>
/// SplitMix64 versione 1. NON compatibile bit-a-bit con kotlin.random.Random.
/// Seed e identificativo del sottostream producono la stessa sequenza su tutte le piattaforme.
/// Le istanze sono stateful e non thread-safe: usarne una per render/worker.
/// </summary>
public sealed class SplitMix64Random : IRandomSource
{
    public const string AlgorithmId = "splitmix64-v1";
    private const ulong Gamma = 0x9E3779B97F4A7C15UL;
    private const ulong FnvOffsetBasis = 0xcbf29ce484222325UL;
    private const ulong FnvPrime = 0x100000001b3UL;

    private readonly ulong _seed;
    private ulong _state;

    public ulong Seed => _seed;

    public SplitMix64Random(ulong seed)
    {
        _seed = seed;
        _state = seed;
    }

    public ulong NextUInt64()
    {
        _state = unchecked(_state + Gamma);
        return Mix(_state);
    }

    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
    public float NextFloat() => (NextUInt64() >> 40) * (1f / 16777216f);

    /// <summary>
    /// Estremi inclusivi e campionamento uniforme per rejection sampling, anche su tutto Int32.
    /// Un intervallo degenerato non consuma numeri casuali.
    /// </summary>
    public int NextInt32Inclusive(int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxInclusive), "L'estremo massimo non può precedere il minimo.");
        if (maxInclusive == minInclusive) return minInclusive;

        ulong range = (ulong)((long)maxInclusive - minInclusive + 1);
        ulong threshold = unchecked(0UL - range) % range;
        ulong value;
        do { value = NextUInt64(); } while (value < threshold);
        return (int)((long)minInclusive + (long)(value % range));
    }

    /// <summary>
    /// Sottostream derivato da seed originale e ID UTF-8; non dipende dalle chiamate già fatte.
    /// La codifica UTF-8 e FNV-1a sono parte del contratto v1.
    /// </summary>
    public IRandomSource Fork(string streamId)
    {
        ArgumentException.ThrowIfNullOrEmpty(streamId);
        ulong hash = FnvOffsetBasis;
        foreach (byte b in Encoding.UTF8.GetBytes(streamId))
        {
            hash = unchecked((hash ^ b) * FnvPrime);
        }
        return new SplitMix64Random(Mix(_seed ^ Mix(hash)));
    }

    private static ulong Mix(ulong z)
    {
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }
}
