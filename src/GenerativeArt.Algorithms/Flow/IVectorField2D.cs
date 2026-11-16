using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// Campo vettoriale 2D in coordinate canvas (X a destra, Y verso il basso).
/// Il risultato di Sample rappresenta una direzione o una velocità a seconda
/// della modalità di integrazione. Vector2 è un value type e non alloca heap.
/// </summary>
public interface IVectorField2D
{
    Vector2 Sample(float x, float y);
}
