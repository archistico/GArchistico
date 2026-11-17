namespace GenerativeArt.Application.Catalog;

/// <summary>G2: pennelli procedurali basati sulle tecniche di Gȧrt Crinis e Flamebrush.</summary>
public static partial class BuiltInArtworkCatalog
{
    private static ArtworkDescriptor CrinisDescriptor() => new("crinis-bristles", 1,
        "Crinis · fasci di setole", "Pennelli / Fibre",
        "Fasci di filamenti attorno a curve portanti, con sovrapposizioni, ombra e inchiostro asciutto.",
        ArtworkCapabilities.Raster,
        [ArtworkParameterSpec.Integer("bands", "Fasci", 12, 2, 100, "Composizione"),
         ArtworkParameterSpec.Number("band-width", "Ampiezza fasci", .22, .04, .55, "Pennello"),
         ArtworkParameterSpec.Number("bristle-pitch", "Distanza setole (px)", 4.6, .55, 16, "Pennello"),
         // R9: il limite storico a 5 px rendeva non valide le ricette R7/R8
         // gia' consegnate con spessori 5.15 e 5.4; il renderer micro applica
         // comunque la saturazione continua in base alla distanza reale.
         ArtworkParameterSpec.Number("hair-width", "Spessore setola (px)", 1.0, .12, 8, "Pennello"),
         ArtworkParameterSpec.Integer("samples", "Campioni curva", 300, 70, 1200, "Curve"),
         ArtworkParameterSpec.Number("turn", "Curvatura", 1.0, 0, 3, "Curve"),
         ArtworkParameterSpec.Number("swirl", "Attrazione orbitale", .6, 0, 1.5, "Curve"),
         ArtworkParameterSpec.Number("shadow-strength", "Ombre fra i fasci", .22, 0, .65, "Ombre"),
         ArtworkParameterSpec.Number("body-opacity", "Opacità base fascio", .16, 0, 1, "Ombre", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-relief", "Rilievo fibre", .42, 0, 1, "Tratto", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-fray", "Sfilacciatura tessile", .24, 0, 1, "Tratto", optionalInJson: true),
         // Contratto R3/R4: il descrittore deve essere ricompilato insieme al renderer micro.
         ArtworkParameterSpec.Choice("detail-style", "Resa delle fibre", "classic", ["classic", "micro"], "Tratto", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-density", "Densità fibre", 1, .5, 3, "Tratto", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-coverage", "Copertura del singolo filo", .8, .15, 1.5, "Microfibre", optionalInJson: true),
         ArtworkParameterSpec.Number("pigment-contrast", "Contrasto del pigmento", .6, 0, 1, "Microfibre", optionalInJson: true),
         ArtworkParameterSpec.Number("depth-lighting", "Luce e profondità", .68, 0, 1, "Microfibre", optionalInJson: true),
         ArtworkParameterSpec.Number("rear-softness", "Sfocatura piani arretrati", .32, 0, 1, "Ombre", optionalInJson: true),
         // G3-R9: parametri neutri nei documenti precedenti. Modellano il fascio
         // nel suo insieme, senza trasformare le fibre in una sagoma piena.
         ArtworkParameterSpec.Number("bundle-compression", "Compressione del fascio", 0, -.35, .7, "Volume fascio", optionalInJson: true),
         ArtworkParameterSpec.Number("bundle-volume", "Rigonfiamento del fascio", 0, 0, 1, "Volume fascio", optionalInJson: true),
         ArtworkParameterSpec.Number("bundle-lighting", "Luce sul volume del fascio", 0, 0, 1, "Volume fascio", optionalInJson: true),
         // R10: la microstratificazione e' neutra sui JSON v1 precedenti.
         ArtworkParameterSpec.Number("intra-bundle-depth", "Profondità dentro il fascio", 0, 0, 1, "Profondità interna", optionalInJson: true),
         ArtworkParameterSpec.Number("featured-fibers", "Corpo filamenti principali", 0, 0, 1, "Profondità interna", optionalInJson: true),
         ArtworkParameterSpec.Number("cross-section-light", "Luce nella sezione", 0, 0, 1, "Profondità interna", optionalInJson: true),
         ArtworkParameterSpec.Number("internal-occlusion", "Occlusione tra filamenti", 0, 0, 1, "Profondità interna", optionalInJson: true),
         ArtworkParameterSpec.Number("micro-detail", "Microtrama", .55, 0, 1, "Tratto", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-taper", "Rastremazione filamenti", .75, 0, 1, "Tratto", optionalInJson: true),
         ArtworkParameterSpec.Number("fiber-shadow-depth", "Ombra delle singole fibre", .48, 0, 1, "Ombre", optionalInJson: true),
         ArtworkParameterSpec.Integer("vortex-centers", "Centri di curvatura", 1, 1, 8, "Curve", optionalInJson: true),
         ArtworkParameterSpec.Integer("ink-flecks", "Granelli di inchiostro", 0, 0, 6000, "Microfibre", optionalInJson: true),
         ArtworkParameterSpec.Number("dryness", "Inchiostro asciutto", .18, 0, .85, "Tratto"),
         ArtworkParameterSpec.Choice("palette", "Palette", "indigo", ["indigo", "copper", "forest", "crimson"], "Colori"),
         ArtworkParameterSpec.ColorOverride("background-color", "Sfondo"),
         ArtworkParameterSpec.ColorOverride("fiber-color-1", "Fibra scura"),
         ArtworkParameterSpec.ColorOverride("fiber-color-2", "Fibra media"),
         ArtworkParameterSpec.ColorOverride("fiber-color-3", "Fibra luminosa"),
         ArtworkParameterSpec.ColorOverride("shadow-color", "Ombra"),
         .. RasterFinishing()]);

    private static ArtworkDescriptor FlamebrushDescriptor() => new("spiral-flamebrush", 1,
        "Flamebrush · spirali puntinate", "Pennelli / Particelle",
        "Pennello a spirali di punti depositate lungo orbite, con fase, scala e dispersione controllabili.",
        ArtworkCapabilities.Raster,
        [ArtworkParameterSpec.Integer("rings", "Orbite", 8, 1, 25, "Composizione"),
         ArtworkParameterSpec.Integer("stamps", "Passaggi per orbita", 110, 24, 300, "Pennello"),
         ArtworkParameterSpec.Integer("particles", "Particelle per spirale", 20, 5, 60, "Pennello"),
         ArtworkParameterSpec.Number("orbit-radius", "Raggio relativo orbite", .37, .08, .48, "Geometria"),
         ArtworkParameterSpec.Number("orbit-spacing", "Separazione orbite", .028, .005, .08, "Geometria"),
         ArtworkParameterSpec.Number("brush-radius", "Raggio spirale", 18, 2, 80, "Pennello"),
         ArtworkParameterSpec.Number("spiral-turns", "Giri della spirale", 3, .5, 8, "Pennello"),
         ArtworkParameterSpec.Number("phase-speed", "Rotazione del pennello", 1, -.5, 3.5, "Movimento"),
         ArtworkParameterSpec.Number("scatter", "Dispersione", .2, 0, 1, "Movimento"),
         ArtworkParameterSpec.Number("dot-size", "Dimensione dei punti", 1.25, .3, 5, "Tratto"),
         ArtworkParameterSpec.Number("opacity", "Opacità dei punti", .68, .1, 1, "Tratto"),
         ArtworkParameterSpec.Choice("palette", "Palette", "cool", ["cool", "ember", "orchid"], "Colori"),
         ArtworkParameterSpec.ColorOverride("background-color", "Sfondo"),
         ArtworkParameterSpec.ColorOverride("particle-color-1", "Pigmento 1"),
         ArtworkParameterSpec.ColorOverride("particle-color-2", "Pigmento 2"),
         ArtworkParameterSpec.ColorOverride("particle-color-3", "Pigmento 3"),
         .. RasterFinishing()]);
}
