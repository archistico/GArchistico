namespace GenerativeArt.Application.Preferences;

/// <summary>
/// Palette dei token Studio: i valori chiari sono espliciti e verificabili
/// senza riferimenti alla GUI. I token sconosciuti mantengono il loro colore.
/// </summary>
public static class StudioThemePalette
{
    private static readonly IReadOnlyDictionary<string, string> LightColors = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["#10151E"] = "#F0F4F9", ["#192330"] = "#FFFFFF",
        ["#7DD9C2"] = "#176E66", ["#97A9BB"] = "#53677A",
        ["#9EAEC0"] = "#526478", ["#0A0E16"] = "#E4EBF3",
        ["#29394C"] = "#BBC8D6", ["#151F2B"] = "#F8FAFD",
        ["#263546"] = "#CED9E4", ["#182431"] = "#FFFFFF",
        ["#A9E6D7"] = "#13675B", ["#8196AB"] = "#617485",
        ["#283E50"] = "#D7E5F0", ["#A8BDCE"] = "#526678",
        ["#2A6B69"] = "#B5DCD5", ["#223040"] = "#E6EEF6",
        ["#D7E5EE"] = "#253648", ["#FF9A9A"] = "#B12832"
    };

    public static string Resolve(string darkHex, bool lightTheme) =>
        lightTheme && LightColors.TryGetValue(darkHex, out string? lightHex) ? lightHex : darkHex;
}
