using System.Numerics;

namespace GenerativeArt.Core.Geometry;

/// <summary>
/// Trasformazioni affini 2D secondo la convenzione row-vector di System.Numerics:
/// un punto p viene trasformato da p * M. Then(first, second) applica first e poi second.
/// Tutte le coordinate e le matrici pubbliche devono essere finite.
/// </summary>
public static class Transform2D
{
    public static Matrix3x2 Translate(Vector2 offset)
    {
        ValidateVector(offset, nameof(offset));
        return Matrix3x2.CreateTranslation(offset);
    }

    public static Matrix3x2 Rotate(float radians, Vector2 center = default)
    {
        ValidateFinite(radians, nameof(radians));
        ValidateVector(center, nameof(center));
        return Matrix3x2.CreateRotation(radians, center);
    }

    /// <summary>Scale zero ammesse: la matrice può risultare non invertibile.</summary>
    public static Matrix3x2 Scale(Vector2 factors, Vector2 center = default)
    {
        ValidateVector(factors, nameof(factors));
        ValidateVector(center, nameof(center));
        return Matrix3x2.CreateScale(factors, center);
    }

    /// <summary>Composizione nell'ordine applicativo: prima first, poi second.</summary>
    public static Matrix3x2 Then(Matrix3x2 first, Matrix3x2 second)
    {
        ValidateMatrix(first, nameof(first));
        ValidateMatrix(second, nameof(second));
        Matrix3x2 result = first * second;
        if (!IsFinite(result))
            throw new OverflowException("La composizione delle trasformazioni produce valori non finiti.");
        return result;
    }

    public static Vector2 Point(Matrix3x2 transform, Vector2 point)
    {
        ValidateMatrix(transform, nameof(transform));
        ValidateVector(point, nameof(point));
        Vector2 result = Vector2.Transform(point, transform);
        if (!float.IsFinite(result.X) || !float.IsFinite(result.Y))
            throw new OverflowException("La trasformazione del punto produce coordinate non finite.");
        return result;
    }

    /// <summary>Trasforma un vettore, ignorando la componente di traslazione.</summary>
    public static Vector2 Direction(Matrix3x2 transform, Vector2 direction)
    {
        ValidateMatrix(transform, nameof(transform));
        ValidateVector(direction, nameof(direction));
        Vector2 result = Vector2.TransformNormal(direction, transform);
        if (!float.IsFinite(result.X) || !float.IsFinite(result.Y))
            throw new OverflowException("La trasformazione del vettore produce coordinate non finite.");
        return result;
    }

    /// <summary>Restituisce false per matrici singolari, invece di creare una falsa inversa.</summary>
    public static bool TryInvert(Matrix3x2 matrix, out Matrix3x2 inverse)
    {
        ValidateMatrix(matrix, nameof(matrix));
        if (Matrix3x2.Invert(matrix, out inverse) && IsFinite(inverse))
            return true;
        inverse = default;
        return false;
    }

    public static Matrix3x2 Invert(Matrix3x2 matrix)
    {
        if (!TryInvert(matrix, out Matrix3x2 inverse))
            throw new InvalidOperationException("La trasformazione non è invertibile.");
        return inverse;
    }

    private static void ValidateFinite(float value, string name)
    {
        if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(name);
    }

    private static void ValidateVector(Vector2 value, string name)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
            throw new ArgumentOutOfRangeException(name, "Le componenti devono essere finite.");
    }

    private static void ValidateMatrix(Matrix3x2 matrix, string name)
    {
        if (!IsFinite(matrix))
            throw new ArgumentOutOfRangeException(name, "La matrice deve contenere soltanto valori finiti.");
    }

    private static bool IsFinite(Matrix3x2 m) =>
        float.IsFinite(m.M11) && float.IsFinite(m.M12) &&
        float.IsFinite(m.M21) && float.IsFinite(m.M22) &&
        float.IsFinite(m.M31) && float.IsFinite(m.M32);
}
