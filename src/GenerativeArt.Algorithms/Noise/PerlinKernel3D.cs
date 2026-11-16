namespace GenerativeArt.Algorithms.Noise;

/// <summary>Calcolo condiviso del Perlin classico a permutazione 256, senza RNG implicito.</summary>
internal static class PerlinKernel3D
{
    public static double Sample(byte[] permutation, double x, double y, double z)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!double.IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));
        double fx = Math.Floor(x), fy = Math.Floor(y), fz = Math.Floor(z);
        int xi = (int)(fx % 256d) & 255;
        int yi = (int)(fy % 256d) & 255;
        int zi = (int)(fz % 256d) & 255;
        double xx = x - fx, yy = y - fy, zz = z - fz;
        double u = Fade(xx), v = Fade(yy), w = Fade(zz);

        int a = P(permutation, xi) + yi;
        int aa = P(permutation, a) + zi;
        int ab = P(permutation, a + 1) + zi;
        int b = P(permutation, xi + 1) + yi;
        int ba = P(permutation, b) + zi;
        int bb = P(permutation, b + 1) + zi;

        return Lerp(w,
            Lerp(v,
                Lerp(u, Grad(P(permutation, aa), xx, yy, zz), Grad(P(permutation, ba), xx - 1, yy, zz)),
                Lerp(u, Grad(P(permutation, ab), xx, yy - 1, zz), Grad(P(permutation, bb), xx - 1, yy - 1, zz))),
            Lerp(v,
                Lerp(u, Grad(P(permutation, aa + 1), xx, yy, zz - 1), Grad(P(permutation, ba + 1), xx - 1, yy, zz - 1)),
                Lerp(u, Grad(P(permutation, ab + 1), xx, yy - 1, zz - 1), Grad(P(permutation, bb + 1), xx - 1, yy - 1, zz - 1))));
    }

    private static int P(byte[] perm, int index) => perm[index & 255];
    private static double Fade(double t) => t * t * t * (t * (t * 6d - 15d) + 10d);
    private static double Lerp(double t, double a, double b) => a + t * (b - a);

    private static double Grad(int hash, double x, double y, double z)
    {
        int h = hash & 15;
        double u = h < 8 ? x : y;
        double v = h < 4 ? y : (h is 12 or 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
