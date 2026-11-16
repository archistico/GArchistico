namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// Kernel simplex 2D/3D basato sulla formulazione classica usata in Gȧrt (Gustavson).
/// Nessuna allocazione nello hot path; la permutazione è fornita dall'esterno.
/// Non confondere con OpenSimplex, che utilizza un algoritmo differente.
/// </summary>
internal static class SimplexKernel
{
    private const double F3 = 1d / 3d;
    private const double G3 = 1d / 6d;
    private static readonly double F2 = 0.5d * (Math.Sqrt(3d) - 1d);
    private static readonly double G2 = (3d - Math.Sqrt(3d)) / 6d;

    private static readonly (double X, double Y, double Z)[] Gradients =
    [
        (1d, 1d, 0d), (-1d, 1d, 0d), (1d, -1d, 0d), (-1d, -1d, 0d),
        (1d, 0d, 1d), (-1d, 0d, 1d), (1d, 0d, -1d), (-1d, 0d, -1d),
        (0d, 1d, 1d), (0d, -1d, 1d), (0d, 1d, -1d), (0d, -1d, -1d)
    ];

    public static double Sample2D(ReadOnlySpan<byte> permutation, double x, double y)
    {
        ValidateCoordinates(x, nameof(x));
        ValidateCoordinates(y, nameof(y));
        double skew = (x + y) * F2;
        int i = Cell(x + skew, nameof(x));
        int j = Cell(y + skew, nameof(y));

        double unskew = ((double)i + j) * G2;
        double x0 = x - (i - unskew);
        double y0 = y - (j - unskew);
        int i1 = x0 > y0 ? 1 : 0;
        int j1 = x0 > y0 ? 0 : 1;
        double x1 = x0 - i1 + G2;
        double y1 = y0 - j1 + G2;
        double x2 = x0 - 1d + 2d * G2;
        double y2 = y0 - 1d + 2d * G2;

        int ii = i & 255;
        int jj = j & 255;
        int gi0 = Index2(permutation, ii, jj);
        int gi1 = Index2(permutation, ii + i1, jj + j1);
        int gi2 = Index2(permutation, ii + 1, jj + 1);
        return 70d * (Contribution2(gi0, x0, y0) + Contribution2(gi1, x1, y1) + Contribution2(gi2, x2, y2));
    }

    public static double Sample3D(ReadOnlySpan<byte> permutation, double x, double y, double z)
    {
        ValidateCoordinates(x, nameof(x));
        ValidateCoordinates(y, nameof(y));
        ValidateCoordinates(z, nameof(z));
        double skew = (x + y + z) * F3;
        int i = Cell(x + skew, nameof(x));
        int j = Cell(y + skew, nameof(y));
        int k = Cell(z + skew, nameof(z));

        double unskew = ((double)i + j + k) * G3;
        double x0 = x - (i - unskew);
        double y0 = y - (j - unskew);
        double z0 = z - (k - unskew);

        int i1, j1, k1, i2, j2, k2;
        if (x0 >= y0)
        {
            if (y0 >= z0)
            {
                (i1, j1, k1) = (1, 0, 0);
                (i2, j2, k2) = (1, 1, 0);
            }
            else if (x0 >= z0)
            {
                (i1, j1, k1) = (1, 0, 0);
                (i2, j2, k2) = (1, 0, 1);
            }
            else
            {
                (i1, j1, k1) = (0, 0, 1);
                (i2, j2, k2) = (1, 0, 1);
            }
        }
        else
        {
            if (y0 < z0)
            {
                (i1, j1, k1) = (0, 0, 1);
                (i2, j2, k2) = (0, 1, 1);
            }
            else if (x0 < z0)
            {
                (i1, j1, k1) = (0, 1, 0);
                (i2, j2, k2) = (0, 1, 1);
            }
            else
            {
                (i1, j1, k1) = (0, 1, 0);
                (i2, j2, k2) = (1, 1, 0);
            }
        }

        double x1 = x0 - i1 + G3;
        double y1 = y0 - j1 + G3;
        double z1 = z0 - k1 + G3;
        double x2 = x0 - i2 + 2d * G3;
        double y2 = y0 - j2 + 2d * G3;
        double z2 = z0 - k2 + 2d * G3;
        double x3 = x0 - 1d + 3d * G3;
        double y3 = y0 - 1d + 3d * G3;
        double z3 = z0 - 1d + 3d * G3;

        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;
        int gi0 = Index3(permutation, ii, jj, kk);
        int gi1 = Index3(permutation, ii + i1, jj + j1, kk + k1);
        int gi2 = Index3(permutation, ii + i2, jj + j2, kk + k2);
        int gi3 = Index3(permutation, ii + 1, jj + 1, kk + 1);
        return 32d * (Contribution3(gi0, x0, y0, z0) +
                      Contribution3(gi1, x1, y1, z1) +
                      Contribution3(gi2, x2, y2, z2) +
                      Contribution3(gi3, x3, y3, z3));
    }

    private static int Index2(ReadOnlySpan<byte> p, int i, int j) =>
        p[(i + p[j & 255]) & 255] % 12;

    private static int Index3(ReadOnlySpan<byte> p, int i, int j, int k) =>
        p[(i + p[(j + p[k & 255]) & 255]) & 255] % 12;

    private static double Contribution2(int index, double x, double y)
    {
        double t = 0.5d - x * x - y * y;
        if (t < 0d) return 0d;
        t *= t;
        var gradient = Gradients[index];
        return t * t * (gradient.X * x + gradient.Y * y);
    }

    private static double Contribution3(int index, double x, double y, double z)
    {
        double t = 0.6d - x * x - y * y - z * z;
        if (t < 0d) return 0d;
        t *= t;
        var gradient = Gradients[index];
        return t * t * (gradient.X * x + gradient.Y * y + gradient.Z * z);
    }

    private static void ValidateCoordinates(double value, string name)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(name, "La coordinata deve essere finita.");
    }

    private static int Cell(double skewedCoordinate, string name)
    {
        double floored = Math.Floor(skewedCoordinate);
        // Il riferimento Kotlin converte il floor a Int; rifiutiamo coordinate fuori range
        // invece di saturarle, evitando celle mal definite e alias imprevedibili.
        if (!double.IsFinite(floored) || floored < int.MinValue || floored > int.MaxValue)
            throw new ArgumentOutOfRangeException(name, "Coordinata oltre la griglia simplex supportata.");
        return (int)floored;
    }
}
