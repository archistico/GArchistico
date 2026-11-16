namespace GenerativeArt.Core.Pixels;

/// <summary>
/// Layout logico del PixelBuffer. Il numero uint contiene 0xAARRGGBB,
/// con canali colore non premoltiplicati (straight alpha).
/// Non descrive la sequenza dei byte fisici in memoria: l'endianness è esterno al contratto.
/// </summary>
public enum PixelStorageFormat
{
    PackedArgb32Straight = 1
}
