namespace GenerativeArt.Application.Catalog;

/// <summary>Capacità dichiarate dall'opera; non implica che tutti gli export siano già disponibili.</summary>
[Flags]
public enum ArtworkCapabilities
{
    None = 0,
    Raster = 1,
    Vector = 2,
    Animated = 4,
    SeamlessLoop = 8
}
