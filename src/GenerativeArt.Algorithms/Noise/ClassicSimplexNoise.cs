namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// Simplex classico 2D/3D con permutazione fissa di Gȧrt.
/// I campioni validi riproducono SimplexNoise.kt; non richiede stato né seed.
/// </summary>
public static class ClassicSimplexNoise
{
    public const string AlgorithmId = "gart-classic-simplex-fixedperm-v1";

    public static double Sample2D(double x, double y) =>
        SimplexKernel.Sample2D(ClassicPerlin3D.FixedPermutation, x, y);

    public static double Sample3D(double x, double y, double z) =>
        SimplexKernel.Sample3D(ClassicPerlin3D.FixedPermutation, x, y, z);
}
