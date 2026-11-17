using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Interference;

/// <summary>
/// Sovrapposizione deterministica di sorgenti d'onda radiali in coordinate
/// normalizzate sul lato minore della tela. Nessuna dipendenza dal raster:
/// campionare lo stesso punto restituisce lo stesso valore anche in fine-2x.
/// L'oggetto è immutabile dopo la costruzione e non contiene cache per-pixel.
/// </summary>
public sealed class WaveInterferenceField2D
{
    private readonly (double X, double Y, double Phase)[] _sources;
    private readonly SeededSimplexNoise _noise;
    private readonly double _frequency;
    private readonly double _distortion;
    private readonly double _falloff;
    private readonly string _mode;

    public int SourceCount => _sources.Length;

    public WaveInterferenceField2D(ulong seed, int sourceCount = 5,
        string layout = "ring", double spread = 1, double frequency = 12,
        double distortion = 0, double falloff = 0.5,
        string mode = "interference")
    {
        if (sourceCount is < 2 or > 12) throw new ArgumentOutOfRangeException(nameof(sourceCount));
        if (layout is not ("ring" or "line" or "scatter")) throw new ArgumentException("Disposizione sconosciuta.", nameof(layout));
        if (!double.IsFinite(spread) || spread is < 0.1 or > 3) throw new ArgumentOutOfRangeException(nameof(spread));
        if (!double.IsFinite(frequency) || frequency is < 2 or > 40) throw new ArgumentOutOfRangeException(nameof(frequency));
        if (!double.IsFinite(distortion) || distortion is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(distortion));
        if (!double.IsFinite(falloff) || falloff is < 0 or > 4) throw new ArgumentOutOfRangeException(nameof(falloff));
        if (mode is not ("interference" or "caustics" or "ribbons")) throw new ArgumentException("Stile sconosciuto.", nameof(mode));

        _frequency = frequency;
        _distortion = distortion;
        _falloff = falloff;
        _mode = mode;
        _noise = new SeededSimplexNoise(seed);
        var random = new SplitMix64Random(seed);
        _sources = new (double X, double Y, double Phase)[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            double angle = Math.Tau * i / sourceCount;
            double x, y;
            switch (layout)
            {
                case "line":
                    x = (i / (double)(sourceCount - 1) - 0.5) * spread * 0.85;
                    y = 0.12 * spread * Math.Sin(angle);
                    break;
                case "scatter":
                    x = (random.NextDouble() - 0.5) * spread;
                    y = (random.NextDouble() - 0.5) * spread;
                    break;
                default:
                    x = 0.34 * spread * Math.Cos(angle);
                    y = 0.34 * spread * Math.Sin(angle);
                    break;
            }
            // L'offset di fase è deterministico e differente per ogni seed.
            _sources[i] = (x, y, random.NextDouble() * Math.Tau);
        }
    }

    /// <summary>Campione in [0,1]. X e Y sono in unità del lato minore.</summary>
    public double Sample(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinate finite obbligatorie.");
        if (_distortion > 0)
        {
            double dx = _noise.Sample2D(x * 3.1 + 13, y * 3.1 - 7) * _distortion * 0.2;
            double dy = _noise.Sample2D(x * 3.1 - 5, y * 3.1 + 17) * _distortion * 0.2;
            x += dx;
            y += dy;
        }

        double amplitude = 0, weight = 0;
        foreach (var source in _sources)
        {
            double sx = x - source.X, sy = y - source.Y;
            double distance = Math.Sqrt(sx * sx + sy * sy);
            double envelope = Math.Exp(-_falloff * distance);
            amplitude += envelope * Math.Sin(Math.Tau * _frequency * distance + source.Phase);
            weight += envelope;
        }
        double sum = weight > 0 ? amplitude / weight : 0;
        double value = _mode switch
        {
            "caustics" => Math.Pow(Math.Abs(sum), 3.0),
            "ribbons" => 0.5 + 0.5 * Math.Sin(sum * Math.Tau * 2.5),
            _ => 0.5 + 0.5 * sum
        };
        return Math.Clamp(value, 0, 1);
    }
}
