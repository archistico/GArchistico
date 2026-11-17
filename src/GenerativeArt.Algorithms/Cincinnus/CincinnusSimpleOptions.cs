namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Parametri del posizionatore Cincinnus Simple, indipendenti dal raster.
/// I valori geometrici sono in pixel del canvas, non in pixel di supersampling.
/// </summary>
public sealed record CincinnusSimpleOptions
{
    /// <summary>Distanza nominale fra le linee (Gȧrt GAP = 110).</summary>
    public float Separation { get; init; } = 110f;

    /// <summary>Distanza minima d'arresto; null = metà Separation, come Streamlines.kt.</summary>
    public float? TestDistance { get; init; }

    /// <summary>Passo RK4 delle spine (Gȧrt DS = 3).</summary>
    public float StepSize { get; init; } = 3f;

    /// <summary>Lunghezza minima per accettare una linea (Simple = 300).</summary>
    public float MinimumLength { get; init; } = 300f;

    /// <summary>
    /// Budget complessivo per entrambe le direzioni di una linea. Gȧrt ammette infinito;
    /// il porting lo limita esplicitamente a 9000 per garantire lavoro finito.
    /// </summary>
    public float MaximumLength { get; init; } = 9000f;

    /// <summary>Margine oltre la tela; null = 3.75 * Separation (3 volte la mezza larghezza originale).</summary>
    public float? Margin { get; init; }

    /// <summary>Cap massimo alle linee accettate; non influisce sull'ordine prima del raggiungimento.</summary>
    public int MaximumLines { get; init; } = 2048;

    /// <summary>Cap globale ai vertici accettati e temporanei nell'indice spaziale.</summary>
    public int MaximumPoints { get; init; } = 250_000;

    /// <summary>Cap ai candidati laterali in attesa; l'ordine delle inserzioni viene mantenuto.</summary>
    public int MaximumPendingSeeds { get; init; } = 500_000;

    /// <summary>Protezione sulla quantità di celle della griglia iniziale di copertura.</summary>
    public int MaximumCoverCells { get; init; } = 150_000;
}
