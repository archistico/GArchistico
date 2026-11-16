namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// Simplex 2D/3D deterministico tramite permutazione Fisher-Yates SplitMix64-v1.
/// Stessa formulazione geometrica del Simplex classico ma permutazione differente:
/// non è OpenSimplex e non è bit-a-bit compatibile con la versione fissa di Gȧrt.
/// Sicuro per campionamenti concorrenti: i dati non vengono modificati dopo la costruzione.
/// </summary>
public sealed class SeededSimplexNoise
{
    public const string AlgorithmId = "seeded-simplex2d3d-splitmix64-v1";
    private readonly byte[] _permutation;

    public ulong Seed { get; }

    public SeededSimplexNoise(ulong seed)
    {
        Seed = seed;
        _permutation = NoisePermutation256.FromSeed(seed);
    }

    public double Sample2D(double x, double y) => SimplexKernel.Sample2D(_permutation, x, y);

    public double Sample3D(double x, double y, double z) => SimplexKernel.Sample3D(_permutation, x, y, z);
}
