namespace GenerativeArt.Application.Catalog;

/// <summary>Controlli opzionali: i default conservano il percorso dei renderer iniziali.</summary>
public static class LegacyArtworkControls
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<ArtworkParameterSpec>> Controls =
        new Dictionary<string, IReadOnlyList<ArtworkParameterSpec>>(StringComparer.Ordinal)
        {
            ["parametric-curves"] = Array.AsReadOnly<ArtworkParameterSpec>([
                C("style", "Composizione", "legacy", ["legacy", "harmonic", "woven"], "Composizione"),
                I("frequency-x", "Frequenza orizzontale", 5, 1, 32, "Geometria"),
                I("frequency-y", "Frequenza verticale", 4, 1, 32, "Geometria"),
                N("phase", "Fase verticale (radianti)", .4, -3.14, 3.14, "Geometria"),
                I("layers", "Curve sovrapposte", 1, 1, 160, "Composizione"),
                N("layer-scale", "Scala curva interna", .8, .1, 1, "Composizione"),
                N("twist", "Rotazione fra curve (gradi)", 0, -180, 180, "Composizione"),
                N("rotation", "Rotazione complessiva (gradi)", 0, -180, 180, "Inquadratura"),
                N("stretch-x", "Scala orizzontale", 1, .3, 2, "Inquadratura"),
                N("stretch-y", "Scala verticale", 1, .3, 2, "Inquadratura"),
                I("samples", "Precisione curva", 960, 256, 24000, "Qualità"),
                N("line-width", "Spessore curve", 1.8, .1, 12, "Tratto"),
                N("orbit-width", "Spessore anello (0 = nascosto)", 1.5, 0, 12, "Tratto")]),
            ["noise-flow"] = Array.AsReadOnly<ArtworkParameterSpec>([
                C("field", "Campo vettoriale", "legacy", ["legacy", "curl", "vortex", "braid"], "Campo"),
                N("frequency", "Frequenza campo", .005, .0005, .04, "Campo"),
                N("curvature", "Torsione campo angolare", 3.7, .2, 10, "Campo"),
                C("start-layout", "Distribuzione partenze", "random", ["random", "grid", "edge"], "Composizione"),
                I("steps", "Passi per traccia", 320, 16, 1000, "Dettaglio"),
                N("step-size", "Lunghezza del passo", 2, .25, 8, "Dettaglio"),
                N("line-width", "Spessore tracce", 1.2, .1, 10, "Tratto"),
                N("opacity", "Opacità tracce", 1, .02, 1, "Tratto")]),
            ["plasma-noise"] = Array.AsReadOnly<ArtworkParameterSpec>([
                C("pattern", "Disegno", "fbm", ["fbm", "turbulence", "ridged", "marble"], "Composizione"),
                N("lacunarity", "Rapporto frequenze ottave", 2, 1, 3.5, "Noise"),
                N("persistence", "Peso dettaglio fine", .5, .1, .9, "Noise"),
                I("bands", "Venature / livelli di contorno", 8, 2, 32, "Composizione"),
                N("rotation", "Rotazione campo (gradi)", 0, -180, 180, "Inquadratura"),
                N("stretch-x", "Scala orizzontale", 1, .25, 4, "Inquadratura"),
                N("stretch-y", "Scala verticale", 1, .25, 4, "Inquadratura"),
                N("contrast", "Contrasto", 1, .1, 4, "Toni"),
                N("gamma", "Distribuzione mezzitoni", 1, .25, 4, "Toni"),
                N("contour-strength", "Incisione contorni", 0, 0, 1, "Toni")]),
            ["spaced-streamlines"] = Array.AsReadOnly<ArtworkParameterSpec>([
                C("field", "Campo vettoriale", "curl", ["curl", "vortex", "braid"], "Campo"),
                N("frequency", "Frequenza campo", .0065, .0005, .04, "Campo"),
                I("max-steps", "Passi per direzione", 190, 16, 800, "Dettaglio"),
                N("integration-step", "Lunghezza massima del passo", 2.4, .25, 8, "Dettaglio"),
                N("opacity", "Opacità tracce", 1, .02, 1, "Tratto")]),
            ["looping-flow"] = Array.AsReadOnly<ArtworkParameterSpec>([
                I("lines", "Tracce (0 = automatico storico)", 0, 0, 2000, "Composizione"),
                I("steps", "Passi per traccia", 95, 16, 800, "Dettaglio"),
                N("frequency", "Frequenza campo", .006, .0005, .04, "Campo"),
                N("step-size", "Lunghezza del passo", 2.25, .25, 8, "Dettaglio"),
                N("line-width", "Spessore tracce", 1.35, .1, 8, "Tratto"),
                N("opacity", "Opacità tracce", 1, .02, 1, "Tratto")])
        };

    public static IReadOnlyList<ArtworkParameterSpec> ForArtwork(string id) =>
        Controls.TryGetValue(id, out var controls) ? controls : Array.Empty<ArtworkParameterSpec>();

    private static ArtworkParameterSpec N(string id, string name, double value, double min, double max, string group) =>
        ArtworkParameterSpec.Number(id, name, value, min, max, group, optionalInJson: true);
    private static ArtworkParameterSpec I(string id, string name, int value, int min, int max, string group) =>
        ArtworkParameterSpec.Integer(id, name, value, min, max, group, optionalInJson: true);
    private static ArtworkParameterSpec C(string id, string name, string value, string[] choices, string group) =>
        ArtworkParameterSpec.Choice(id, name, value, choices, group, optionalInJson: true);
}
