using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Riduce le dimensioni di anteprima senza alterare la ricetta finale, il seed o la fase.
/// Limita il costo anche per il primo Studio; non sostituisce Validate del renderer.
/// </summary>
public static class StudioPreviewPolicy
{
    public static ArtworkRecipe CreatePreview(ArtworkRecipe original, ArtworkRegistry registry,
        int maximumWidth = 800, int maximumHeight = 600, long maximumPixels = 480_000)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(registry);
        if (maximumWidth < 64 || maximumHeight < 64 || maximumPixels < 4096)
            throw new ArgumentOutOfRangeException(nameof(maximumPixels), "Il budget di anteprima è troppo piccolo.");
        original.ValidateAgainst(registry);

        double scale = Math.Min(1d, Math.Min((double)maximumWidth / original.Size.Width,
            Math.Min((double)maximumHeight / original.Size.Height,
                Math.Sqrt((double)maximumPixels / original.Size.PixelCount))));
        int width = Math.Max(1, (int)Math.Floor(original.Size.Width * scale));
        int height = Math.Max(1, (int)Math.Floor(original.Size.Height * scale));
        if (original.Animation is not null && (width < 64 || height < 64))
            throw new ArgumentOutOfRangeException(nameof(original),
                "La ricetta animata produce un'anteprima inferiore a 64 pixel: aumentare le dimensioni.");
        return ArtworkRecipe.Create(registry.Get(original.ArtworkId), new CanvasSize(width, height),
            original.Seed, original.Parameters, original.Animation);
    }
}
