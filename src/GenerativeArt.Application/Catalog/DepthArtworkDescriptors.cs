namespace GenerativeArt.Application.Catalog;

public static partial class BuiltInArtworkCatalog
{
    private static ArtworkDescriptor PaperCutDescriptor() => new("paper-cut", 1,
        "Carta ritagliata · tunnel", "Profondità / Carta",
        "Aperture organiche sovrapposte con ombre gaussiane, bordi illuminati e profondità di campo.",
        ArtworkCapabilities.Raster,
        [ArtworkParameterSpec.Integer("layers", "Fogli sovrapposti", 18, 3, 64, "Strati"),
         ArtworkParameterSpec.Number("inner-radius", "Raggio apertura interna / lato minimo", .07, .015, .35, "Strati"),
         ArtworkParameterSpec.Number("outer-radius", "Raggio apertura esterna / lato minimo", .64, .2, 1.4, "Strati"),
         ArtworkParameterSpec.Number("layer-bias", "Progressione distanza strati", 1.1, .3, 3, "Strati"),
         ArtworkParameterSpec.Integer("lobes", "Lobi del contorno", 5, 1, 24, "Geometria"),
         ArtworkParameterSpec.Number("ripple", "Ampiezza lobi", .12, 0, .35, "Geometria"),
         ArtworkParameterSpec.Number("roughness", "Irregolarità organica", .1, 0, .25, "Geometria"),
         ArtworkParameterSpec.Number("twist", "Rotazione fra fondo e fronte (gradi)", 65, -180, 180, "Geometria"),
         ArtworkParameterSpec.Number("drift", "Deriva centro fra strati", .1, 0, .4, "Geometria"),
         ArtworkParameterSpec.Number("stretch", "Allungamento orizzontale aperture", 1.4, .25, 4, "Inquadratura"),
         ArtworkParameterSpec.Number("rotation", "Rotazione composizione (gradi)", -12, -180, 180, "Inquadratura"),
         ArtworkParameterSpec.Number("center-x", "Centro X", .5, 0, 1, "Inquadratura"),
         ArtworkParameterSpec.Number("center-y", "Centro Y", .5, 0, 1, "Inquadratura"),
         ArtworkParameterSpec.Integer("samples", "Campioni per apertura", 1000, 64, 4096, "Qualità"),
         ArtworkParameterSpec.Number("rim-light", "Luce sul bordo", .3, 0, 1, "Luce"),
         ArtworkParameterSpec.Number("rim-width", "Spessore bordo su lato 1000 px", .9, .1, 4, "Luce"),
         .. DepthControls(.3, 5, 8, 3),
         .. DepthColors("lagoon", ["lagoon", "canyon", "ivory"]), .. RasterFinishing()]);

    private static ArtworkDescriptor BokehDescriptor() => new("bokeh-depth", 1,
        "Bokeh · piani di luce", "Profondità / Luce",
        "Particelle illuminate e velature luminose, distribuite per profondità con fuoco selettivo.",
        ArtworkCapabilities.Raster,
        [ArtworkParameterSpec.Integer("particles", "Particelle", 280, 1, 4000, "Composizione"),
         ArtworkParameterSpec.Integer("planes", "Piani di profondità", 12, 3, 32, "Composizione"),
         ArtworkParameterSpec.Choice("layout", "Distribuzione", "flow", ["cloud", "spiral", "flow"], "Composizione"),
         ArtworkParameterSpec.Number("spread", "Estensione della composizione", 1.1, .1, 2, "Composizione"),
         ArtworkParameterSpec.Number("turns", "Spire / onde del flusso", 3.5, .5, 12, "Composizione"),
         ArtworkParameterSpec.Number("radius", "Raggio medio su lato 1000 px", 22, 2, 100, "Particelle"),
         ArtworkParameterSpec.Number("depth-scale", "Ingrandimento verso il primo piano", 3.2, 1, 6, "Particelle"),
         ArtworkParameterSpec.Number("opacity", "Opacità particelle", .55, .02, 1, "Particelle"),
         ArtworkParameterSpec.Number("aperture-ring", "Luce sul bordo dei dischi", .25, 0, 1, "Particelle"),
         ArtworkParameterSpec.Number("light-strength", "Modellazione della luce", .65, 0, 1, "Luce"),
         ArtworkParameterSpec.Number("glow", "Alone diffuso per piano", .18, 0, .7, "Luce"),
         .. DepthControls(.12, 5, 6, 18),
         .. DepthColors("aurora", ["aurora", "amber", "pearl"]), .. RasterFinishing()]);

    private static ArtworkParameterSpec[] DepthControls(double shadow, double softness, double distance, double blur) =>
    [ArtworkParameterSpec.Number("shadow-opacity", "Opacità ombra proiettata", shadow, 0, .7, "Profondità"),
     ArtworkParameterSpec.Number("shadow-softness", "Sfocatura ombra su lato 1000 px", softness, 0, 32, "Profondità"),
     ArtworkParameterSpec.Number("shadow-distance", "Distanza ombra su lato 1000 px", distance, 0, 48, "Profondità"),
     ArtworkParameterSpec.Number("light-angle", "Direzione della luce (gradi)", -55, -180, 180, "Profondità"),
     ArtworkParameterSpec.Number("depth-blur", "Sfocatura fuori fuoco su lato 1000 px", blur, 0, 32, "Profondità"),
     ArtworkParameterSpec.Number("focus", "Piano a fuoco (0 = fondo, 1 = fronte)", .8, 0, 1, "Profondità"),
     ArtworkParameterSpec.Number("focus-range", "Ampiezza zona nitida", .16, 0, 1, "Profondità")];

    private static ArtworkParameterSpec[] DepthColors(string palette, string[] choices) =>
    [ArtworkParameterSpec.Choice("palette", "Palette", palette, choices, "Colori"),
     ArtworkParameterSpec.ColorOverride("background-color", "Sfondo / centro del tunnel"),
     ArtworkParameterSpec.ColorOverride("depth-color-1", "Tono lontano"),
     ArtworkParameterSpec.ColorOverride("depth-color-2", "Tono intermedio"),
     ArtworkParameterSpec.ColorOverride("depth-color-3", "Tono vicino"),
     ArtworkParameterSpec.ColorOverride("highlight-color", "Luci e bordi"),
     ArtworkParameterSpec.ColorOverride("shadow-color", "Ombra proiettata")];

    private static ArtworkParameterSpec[] TerrainDepthControls() =>
    [ArtworkParameterSpec.Number("depth-blur", "Sfocatura lontananza su lato 1000 px", 0, 0, 32, "Profondità", optionalInJson: true),
     ArtworkParameterSpec.Number("soft-shadow", "Opacità ombre gaussiane", 0, 0, .7, "Profondità", optionalInJson: true),
     ArtworkParameterSpec.Number("soft-shadow-softness", "Sfocatura ombre su lato 1000 px", 6, 0, 32, "Profondità", optionalInJson: true),
     ArtworkParameterSpec.Number("soft-shadow-distance", "Distanza ombre su lato 1000 px", 8, 0, 48, "Profondità", optionalInJson: true),
     ArtworkParameterSpec.Number("light-angle", "Direzione luce (gradi)", -55, -180, 180, "Profondità", optionalInJson: true),
     ArtworkParameterSpec.Number("rim-light", "Luce sul bordo dei rilievi", 0, 0, 1, "Profondità", optionalInJson: true)];
}
