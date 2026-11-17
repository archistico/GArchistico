using System.Numerics;

namespace GenerativeArt.Algorithms.Brushes;

/// <summary>Campione lungo la curva portante di un pennello a setole, senza dipendenze grafiche.</summary>
public readonly record struct BristleSpinePoint(Vector2 Position, Vector2 Normal, float HalfWidth);

/// <summary>
/// Curva a curvature lente e orientamento orbitale. I filamenti possono essere costruiti
/// usando Position + Normal * offset * HalfWidth per ogni campione.
/// Ispirato al concetto Spine/Bristles di Gȧrt, ma con geometria originale in C#.
/// </summary>
public static class BristleSpine2D
{
    public static BristleSpinePoint[] Build(Vector2 start, Vector2 eye, int samples, float step,
        float halfWidth, double turn, double swirl, double phase, int spin = 1)
    {
        if (samples is < 2 or > 3000 || !float.IsFinite(step) || step <= 0 ||
            !float.IsFinite(halfWidth) || halfWidth <= 0 ||
            !double.IsFinite(turn) || !double.IsFinite(swirl) || !double.IsFinite(phase) ||
            !float.IsFinite(start.X) || !float.IsFinite(start.Y) ||
            !float.IsFinite(eye.X) || !float.IsFinite(eye.Y) || spin is not (-1 or 1))
            throw new ArgumentOutOfRangeException(nameof(samples), "Geometria del pennello non valida.");

        var points = new BristleSpinePoint[samples];
        Vector2 position = start;
        double angle = Math.Atan2(start.Y - eye.Y, start.X - eye.X) + spin * Math.PI / 2 + phase * .15;
        double orbitRadius = Math.Max(halfWidth * 2, Vector2.Distance(start, eye));
        for (int i = 0; i < samples; i++)
        {
            double t = i / (samples - 1d);
            double taper = Math.Pow(Math.Max(0, Math.Sin(Math.PI * t)), .58);
            float width = (float)(halfWidth * (.02 + .98 * taper) *
                (1 + .12 * Math.Sin(2 * Math.PI * t * 2 + phase)));
            Vector2 normal = new((float)-Math.Sin(angle), (float)Math.Cos(angle));
            points[i] = new BristleSpinePoint(position, normal, Math.Max(0.01f, width));
            double bend = turn * (.035 * Math.Sin(2 * Math.PI * t * 1.4 + phase) +
                .012 * Math.Sin(2 * Math.PI * t * 3.3 + phase * 1.7));
            angle += bend + spin * swirl * step / orbitRadius * .65;
            position += new Vector2((float)(Math.Cos(angle) * step), (float)(Math.Sin(angle) * step));
        }
        return points;
    }
}
