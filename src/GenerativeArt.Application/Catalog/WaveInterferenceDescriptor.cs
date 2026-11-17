namespace GenerativeArt.Application.Catalog;

public static partial class BuiltInArtworkCatalog
{
    private static ArtworkDescriptor WaveInterferenceDescriptor() => new("wave-interference", 1,
        "Interferenze d'onda · Moiré", "Campi scalari",
        "Sorgenti radiali sovrapposte: interferenze, caustiche e nastri deformati, con palette e seed controllabili.",
        ArtworkCapabilities.Raster,
        [ArtworkParameterSpec.Integer("sources", "Sorgenti d'onda", 5, 2, 12, "Onde"),
         ArtworkParameterSpec.Choice("layout", "Disposizione", "ring", ["ring", "line", "scatter"], "Onde"),
         ArtworkParameterSpec.Number("spread", "Distanza sorgenti", 1, 0.1, 3, "Onde"),
         ArtworkParameterSpec.Number("frequency", "Frequenza spaziale", 12, 2, 40, "Onde"),
         ArtworkParameterSpec.Number("falloff", "Attenuazione", 0.5, 0, 4, "Onde"),
         ArtworkParameterSpec.Choice("mode", "Stile interferenza", "interference",
            ["interference", "caustics", "ribbons"], "Composizione"),
         ArtworkParameterSpec.Number("distortion", "Deformazione fluida", 0.2, 0, 1, "Composizione"),
         ArtworkParameterSpec.Number("contrast", "Contrasto", 1.3, 0.3, 4, "Toni"),
         ArtworkParameterSpec.Number("gamma", "Distribuzione mezzitoni", 1, 0.25, 4, "Toni"),
         ArtworkParameterSpec.Choice("palette", "Palette", "lagoon", ["lagoon", "copper", "nocturne"], "Colori"),
         ArtworkParameterSpec.ColorOverride("gradient-color-1", "Gradiente · ombra"),
         ArtworkParameterSpec.ColorOverride("gradient-color-2", "Gradiente · mezzotono"),
         ArtworkParameterSpec.ColorOverride("gradient-color-3", "Gradiente · luce"),
         ArtworkParameterSpec.ColorOverride("gradient-color-4", "Gradiente · apice"),
         .. RasterFinishing()]);
}
