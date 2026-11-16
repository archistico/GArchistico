namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// fBm normalizzato: sovrappone octaves di Perlin seeded; stessa base per ogni ottava,
/// coordinate scalate con lacunarity e ampiezza con persistence.
/// </summary>
public static class FractalNoise3D
{
    public static double Sample(SeededPerlin3D noise, double x, double y, double z,
        int octaves = 4, double lacunarity = 2d, double persistence = 0.5d)
    {
        ArgumentNullException.ThrowIfNull(noise);
        if (octaves is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(octaves));
        if (!double.IsFinite(lacunarity) || lacunarity <= 0d)
            throw new ArgumentOutOfRangeException(nameof(lacunarity));
        if (!double.IsFinite(persistence) || persistence < 0d || persistence > 1d)
            throw new ArgumentOutOfRangeException(nameof(persistence));
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!double.IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));
        double value = 0d, weight = 0d, amplitude = 1d, frequency = 1d;
        for (int i = 0; i < octaves; i++)
        {
            // Un overflow delle coordinate non deve trasformarsi in un risultato NaN silenzioso.
            double nx = x * frequency, ny = y * frequency, nz = z * frequency;
            if (!double.IsFinite(nx) || !double.IsFinite(ny) || !double.IsFinite(nz))
                throw new ArgumentOutOfRangeException(nameof(lacunarity), "Le ottave producono coordinate non finite.");
            value += amplitude * noise.Sample(nx, ny, nz);
            weight += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return value / weight;
    }
}
