using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Pattern;

/// <summary>
/// Simulazione Gray–Scott in una griglia toroidale: due specie U e V, laplaciano a
/// nove punti, integrazione esplicita. Conserva solo quattro array della griglia.
/// Il seed influenza esclusivamente l'inoculo iniziale (riproducibile).
/// </summary>
public sealed class ReactionDiffusionField2D
{
    private double[] _u, _v, _nextU, _nextV;
    private readonly bool _production;
    public int Width { get; }
    public int Height { get; }
    public long CellCount => (long)Width * Height;

    public ReactionDiffusionField2D(int width, int height, ulong seed,
        int spots = 18, int spotRadius = 4, bool production = false)
    {
        if (width is < 4 or > 2048 || height is < 4 or > 2048 || (long)width * height > (production ? 1_048_576 : 350_000))
            throw new ArgumentOutOfRangeException(nameof(width), $"Griglia massima: {(production ? 1_048_576 : 350_000):N0} celle, minimo 4 per lato.");
        if (spots is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(spots));
        if (spotRadius is < 1 or > 24) throw new ArgumentOutOfRangeException(nameof(spotRadius));
        Width = width;
        Height = height;
        _production = production;
        int length = checked(width * height);
        _u = new double[length];
        _v = new double[length];
        _nextU = new double[length];
        _nextV = new double[length];
        Array.Fill(_u, 1);
        var random = new SplitMix64Random(seed);
        for (int i = 0; i < spots; i++)
        {
            int cx = random.NextInt32Inclusive(0, width - 1);
            int cy = random.NextInt32Inclusive(0, height - 1);
            for (int dy = -spotRadius; dy <= spotRadius; dy++)
            for (int dx = -spotRadius; dx <= spotRadius; dx++)
            {
                if (dx * dx + dy * dy > spotRadius * spotRadius) continue;
                int x = Mod(cx + dx, width);
                int y = Mod(cy + dy, height);
                int n = y * width + x;
                _u[n] = 0.35 + random.NextDouble() * 0.12;
                _v[n] = 0.55 + random.NextDouble() * 0.15;
            }
        }
    }

    public void Evolve(int iterations, double feed, double kill,
        double diffusionU, double diffusionV, CancellationToken cancellationToken = default)
    {
        if (iterations is < 1 or > 1500) throw new ArgumentOutOfRangeException(nameof(iterations));
        if (!double.IsFinite(feed) || feed is < 0.005 or > 0.1) throw new ArgumentOutOfRangeException(nameof(feed));
        if (!double.IsFinite(kill) || kill is < .02 or > .09) throw new ArgumentOutOfRangeException(nameof(kill));
        if (!double.IsFinite(diffusionU) || diffusionU is < .05 or > .25)
            throw new ArgumentOutOfRangeException(nameof(diffusionU));
        if (!double.IsFinite(diffusionV) || diffusionV is < .02 or > .15)
            throw new ArgumentOutOfRangeException(nameof(diffusionV));
        if (CellCount * iterations > (_production ? 1_500_000_000 : 75_000_000))
            throw new ArgumentOutOfRangeException(nameof(iterations), $"Reazione–diffusione: massimo {(_production ? 1_500_000_000 : 75_000_000):N0} aggiornamenti di cella.");

        for (int step = 0; step < iterations; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GenerativeArt.Core.Rendering.RasterRows.For(0, Height, _production, cancellationToken, y =>
            {
                int up = (y == 0 ? Height - 1 : y - 1) * Width;
                int down = (y == Height - 1 ? 0 : y + 1) * Width;
                int row = y * Width;
                for (int x = 0; x < Width; x++)
                {
                    int left = x == 0 ? Width - 1 : x - 1;
                    int right = x == Width - 1 ? 0 : x + 1;
                    int n = row + x;
                    double u = _u[n], v = _v[n];
                    double lapU = -u + .2 * (_u[row + left] + _u[row + right] + _u[up + x] + _u[down + x])
                        + .05 * (_u[up + left] + _u[up + right] + _u[down + left] + _u[down + right]);
                    double lapV = -v + .2 * (_v[row + left] + _v[row + right] + _v[up + x] + _v[down + x])
                        + .05 * (_v[up + left] + _v[up + right] + _v[down + left] + _v[down + right]);
                    double reaction = u * v * v;
                    _nextU[n] = Math.Clamp(u + diffusionU * lapU - reaction + feed * (1 - u), 0, 1);
                    _nextV[n] = Math.Clamp(v + diffusionV * lapV + reaction - (feed + kill) * v, 0, 1);
                }
            });
            (_u, _nextU) = (_nextU, _u);
            (_v, _nextV) = (_nextV, _v);
        }
    }

    public double Sample(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(x));
        // Campionamento bilineare periodico per conservare la continuità ai bordi.
        double gx = x * Width - .5, gy = y * Height - .5;
        int x0 = (int)Math.Floor(gx), y0 = (int)Math.Floor(gy);
        double tx = gx - x0, ty = gy - y0;
        int a = Mod(x0, Width), b = Mod(x0 + 1, Width);
        int c = Mod(y0, Height), d = Mod(y0 + 1, Height);
        double top = _v[c * Width + a] * (1 - tx) + _v[c * Width + b] * tx;
        double bottom = _v[d * Width + a] * (1 - tx) + _v[d * Width + b] * tx;
        return top * (1 - ty) + bottom * ty;
    }

    private static int Mod(int n, int mod) => (n % mod + mod) % mod;
}
