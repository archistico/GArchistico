using System.Numerics;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class Transform2DTests
{
    private static void Close(Vector2 expected, Vector2 actual, float tolerance = 0.0001f)
    {
        Assert.InRange(Vector2.Distance(expected, actual), 0f, tolerance);
    }

    [Fact]
    public void CompositionUsesRowVectorOrder()
    {
        Matrix3x2 translate = Transform2D.Translate(new Vector2(10, 20));
        Matrix3x2 rotate = Transform2D.Rotate(MathF.PI / 2f);
        Vector2 p = new(1, 2);
        Close(new Vector2(-22, 11), Transform2D.Point(Transform2D.Then(translate, rotate), p));
        Close(new Vector2(8, 21), Transform2D.Point(Transform2D.Then(rotate, translate), p));
    }

    [Fact]
    public void TranslationDoesNotChangeDirection()
    {
        Matrix3x2 translation = Transform2D.Translate(new Vector2(100, -35));
        Close(new Vector2(5, 3), Transform2D.Direction(translation, new Vector2(5, 3)));
        Close(new Vector2(105, -32), Transform2D.Point(translation, new Vector2(5, 3)));
    }

    [Fact]
    public void RotationAroundPivotLeavesPivotInPlace()
    {
        Vector2 pivot = new(3, -5);
        Matrix3x2 matrix = Transform2D.Rotate(MathF.PI, pivot);
        Close(pivot, Transform2D.Point(matrix, pivot));
        Close(new Vector2(2, -5), Transform2D.Point(matrix, new Vector2(4, -5)));
    }

    [Fact]
    public void ScalingCanReflectAndCanBeInverted()
    {
        Matrix3x2 matrix = Transform2D.Then(
            Transform2D.Scale(new Vector2(-2, 3)),
            Transform2D.Translate(new Vector2(9, -7)));
        Vector2 original = new(2, 4);
        Vector2 transformed = Transform2D.Point(matrix, original);
        Assert.True(Transform2D.TryInvert(matrix, out Matrix3x2 inverse));
        Close(original, Transform2D.Point(inverse, transformed));
        Close(original, Transform2D.Point(Transform2D.Invert(matrix), transformed));
    }

    [Fact]
    public void ZeroScaleIsSingular()
    {
        Matrix3x2 matrix = Transform2D.Scale(new Vector2(0, 2));
        Assert.False(Transform2D.TryInvert(matrix, out _));
        Assert.Throws<InvalidOperationException>(() => Transform2D.Invert(matrix));
    }

    [Fact]
    public void FiniteInputValidation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Transform2D.Rotate(float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => Transform2D.Translate(new Vector2(float.PositiveInfinity, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Transform2D.Scale(new Vector2(1, float.NaN)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Transform2D.Point(Matrix3x2.Identity, new Vector2(0, float.NaN)));
        var bad = new Matrix3x2(1, 0, 0, 1, float.NaN, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => Transform2D.TryInvert(bad, out _));
    }

    [Fact]
    public void OverflowInTransformIsExplicit()
    {
        Matrix3x2 extreme = Transform2D.Scale(new Vector2(float.MaxValue, 1));
        Assert.Throws<OverflowException>(() => Transform2D.Point(extreme, new Vector2(2, 0)));
        Assert.Throws<OverflowException>(() => Transform2D.Then(extreme, extreme));
    }
}
