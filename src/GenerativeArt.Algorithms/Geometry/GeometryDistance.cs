namespace GenerativeArt.Algorithms.Geometry;

/// <summary>
/// Lunghezza euclidea numericamente stabile. .NET 10 non espone Math.Hypot;
/// il ridimensionamento evita overflow/underflow intermedi nel calcolo x²+y².
/// </summary>
public static class GeometryDistance
{
    public static double Hypotenuse(double x, double y)
    {
        if (double.IsInfinity(x) || double.IsInfinity(y))
            return double.PositiveInfinity;
        if (double.IsNaN(x) || double.IsNaN(y))
            return double.NaN;

        double ax = Math.Abs(x);
        double ay = Math.Abs(y);
        double major = Math.Max(ax, ay);
        if (major == 0d)
            return 0d;
        double minor = Math.Min(ax, ay);
        double ratio = minor / major;
        return major * Math.Sqrt(1d + ratio * ratio);
    }
}
