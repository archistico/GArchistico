namespace GenerativeArt.Algorithms.Noise;

/// <summary>Profilo frattale continuo per strati e orizzonti, indipendente dal renderer.</summary>
public sealed class FractalProfile1D
{
    private readonly SeededSimplexNoise _noise;
    private readonly int _octaves;
    private readonly double _persistence;
    private readonly double _warp;
    public FractalProfile1D(ulong seed, int octaves = 4, double persistence = .5, double warp = .5)
    {
        if (octaves is < 1 or > 7 || !double.IsFinite(persistence) || persistence is < .1 or > .9
            || !double.IsFinite(warp) || warp is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(octaves));
        _noise = new SeededSimplexNoise(seed);
        _octaves = octaves; _persistence = persistence; _warp = warp;
    }

    public double Sample(double x, double layer)
    {
        if (!double.IsFinite(x) || !double.IsFinite(layer) || Math.Abs(x) > 10000 || Math.Abs(layer) > 10000)
            throw new ArgumentOutOfRangeException(nameof(x));
        double shifted = x + _warp * _noise.Sample2D(x * .45 + 13, layer * .3 + 31);
        double sum = 0, total = 0, weight = 1, frequency = 1;
        for (int octave = 0; octave < _octaves; octave++)
        {
            sum += weight * _noise.Sample2D(shifted * frequency, layer + octave * 7.3);
            total += weight; weight *= _persistence; frequency *= 2;
        }
        return Math.Clamp(sum / total, -1, 1);
    }
}
