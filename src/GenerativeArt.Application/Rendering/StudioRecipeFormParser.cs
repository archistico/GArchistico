using System.Globalization;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Converte lo stato testuale dell'editor Studio in una ricetta immutabile.
/// Non dipende da Avalonia: la validazione è condivisibile con i test headless.
/// </summary>
public static class StudioRecipeFormParser
{
    public static ArtworkRecipe Parse(
        ArtworkDescriptor descriptor,
        string? seedText,
        string? widthText,
        string? heightText,
        IReadOnlyDictionary<string, string> parameters,
        IReadOnlyCollection<string> invalidInputs,
        string? framesText = null,
        string? fpsText = null)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(invalidInputs);
        if (invalidInputs.Count > 0)
            throw new ArgumentException("Correggere i valori evidenziati in rosso prima di salvare o esportare.");
        if (!ulong.TryParse(seedText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed))
            throw new ArgumentException("Seed non valido: inserire un UInt64 decimale da 0 a 18446744073709551615.");
        if (!int.TryParse(widthText, NumberStyles.None, CultureInfo.InvariantCulture, out int width) ||
            !int.TryParse(heightText, NumberStyles.None, CultureInfo.InvariantCulture, out int height))
            throw new ArgumentException("Larghezza e altezza devono essere interi positivi.");

        RecipeAnimation? animation = null;
        if (descriptor.SupportsAnimation)
        {
            if (!int.TryParse(framesText, NumberStyles.None, CultureInfo.InvariantCulture, out int frames) ||
                !int.TryParse(fpsText, NumberStyles.None, CultureInfo.InvariantCulture, out int fps))
                throw new ArgumentException("Frame e FPS devono essere interi.");
            animation = new RecipeAnimation(frames, fps);
        }

        return ArtworkRecipe.Create(descriptor, new CanvasSize(width, height), seed, parameters, animation);
    }
}
