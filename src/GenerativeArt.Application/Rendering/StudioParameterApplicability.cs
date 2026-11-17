using System.Globalization;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Indica quando un controllo del laboratorio non influenza il raster con le
/// scelte correnti. E' solo una regola di presentazione: NON modifica le ricette
/// né i valori serializzati e non rimuove parametri dalla validazione JSON.
/// Le condizioni rispecchiano i rami effettivi degli algoritmi di rendering.
/// </summary>
public static class StudioParameterApplicability
{
    public static string? InactiveReason(string artworkId, string parameterId,
        IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artworkId);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterId);
        ArgumentNullException.ThrowIfNull(parameters);

        string? Get(string name) => parameters.TryGetValue(name, out string? value) ? value : null;
        bool IsPositive(string name) => double.TryParse(Get(name), NumberStyles.Float,
            CultureInfo.InvariantCulture, out double value) && value > 0;

        if (artworkId == "voronoi-mosaic")
        {
            bool layered = Get("depth-style") == "layered";
            if (!layered && parameterId != "depth-style" &&
                GenerativeArt.Application.Catalog.VoronoiDepthControls.All.Any(p => p.Id == parameterId))
                return "Scegli layered per aprire le tessere e usare i piani di profondità.";
            if (layered && parameterId == "hole-pattern" && !IsPositive("missing-tiles"))
                return "Aumenta le tessere mancanti per scegliere la distribuzione delle aperture.";
            if (layered && parameterId == "bevel-width" && !IsPositive("tile-relief"))
                return "Attiva il rilievo delle tessere per regolare lo smusso.";
            if (layered && parameterId == "accent-color" && !IsPositive("accent-strength"))
                return "Attiva gli inserti luminosi per modificarne il colore.";
            if (layered && parameterId == "back-palette" && Enumerable.Range(1, 5).All(i =>
                Get($"tile-color-{i}") is string value && value != "auto"))
                return "I cinque colori personalizzati si applicano a tutti i piani e sostituiscono la palette.";
        }

        if (artworkId == "cincinnus-bristles")
        {
            if (Get("mode") == "simple" && parameterId is
                ("main-strokes" or "placement-attempts" or "patch-attempts" or "coverage-cell" or "adaptive-width"))
                return "Il parametro si usa solo in modalità Adaptive.";
            if (Get("mode") == "adaptive" && parameterId is ("separation" or "half-width-factor"))
                return "Il parametro si usa solo in modalità Simple.";
        }

        if (artworkId == "ocellus-eyespots")
        {
            if (parameterId == "pore-radius" && !IsPositive("pore-strength"))
                return "Attiva l'apertura degli interstizi per regolarne la dimensione.";
            if (parameterId == "rim-width" && !IsPositive("rim-light"))
                return "Attiva la luce dei bordi per regolarne l'ampiezza.";
        }
        if (artworkId == "rete-foam" && parameterId == "junction-radius" &&
            !IsPositive("junction-strength") && !IsPositive("node-glow"))
            return "Attiva il rilievo o la luce dei nodi per regolarne l'ampiezza.";

        // I vortici geometrici sono indipendenti dalla frequenza del noise.
        if (parameterId == "frequency" && Get("field") == "vortex" &&
            (artworkId is "noise-flow" or "spaced-streamlines" or "flow-brush" or "lic-flow"))
            return "Il campo vortex è geometrico: la frequenza non modifica il disegno.";

        // La torsione viene letta da braid e dal campo legacy, ma non da curl/vortex.
        if (parameterId == "curvature" &&
            (artworkId == "flow-brush" && Get("field") != "braid" ||
             artworkId == "noise-flow" && (Get("field") is "curl" or "vortex")))
            return "La torsione si applica solo ai campi che usano un angolo parametrico.";

        if (artworkId == "phyllotaxis" && parameterId == "arms" && Get("shape") == "sunflower")
            return "I bracci modulano solo rosette e galassie, non la spirale base.";
        if (artworkId == "reaction-diffusion" && parameterId == "bands" && Get("style") != "contours")
            return "Le bande isocromatiche sono disponibili solo nello stile contours.";

        if (artworkId == "fascia-ribbons" &&
            (parameterId is "droplet-size" or "drop-density") && Get("fade") == "0")
            return "Attiva la dissolvenza per regolare la retinatura locale.";

        // Crinis: un fascio invisibile non può proiettare l'ombra della sua sagoma.
        // Le ombre delle singole fibre restano separate nella modalità micro.
        if (artworkId == "crinis-bristles")
        {
            if (parameterId == "shadow-strength" &&
                double.TryParse(Get("body-opacity"), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double bodyOpacity) && bodyOpacity == 0)
                return "La base è invisibile: non viene disegnata l'ombra dell'intero fascio.";

            if (parameterId == "internal-occlusion" && Get("detail-style") == "micro" &&
                double.TryParse(Get("intra-bundle-depth"), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double innerDepth) && innerDepth == 0)
                return "Aumenta la profondità dentro il fascio per attivare l'occlusione interna.";

            if (Get("detail-style") != "micro" && parameterId is
                ("fiber-density" or "fiber-coverage" or "pigment-contrast" or
                 "depth-lighting" or "rear-softness" or "micro-detail" or "fiber-taper" or
                 "fiber-shadow-depth" or "vortex-centers" or "ink-flecks" or
                 "bundle-compression" or "bundle-volume" or "bundle-lighting" or
                 "intra-bundle-depth" or "featured-fibers" or "cross-section-light" or "internal-occlusion"))
                return "Seleziona la resa micro per usare i controlli delle microfibre.";
        }

        // Parametri di finitura: valori mantenuti nella ricetta anche se l'effetto è spento.
        if (parameterId == "grain-size" &&
            double.TryParse(Get("grain-amount"), NumberStyles.Float, CultureInfo.InvariantCulture, out double grain) &&
            grain == 0)
            return "Aumenta l'intensità della grana per usare questo controllo.";
        if (parameterId == "dither-levels" && Get("dither-mode") == "none")
            return "Scegli un metodo di dithering per impostare i livelli.";
        if (parameterId == "paper-strength" && Get("paper-texture") == "none")
            return "Scegli una texture di carta per regolarne l'intensità.";
        if (artworkId == "tonal-engraving" &&
            (parameterId is "mask-radius" or "mask-feather") && Get("mask") == "none")
            return "Scegli una maschera per regolarne il raggio o la sfumatura.";

        return null;
    }
}
