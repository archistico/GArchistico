using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// LIC discreta: media triangolare simmetrica di una texture scalare lungo
/// streamlines RK2 normalizzate. Bordi troncati e pesi rinormalizzati.
/// Implementazione autonoma del principio di Cabral e Leedom (1993), non fast-LIC.
/// </summary>
public static class LineIntegralConvolution
{
    public const long MaximumPixels = 16_777_216;
    public const long MaximumSamples = 400_000_000;

    public static void ValidateBudget(int width, int height, int steps)
    {
        long pixels = (long)width * height;
        if (width < 1 || height < 1 || pixels > MaximumPixels)
            throw new ArgumentOutOfRangeException(nameof(width), "LIC: massimo 16.777.216 pixel raster.");
        if (steps is < 0 or > 64 || pixels * (2L * steps + 1) > MaximumSamples)
            throw new ArgumentOutOfRangeException(nameof(steps), "LIC: massimo 400 milioni di campioni e 64 passi per direzione.");
    }

    public static float[] Apply(int width, int height, IReadOnlyList<float> texture,
        IVectorField2D field, int steps = 12, float stepSize = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(field);
        ValidateBudget(width, height, steps);
        if (!float.IsFinite(stepSize) || stepSize is <= 0 or > 16)
            throw new ArgumentOutOfRangeException(nameof(stepSize));
        if (texture.Count != (long)width * height)
            throw new ArgumentException("La texture deve contenere width * height valori.", nameof(texture));
        cancellationToken.ThrowIfCancellationRequested();
        for (int i = 0; i < texture.Count; i++)
        {
            if ((i & 4095) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(texture[i]) || texture[i] is < 0 or > 1)
                throw new ArgumentException("La texture deve contenere valori finiti in [0,1].", nameof(texture));
        }
        var output = new float[texture.Count];

        float Sample(Vector2 p)
        {
            int x0 = (int)p.X, y0 = (int)p.Y;
            int x1 = Math.Min(x0 + 1, width - 1), y1 = Math.Min(y0 + 1, height - 1);
            float tx = p.X - x0, ty = p.Y - y0;
            float top = texture[y0 * width + x0] * (1 - tx) + texture[y0 * width + x1] * tx;
            float bottom = texture[y1 * width + x0] * (1 - tx) + texture[y1 * width + x1] * tx;
            return top * (1 - ty) + bottom * ty;
        }

        for (int y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < width; x++)
            {
                if ((x & 63) == 0) cancellationToken.ThrowIfCancellationRequested();
                double sum = texture[y * width + x], weights = 1;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    var at = new Vector2(x, y);
                    for (int i = 1; i <= steps; i++)
                    {
                        if (!FlowIntegrator.TryAdvance(field, at, sign * stepSize,
                                FlowIntegrationMethod.Rk2, out Vector2 next)
                            || next.X < 0 || next.Y < 0 || next.X > width - 1 || next.Y > height - 1)
                            break;
                        double weight = 1d - (double)i / (steps + 1);
                        sum += Sample(next) * weight;
                        weights += weight;
                        at = next;
                    }
                }
                output[y * width + x] = (float)Math.Clamp(sum / weights, 0, 1);
            }
        }
        return output;
    }
}
