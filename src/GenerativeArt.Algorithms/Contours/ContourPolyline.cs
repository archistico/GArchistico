using System.Numerics;

namespace GenerativeArt.Algorithms.Contours;

/// <summary>
/// Una curva di livello già raccordata. Le coordinate sono espresse in unità della
/// griglia di campionamento; se Closed è true, l'ultimo punto coincide col primo.
/// </summary>
public sealed class ContourPolyline
{
    public IReadOnlyList<Vector2> Points { get; }
    public bool Closed { get; }

    internal ContourPolyline(Vector2[] points, bool closed)
    {
        Points = Array.AsReadOnly(points);
        Closed = closed;
    }
}
