using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Contratto headless per la CLI e il futuro Studio. L'output rimane del chiamante:
/// il renderer non chiude lo stream e genera un solo PNG per invocazione.
/// Per un'opera animata frameIndex è obbligatorio; per una statica dev'essere null.
/// </summary>
public interface IArtworkRasterRenderer
{
    void Validate(ArtworkRecipe recipe, int? frameIndex = null);
    void Render(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
        CancellationToken cancellationToken = default);
}
