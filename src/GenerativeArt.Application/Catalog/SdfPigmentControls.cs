namespace GenerativeArt.Application.Catalog;

public static class SdfPigmentControls
{
    public static IReadOnlyList<ArtworkParameterSpec> All { get; } = Array.AsReadOnly<ArtworkParameterSpec>([
        ArtworkParameterSpec.Choice("style", "Stile SDF", "classic", ["classic", "pigment", "glass"], "Composizione", optionalInJson: true),
        ArtworkParameterSpec.Choice("layout", "Distribuzione macchie", "cluster", ["cluster", "orbital", "river"], "Composizione", optionalInJson: true),
        ArtworkParameterSpec.Integer("blobs", "Macchie per piano", 10, 2, 40, "Composizione", optionalInJson: true),
        ArtworkParameterSpec.Integer("layers", "Piani sovrapposti", 3, 1, 6, "Composizione", optionalInJson: true),
        ArtworkParameterSpec.Number("blob-radius", "Raggio macchie / lato minimo", .18, .03, .36, "Macchie", optionalInJson: true),
        ArtworkParameterSpec.Number("radius-variation", "Variazione dimensioni", .55, 0, .85, "Macchie", optionalInJson: true),
        ArtworkParameterSpec.Number("elongation", "Allungamento delle macchie", 1.7, 1, 3, "Macchie", optionalInJson: true),
        ArtworkParameterSpec.Number("spread", "Estensione della composizione", 1, .1, 1.5, "Macchie", optionalInJson: true),
        ArtworkParameterSpec.Number("warp-amount", "Deformazione organica", .055, 0, .2, "Deformazione", optionalInJson: true),
        ArtworkParameterSpec.Number("warp-frequency", "Dettaglio della deformazione", 2.8, .3, 10, "Deformazione", optionalInJson: true),
        ArtworkParameterSpec.Number("color-mix", "Fusione fra pigmenti", .85, 0, 1, "Pigmenti", optionalInJson: true),
        ArtworkParameterSpec.Choice("color-mode", "Miscela colore", "pigment", ["linear", "pigment"], "Pigmenti", optionalInJson: true),
        ArtworkParameterSpec.Number("marbling", "Venature nei colori", .35, 0, 1, "Pigmenti", optionalInJson: true),
        ArtworkParameterSpec.Number("marble-frequency", "Frequenza delle venature", 9, 1, 32, "Pigmenti", optionalInJson: true),
        ArtworkParameterSpec.Number("opacity", "Opacità dei piani", .9, .05, 1, "Pigmenti", optionalInJson: true),
        ArtworkParameterSpec.Number("contour-opacity", "Fasce interne (0 = nascoste)", .08, 0, 1, "Dettaglio", optionalInJson: true),
        ArtworkParameterSpec.Number("outline", "Contorno esterno (0 = nascosto)", .08, 0, 1, "Dettaglio", optionalInJson: true),
        ArtworkParameterSpec.Number("relief", "Rilievo delle macchie", .45, 0, 1, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("highlight", "Riflessi e bordi illuminati", .35, 0, 1, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("light-angle", "Direzione luce (gradi)", -55, -180, 180, "Luce", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-opacity", "Ombre fra i piani", .2, 0, .7, "Profondità", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-softness", "Sfocatura ombre su lato 1000 px", 6, 0, 32, "Profondità", optionalInJson: true),
        ArtworkParameterSpec.Number("shadow-distance", "Distanza ombre su lato 1000 px", 8, 0, 48, "Profondità", optionalInJson: true),
        ArtworkParameterSpec.Number("depth-blur", "Sfocatura piani lontani su lato 1000 px", 3, 0, 32, "Profondità", optionalInJson: true),
        ArtworkParameterSpec.Number("rotation", "Rotazione (gradi)", 0, -180, 180, "Inquadratura", optionalInJson: true),
        ArtworkParameterSpec.Number("stretch-x", "Scala orizzontale", 1, .3, 3, "Inquadratura", optionalInJson: true),
        ArtworkParameterSpec.Number("stretch-y", "Scala verticale", 1, .3, 3, "Inquadratura", optionalInJson: true),
        ArtworkParameterSpec.Number("center-x", "Centro X", .5, 0, 1, "Inquadratura", optionalInJson: true),
        ArtworkParameterSpec.Number("center-y", "Centro Y", .5, 0, 1, "Inquadratura", optionalInJson: true),
        ArtworkParameterSpec.ColorOverride("pigment-color-4", "Pigmento 4"),
        ArtworkParameterSpec.ColorOverride("shadow-color", "Ombra proiettata")
    ]);
}
