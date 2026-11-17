using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Pattern;

/// <summary>Distribuzione fillotattica deterministica: coordinate normalizzate al raggio unitario.</summary>
public static class PhyllotaxisPattern
{
    public readonly record struct SeedPoint(double X, double Y, double Radius, double Progress);

    /// <summary>
    /// Il progressivo governa dimensioni e colore, mentre il seed governa piccole
    /// irregolarità locali. Non crea liste: i renderer possono scorrere i semi uno alla volta.
    /// </summary>
    public static IEnumerable<SeedPoint> Generate(ulong seed, int count, double angleDegrees,
        double jitter, int arms, string shape)
    {
        if (count is < 1 or > 25000) throw new ArgumentOutOfRangeException(nameof(count));
        if (!double.IsFinite(angleDegrees) || angleDegrees is < 95 or > 180)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees));
        if (!double.IsFinite(jitter) || jitter is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(jitter));
        if (arms is < 1 or > 24) throw new ArgumentOutOfRangeException(nameof(arms));
        if (shape is not ("sunflower" or "rosette" or "galaxy"))
            throw new ArgumentException("Forma fillotattica sconosciuta.", nameof(shape));

        var random = new SplitMix64Random(seed);
        double radians = angleDegrees * Math.PI / 180;
        for (int i = 0; i < count; i++)
        {
            double progress = (i + 0.5) / count;
            double angle = i * radians + (random.NextDouble() - .5) * jitter * .18;
            double radius = Math.Sqrt(progress);
            double flower = 1 + .16 * Math.Cos(arms * angle);
            if (shape == "rosette") radius *= flower;
            if (shape == "galaxy")
            {
                radius *= .55 + .45 * progress;
                angle += 3 * (1 - progress) + .4 * Math.Sin(arms * angle);
            }
            radius *= 1 + (random.NextDouble() - .5) * jitter * .22;
            yield return new SeedPoint(radius * Math.Cos(angle), radius * Math.Sin(angle),
                radius, progress);
        }
    }
}
