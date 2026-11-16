namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// fBm Simplex deformato da due campi indipendenti. Le coordinate sono in unità
/// noise, l'ampiezza in unità di dominio. Immutabile, nessuna allocazione per sample.
/// </summary>
public sealed class DomainWarpNoise2D
{
    private readonly SeededSimplexNoise _baseNoise;
    private readonly SeededSimplexNoise _warpX;
    private readonly SeededSimplexNoise _warpY;
    public int Octaves { get; }
    public double WarpAmount { get; }
    public int WarpLayers { get; }
    public double Lacunarity { get; }
    public double Persistence { get; }
    public double WarpFrequency { get; }

    public DomainWarpNoise2D(ulong seed, int octaves = 3, double warpAmount = 1.3,
        int warpLayers = 2, double lacunarity = 2, double persistence = .5,
        double warpFrequency = 1)
    {
        if (octaves is < 1 or > 6) throw new ArgumentOutOfRangeException(nameof(octaves));
        if (!double.IsFinite(warpAmount) || warpAmount is < 0 or > 8)
            throw new ArgumentOutOfRangeException(nameof(warpAmount));
        if (warpLayers is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(warpLayers));
        if (!double.IsFinite(lacunarity) || lacunarity is < 1 or > 3.5)
            throw new ArgumentOutOfRangeException(nameof(lacunarity));
        if (!double.IsFinite(persistence) || persistence is < .1 or > .9)
            throw new ArgumentOutOfRangeException(nameof(persistence));
        if (!double.IsFinite(warpFrequency) || warpFrequency is < .2 or > 3)
            throw new ArgumentOutOfRangeException(nameof(warpFrequency));
        Octaves = octaves;
        WarpAmount = warpAmount;
        WarpLayers = warpLayers;
        Lacunarity = lacunarity;
        Persistence = persistence;
        WarpFrequency = warpFrequency;
        _baseNoise = new SeededSimplexNoise(seed);
        _warpX = new SeededSimplexNoise(seed ^ 0x9E3779B97F4A7C15UL);
        _warpY = new SeededSimplexNoise(seed ^ 0xD1B54A32D192ED03UL);
    }

    /// <summary>Campione finito in [-1,1]; ampiezza zero equivale a fBm non deformato.</summary>
    public double Sample(double x, double y)
    {
        if (!double.IsFinite(x) || Math.Abs(x) > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y) || Math.Abs(y) > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(y));
        double px = x, py = y;
        if (WarpAmount > 0)
            for (int layer = 0; layer < WarpLayers; layer++)
            {
                double dx = Fbm(_warpX, px * WarpFrequency + 17.1 + layer * 7.3, py * WarpFrequency + 9.2);
                double dy = Fbm(_warpY, px * WarpFrequency - 8.3, py * WarpFrequency + 23.7 + layer * 5.1);
                // Tutti i livelli deformano lo stesso dominio originale.
                px = x + WarpAmount * dx;
                py = y + WarpAmount * dy;
            }
        return Math.Clamp(Fbm(_baseNoise, px, py), -1, 1);
    }

    private double Fbm(SeededSimplexNoise noise, double x, double y)
    {
        double sum = 0, total = 0, weight = 1;
        for (int octave = 0; octave < Octaves; octave++)
        {
            sum += weight * noise.Sample2D(x, y);
            total += weight;
            weight *= Persistence;
            x *= Lacunarity;
            y *= Lacunarity;
        }
        return sum / total;
    }
}
