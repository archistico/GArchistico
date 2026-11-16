namespace GenerativeArt.Application.Preferences;

/// <summary>
/// Stato leggero per la sola interfaccia. Non modifica né il formato delle ricette
/// né i risultati del motore deterministico; i percorsi rimangono solo sul PC.
/// </summary>
public sealed class StudioPreferences
{
    public const int MaximumRecentRecipes = 8;
    public const int MaximumFavorites = 128;
    public bool UseLightTheme { get; }
    public IReadOnlyList<string> FavoriteArtworkIds { get; }
    public IReadOnlyList<string> RecentRecipePaths { get; }

    public StudioPreferences(bool useLightTheme = false,
        IEnumerable<string>? favoriteArtworkIds = null,
        IEnumerable<string>? recentRecipePaths = null)
    {
        UseLightTheme = useLightTheme;
        FavoriteArtworkIds = Array.AsReadOnly((favoriteArtworkIds ?? []).Where(IsValidArtworkId)
            .Distinct(StringComparer.Ordinal).Take(MaximumFavorites).ToArray());
        RecentRecipePaths = Array.AsReadOnly((recentRecipePaths ?? [])
            .Select(TryNormalizePath).Where(path => path is not null).Select(path => path!)
            .Distinct(PathComparer).Take(MaximumRecentRecipes).ToArray());
    }

    public bool IsFavorite(string artworkId) => FavoriteArtworkIds.Contains(artworkId, StringComparer.Ordinal);

    public StudioPreferences WithTheme(bool useLightTheme) =>
        new(useLightTheme, FavoriteArtworkIds, RecentRecipePaths);

    public StudioPreferences ToggleFavorite(string artworkId)
    {
        if (!IsValidArtworkId(artworkId))
            throw new ArgumentException("Identificativo opera non valido.", nameof(artworkId));
        string[] updated = IsFavorite(artworkId)
            ? FavoriteArtworkIds.Where(id => id != artworkId).ToArray()
            : FavoriteArtworkIds.Append(artworkId).ToArray();
        return new StudioPreferences(UseLightTheme, updated, RecentRecipePaths);
    }

    public StudioPreferences AddRecentRecipe(string path)
    {
        string? fullPath = TryNormalizePath(path);
        if (fullPath is null)
            throw new ArgumentException("Percorso ricetta non valido.", nameof(path));
        return new StudioPreferences(UseLightTheme, FavoriteArtworkIds,
            new[] { fullPath }.Concat(RecentRecipePaths.Where(p => !PathComparer.Equals(p, fullPath))));
    }

    public StudioPreferences RemoveRecentRecipe(string path) =>
        new(UseLightTheme, FavoriteArtworkIds,
            RecentRecipePaths.Where(p => !PathComparer.Equals(p, path)));

    public StudioPreferences ClearRecentRecipes() =>
        new(UseLightTheme, FavoriteArtworkIds);

    private static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static bool IsValidArtworkId(string? id) => id is { Length: > 0 and <= 100 }
        && id.All(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-');

    private static string? TryNormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 2048 || path.Any(char.IsControl))
            return null;
        try
        {
            return Path.IsPathFullyQualified(path) ? Path.GetFullPath(path) : null;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }
}
