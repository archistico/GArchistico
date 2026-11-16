using System.Numerics;
using GenerativeArt.Algorithms.Contours;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class MarchingSquaresTests
{
    [Fact]
    public void ConstantFieldHasNoContoursIncludingExactIsovalue()
    {
        float[] flat = Enumerable.Repeat(1f, 9).ToArray();
        Assert.Empty(MarchingSquares.Trace(flat, 3, 3, 1));
        Assert.Empty(MarchingSquares.Trace(flat, 3, 3, 2));
        Assert.Empty(MarchingSquares.Trace(flat, 3, 3, 0));
    }

    [Fact]
    public void PlanarFieldStitchesAcrossCellBoundaries()
    {
        float[] slope = [0, 1, 2, 0, 1, 2, 0, 1, 2];
        ContourPolyline line = Assert.Single(MarchingSquares.Trace(slope, 3, 3, .5f));
        Assert.False(line.Closed);
        Assert.Equal(3, line.Points.Count);
        Assert.All(line.Points, p => Assert.Equal(.5f, p.X, 5));
        Assert.Equal(new[] { 0f, 1f, 2f }, line.Points.Select(p => p.Y).Order().ToArray());
    }

    [Theory]
    [InlineData(3f, true)]
    [InlineData(1.5f, false)]
    public void AmbiguousSaddleUsesBilinearDecider(float high, bool highConnected)
    {
        // TL and BR are high: mask 5; both possible topologies are tested.
        float[] values = [high, 0, 0, high];
        IReadOnlyList<ContourPolyline> lines = MarchingSquares.Trace(values, 2, 2, 1);
        Assert.Equal(2, lines.Count);
        Assert.All(lines, line => { Assert.False(line.Closed); Assert.Equal(2, line.Points.Count); });
        bool topRight = ContainsConnection(lines, new Vector2((high - 1) / high, 0), new Vector2(1, 1 / high));
        Assert.Equal(highConnected, topRight);
        // There must be four distinct border intersections, no crossing in the cell.
        Assert.Equal(4, lines.SelectMany(line => line.Points).Distinct().Count());
    }

    [Theory]
    [InlineData(3f, true)]
    [InlineData(1.5f, false)]
    public void ComplementarySaddleCaseTenUsesSameDecider(float high, bool highConnected)
    {
        // TR and BL are high: mask 10.
        float[] values = [0, high, high, 0];
        IReadOnlyList<ContourPolyline> lines = MarchingSquares.Trace(values, 2, 2, 1);
        Assert.Equal(2, lines.Count);
        bool topLeft = ContainsConnection(lines,
            new Vector2(1 / high, 0), new Vector2(0, 1 / high));
        Assert.Equal(highConnected, topLeft);
    }

    [Fact]
    public void ExactLevelVerticesDoNotProduceZeroLengthSegments()
    {
        float[] values = [1, 0, 0, 2];
        foreach (ContourPolyline line in MarchingSquares.Trace(values, 2, 2, 1))
        {
            for (int i = 1; i < line.Points.Count; i++)
                Assert.True(Vector2.Distance(line.Points[i - 1], line.Points[i]) > 0);
        }
    }

    [Fact]
    public void IsolatedPeakProducesSingleClosedLoopWithRepeatedEndpoint()
    {
        float[] values = [0, 0, 0, 0, 3, 0, 0, 0, 0];
        ContourPolyline loop = Assert.Single(MarchingSquares.Trace(values, 3, 3, 1));
        Assert.True(loop.Closed);
        Assert.Equal(5, loop.Points.Count);
        Assert.Equal(loop.Points[0], loop.Points[^1]);
        Assert.Equal(4, loop.Points.Take(4).Distinct().Count());
    }

    [Fact]
    public void IdenticalInputHasIdenticalOrderedPolylineCoordinates()
    {
        float[] values = [0, 2, 1, 3, 2, 0, 3, 0, 1, 2, 3, 0];
        var a = MarchingSquares.Trace(values, 4, 3, 1.3f);
        var b = MarchingSquares.Trace(values, 4, 3, 1.3f);
        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].Closed, b[i].Closed);
            Assert.Equal(a[i].Points.ToArray(), b[i].Points.ToArray());
        }
    }

    [Fact]
    public void InvalidGridAndNonFiniteValuesAndCancellationAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MarchingSquares.Trace(new float[2], 1, 2, 0));
        Assert.Throws<ArgumentException>(() => MarchingSquares.Trace(new float[3], 2, 2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MarchingSquares.Trace(new float[4], 2, 2, float.NaN));
        Assert.Throws<ArgumentException>(() => MarchingSquares.Trace(new float[] { 0, 1, float.PositiveInfinity, 2 }, 2, 2, 1));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => MarchingSquares.Trace(new float[4], 2, 2, 0, cts.Token));
    }

    private static bool ContainsConnection(IEnumerable<ContourPolyline> lines, Vector2 a, Vector2 b) =>
        lines.Any(line => Near(line.Points[0], a) && Near(line.Points[^1], b) ||
                          Near(line.Points[0], b) && Near(line.Points[^1], a));

    private static bool Near(Vector2 a, Vector2 b) => Vector2.Distance(a, b) < 0.0001f;
}
