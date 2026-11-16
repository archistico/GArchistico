using System.Numerics;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Geometry;

public enum DepthParticleLayout { Cloud, Spiral, Flow }
public readonly record struct DepthParticle(Vector2 Position, float Depth, float Size, float Brightness);

/// <summary>Particelle normalizzate ordinate dal fondo al primo piano. Nessuna dipendenza raster.</summary>
public static class DepthParticleField
{
    public static DepthParticle[] Build(ulong seed, int count = 240,
        DepthParticleLayout layout = DepthParticleLayout.Cloud, double spread = 1,
        double turns = 3.5, CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 4000 || !Enum.IsDefined(layout)
            || !double.IsFinite(spread) || spread is < .1 or > 2
            || !double.IsFinite(turns) || turns is < .5 or > 12)
            throw new ArgumentOutOfRangeException(nameof(count));
        cancellationToken.ThrowIfCancellationRequested();
        var random = new SplitMix64Random(seed);
        var result = new DepthParticle[count];
        double phase = random.NextDouble() * Math.Tau;
        for (int i = 0; i < count; i++)
        {
            if ((i & 63) == 0) cancellationToken.ThrowIfCancellationRequested();
            double u = random.NextDouble(), v = random.NextDouble();
            Vector2 position;
            if (layout == DepthParticleLayout.Spiral)
            {
                double t = Math.Sqrt(u), a = t * turns * Math.Tau + phase;
                position = new Vector2((float)(.5 + Math.Cos(a) * t * spread * .52 + (v - .5) * .045),
                    (float)(.5 + Math.Sin(a) * t * spread * .52 + (random.NextDouble() - .5) * .045));
            }
            else if (layout == DepthParticleLayout.Flow)
                position = new Vector2((float)(.5 + (u - .5) * 1.2 * spread),
                    (float)(.5 + Math.Sin(u * turns * Math.PI + phase) * .24 * spread + (v - .5) * .42 * spread));
            else position = new Vector2((float)(.5 + (u - .5) * 1.2 * spread), (float)(.5 + (v - .5) * 1.2 * spread));
            // Le estrazioni di profondità/taglia restano separate dalla composizione.
            result[i] = new DepthParticle(position, random.NextFloat(),
                .45f + random.NextFloat() * .9f, .65f + random.NextFloat() * .35f);
        }
        Array.Sort(result, (a, b) => a.Depth.CompareTo(b.Depth));
        return result;
    }
}
