using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

internal static class ArtisticFlow
{
    internal static IVectorField2D Field(string kind, ulong seed, FlowBounds bounds,
        double frequency, double curvature = 3.7)
    {
        if (kind == "curl") return new CurlNoiseField2D(seed, (float)frequency);
        if (kind == "vortex") return VectorFields.Vortex(new Vector2(
            (bounds.Left + bounds.Right) * .5f, (bounds.Top + bounds.Bottom) * .5f), pull: .12f);
        if (kind == "braid")
        {
            var random = new SplitMix64Random(seed);
            double phase = random.NextDouble() * Math.Tau;
            return VectorFields.Angles((x, y) => (float)(curvature * (
                .65 * Math.Sin(x * frequency + phase) + .35 * Math.Cos(y * frequency * .73 - phase))));
        }
        var noise = new SeededSimplexNoise(seed);
        return VectorFields.Angles((x, y) => (float)(noise.Sample2D(x * frequency, y * frequency)
            * Math.PI * curvature + .4 * Math.Sin(x * frequency * 1.2)));
    }

    internal static Vector2 Start(int index, int count, string layout, SplitMix64Random random, FlowBounds bounds)
    {
        float width = bounds.Right - bounds.Left, height = bounds.Bottom - bounds.Top;
        float x, y;
        if (layout == "grid")
        {
            int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count * (double)width / height)));
            int rows = (count + columns - 1) / columns;
            x = (index % columns + .5f + (random.NextFloat() - .5f) * .5f) * width / columns;
            y = (index / columns + .5f + (random.NextFloat() - .5f) * .5f) * height / rows;
        }
        else if (layout == "edge")
        {
            float t = (index / 4 + .5f) / Math.Max(1, (count + 3) / 4);
            (x, y) = (index % 4) switch
            {
                0 => (.001f, t * height), 1 => (width - .001f, t * height),
                2 => (t * width, .001f), _ => (t * width, height - .001f)
            };
        }
        else { x = random.NextFloat() * width; y = random.NextFloat() * height; }
        return new Vector2(bounds.Left + Math.Clamp(x, .001f, width - .001f),
            bounds.Top + Math.Clamp(y, .001f, height - .001f));
    }
}
