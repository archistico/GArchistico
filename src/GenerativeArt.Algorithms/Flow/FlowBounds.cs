using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>Rettangolo con bordo sinistro/superiore incluso e destro/inferiore escluso.</summary>
public readonly struct FlowBounds
{
    public float Left { get; }
    public float Top { get; }
    public float Right { get; }
    public float Bottom { get; }

    public FlowBounds(float left, float top, float right, float bottom)
    {
        if (!float.IsFinite(left) || !float.IsFinite(top) || !float.IsFinite(right)
            || !float.IsFinite(bottom) || !(right > left) || !(bottom > top))
            throw new ArgumentOutOfRangeException(nameof(right), "I limiti devono essere finiti e non vuoti.");
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public bool Contains(Vector2 point) =>
        point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
}
