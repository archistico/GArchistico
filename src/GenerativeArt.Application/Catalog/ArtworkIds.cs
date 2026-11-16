namespace GenerativeArt.Application.Catalog;

/// <summary>Identificatori stabili e sicuri per CLI, ricette e file.</summary>
public static class ArtworkIds
{
    public static void Validate(string id, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64 || id[0] < 'a' || id[0] > 'z')
            throw new ArgumentException("L'ID deve iniziare con una lettera ASCII minuscola (max 64 caratteri).", argumentName);
        bool previousDash = false;
        foreach (char c in id)
        {
            bool isDash = c == '-';
            if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || isDash)
                || (isDash && previousDash))
                throw new ArgumentException("L'ID consente soltanto a-z, 0-9 e trattini singoli.", argumentName);
            previousDash = isDash;
        }
        if (previousDash) throw new ArgumentException("L'ID non può terminare con un trattino.", argumentName);
    }
}
