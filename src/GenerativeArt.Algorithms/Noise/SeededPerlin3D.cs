namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// Perlin 3D con permutazione Fisher-Yates costruita da SplitMix64-v1.
/// NON è l'algoritmo seeded OpenSimplex di Gȧrt: i risultati sono specifici del nostro framework.
/// Immutabile dopo il costruttore, utilizzabile da più thread in sola lettura.
/// </summary>
public sealed class SeededPerlin3D
{
    public const string AlgorithmId = "seeded-perlin3d-splitmix64-v1";
    private readonly byte[] _permutation;
    public ulong Seed { get; }

    public SeededPerlin3D(ulong seed)
    {
        Seed = seed;
        _permutation = NoisePermutation256.FromSeed(seed);
    }

    public double Sample(double x, double y, double z) =>
        PerlinKernel3D.Sample(_permutation, x, y, z);
}
