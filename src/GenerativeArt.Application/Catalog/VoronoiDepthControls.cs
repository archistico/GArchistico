namespace GenerativeArt.Application.Catalog;

public static class VoronoiDepthControls
{
    public static IReadOnlyList<ArtworkParameterSpec> All { get; } = Array.AsReadOnly<ArtworkParameterSpec>([
        ArtworkParameterSpec.Choice("depth-style", "Composizione del mosaico", "flat", ["flat", "layered"], "Piani", optionalInJson: true),
        ArtworkParameterSpec.Integer("depth-layers", "Piani Voronoi sovrapposti", 4, 2, 8, "Piani", optionalInJson: true),
        ArtworkParameterSpec.Number("missing-tiles", "Frazione tessere mancanti (0–1)", .38, 0, 1, "Aperture", optionalInJson: true),
        ArtworkParameterSpec.Choice("hole-pattern", "Distribuzione aperture", "clustered", ["random", "clustered"], "Aperture", optionalInJson: true),
        ArtworkParameterSpec.Number("back-density", "Densità tessere sul fondo", 1.65, 1, 3, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Number("parallax-x", "Scarto X fra piani su lato 1000 px", 24, -120, 120, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Number("parallax-y", "Scarto Y fra piani su lato 1000 px", -16, -120, 120, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Number("distance-dimming", "Oscuramento dei piani lontani", .5, 0, .85, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Number("atmosphere", "Velatura atmosferica sul fondo", .15, 0, .8, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Number("depth-blur", "Sfocatura fondo su lato 1000 px", 7, 0, 32, "Distanza", optionalInJson: true),
        ArtworkParameterSpec.Choice("back-palette", "Palette dei piani arretrati", "same", ["same", "glass", "copper", "lagoon"], "Colori", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-opacity", "Opacità ombre fra piani", .5, 0, .9, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-softness", "Sfocatura ombre su lato 1000 px", 6, 0, 24, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-distance", "Distanza ombre su lato 1000 px", 14, 0, 48, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("light-angle", "Direzione luce (gradi)", -55, -180, 180, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("tile-relief", "Rilievo delle tessere", .7, 0, 1, "Tessere", optionalInJson: true),
        ArtworkParameterSpec.Number("bevel-width", "Smusso bordi su lato 1000 px", 3, 0, 12, "Tessere", optionalInJson: true),
        ArtworkParameterSpec.Number("edge-opacity", "Opacità contorno tessere", .35, 0, 1, "Tessere", optionalInJson: true),
        ArtworkParameterSpec.Number("tile-variation", "Variazione tonale delle tessere", .2, 0, .5, "Tessere", optionalInJson: true),
        ArtworkParameterSpec.Number("accent-strength", "Intensità inserti luminosi", 0, 0, 1, "Tessere", optionalInJson: true),
        ArtworkParameterSpec.ColorOverride("shadow-color", "Ombra fra i mosaici"),
        ArtworkParameterSpec.ColorOverride("depth-color", "Velatura sul fondo")
    ]);
}
