using System.Numerics;
using GenerativeArt.Algorithms.Brushes;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// G3-R10: microfibre con profondità stratificata all'interno del fascio. Il centro del fascio è solo una guida
/// geometrica: quando body-opacity è zero NON viene disegnato alcun riempimento
/// né ombra poligonale del fascio. Le ombre locali appartengono alle singole fibre.
/// Compressione, volume e illuminazione collettiva danno profondità ai fasci
/// mantenendo ogni singolo filamento distinguibile.
/// Ricostruzione originale C# dei principi di tessitura e copertura studiati in
/// Gȧrt Cincinnus, senza traduzione letterale delle routines Kotlin.
/// </summary>
internal static class CrinisMicroRenderer
{
    private static readonly string[] FiberRoles = ["fiber-color-1", "fiber-color-2", "fiber-color-3"];

    private readonly record struct FiberPoint(Vector2 At, Vector2 Normal, float Radius);

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken cancellationToken)
    {
        (uint background, uint[] rawFibers, uint rawShadow) = recipe.Parameters["palette"] switch
        {
            "copper" => (0xFF2C1521u, new uint[] { 0xFF733544u, 0xFFCE8166u, 0xFFF6D4A7u }, 0xFF190D1Cu),
            "forest" => (0xFF101E20u, new uint[] { 0xFF345A56u, 0xFF91BAA2u, 0xFFE0E8C2u }, 0xFF081518u),
            "crimson" => (0xFF1A0308u, new uint[] { 0xFF4A0918u, 0xFFD32C50u, 0xFFF9C9B6u }, 0xFF110207u),
            _ => (0xFF06132Cu, new uint[] { 0xFF25456Cu, 0xFF769CBEu, 0xFFE1EFF7u }, 0xFF020A1Bu)
        };
        surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.FromArgb(background)));
        Color32[] colors = PatternPalette.Resolve(recipe, FiberRoles, rawFibers);
        Color32 shadow = ArtworkInk.Resolve(recipe, "shadow-color", Color32.FromArgb(rawShadow));

        int bands = ArtworkPaint.Int(recipe, "bands");
        int samples = ArtworkPaint.Int(recipe, "samples");
        float shortSide = Math.Min(recipe.Size.Width, recipe.Size.Height);
        float half = shortSide * (float)ArtworkPaint.Number(recipe, "band-width") * .5f;
        float pitch = (float)ArtworkPaint.Number(recipe, "bristle-pitch");
        float fiberWidth = (float)ArtworkPaint.Number(recipe, "hair-width");
        double density = ArtworkPaint.Number(recipe, "fiber-density");
        double coverage = ArtworkPaint.Number(recipe, "fiber-coverage");
        double pigmentContrast = ArtworkPaint.Number(recipe, "pigment-contrast");
        double depthLighting = ArtworkPaint.Number(recipe, "depth-lighting");
        double rearSoftness = ArtworkPaint.Number(recipe, "rear-softness");
        double bundleCompression = ArtworkPaint.Number(recipe, "bundle-compression");
        double bundleVolume = ArtworkPaint.Number(recipe, "bundle-volume");
        double bundleLighting = ArtworkPaint.Number(recipe, "bundle-lighting");
        double intraDepth = ArtworkPaint.Number(recipe, "intra-bundle-depth");
        double featuredFibers = ArtworkPaint.Number(recipe, "featured-fibers");
        double sectionLight = ArtworkPaint.Number(recipe, "cross-section-light");
        double innerOcclusion = ArtworkPaint.Number(recipe, "internal-occlusion");
        double relief = ArtworkPaint.Number(recipe, "fiber-relief");
        double fray = ArtworkPaint.Number(recipe, "fiber-fray");
        double micro = ArtworkPaint.Number(recipe, "micro-detail");
        double taper = ArtworkPaint.Number(recipe, "fiber-taper");
        double fiberShadow = ArtworkPaint.Number(recipe, "fiber-shadow-depth");
        double bodyOpacity = ArtworkPaint.Number(recipe, "body-opacity");
        double bandShadow = ArtworkPaint.Number(recipe, "shadow-strength");
        double dryness = ArtworkPaint.Number(recipe, "dryness");
        double turn = ArtworkPaint.Number(recipe, "turn");
        double swirl = ArtworkPaint.Number(recipe, "swirl");
        int flecks = ArtworkPaint.Int(recipe, "ink-flecks");
        int eyesCount = ArtworkPaint.Int(recipe, "vortex-centers");

        var rnd = new SplitMix64Random(recipe.Seed);
        var eyes = new Vector2[eyesCount];
        for (int i = 0; i < eyes.Length; i++)
        {
            eyes[i] = new Vector2((float)(recipe.Size.Width * (-.15 + 1.3 * rnd.NextDouble())),
                (float)(recipe.Size.Height * (-.15 + 1.3 * rnd.NextDouble())));
        }

        // Distribuzione per copertura reale dei percorsi: non solo la casella iniziale.
        const int gridY = 12;
        int gridX = Math.Clamp((int)Math.Round(gridY * (double)recipe.Size.Width / recipe.Size.Height), 8, 24);
        var covered = new int[gridX * gridY];
        int spin = rnd.NextDouble() < .5 ? -1 : 1;

        for (int band = 0; band < bands; band++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int minimum = covered.Min();
            int[] candidates = Enumerable.Range(0, covered.Length).Where(i => covered[i] == minimum).ToArray();
            int cell = candidates[rnd.NextInt32Inclusive(0, candidates.Length - 1)];
            Vector2 start = new((float)((cell % gridX + .12 + .76 * rnd.NextDouble()) * recipe.Size.Width / gridX),
                (float)((cell / gridX + .12 + .76 * rnd.NextDouble()) * recipe.Size.Height / gridY));
            Vector2 eye = eyes[rnd.NextInt32Inclusive(0, eyes.Length - 1)];
            double phase = rnd.NextDouble() * Math.Tau;
            float bandHalf = half * (float)(.68 + .58 * rnd.NextDouble());
            float step = Math.Max(.72f, shortSide / 290f);
            BristleSpinePoint[] spine = BristleSpine2D.Build(start, eye, samples, step,
                bandHalf, turn, swirl, phase, spin);
            spin = -spin;

            // Precalcolo per fascio, non per filamento: stessa sezione per tutti,
            // nessuna ripetizione di funzioni trigonometriche per milioni di punti.
            var sectionScales = new double[spine.Length];
            for (int k = 0; k < spine.Length; k++)
                sectionScales[k] = BundleSectionScale(k, phase, bundleVolume);

            MarkCoverage(spine, covered, gridX, gridY, recipe.Size.Width, recipe.Size.Height);
            int hairs = Math.Clamp((int)Math.Ceiling(bandHalf * 2 / pitch * density), 3, 360);
            // La densità decide QUANTI fili: il loro diametro non può crescere in
            // proporzione incontrollata, altrimenti formano un nastro opaco.
            float physicalSpacing = bandHalf * 2f / hairs;
            float diameterLimit = physicalSpacing * (float)coverage;
            double depth = band / (double)Math.Max(1, bands - 1);
            double focus = SmoothStep01(depth);

            void PaintBand(SkiaRasterSurface target)
            {
                if (bodyOpacity > 0)
                    DrawOptionalBandBody(target, spine, colors, shadow, depth,
                        bodyOpacity, bandShadow, shortSide);

                if (intraDepth <= 0)
                {
                    // Percorso precedente invariato: stesso ordine e consumo RNG,
                    // quindi i file storici senza nuovi parametri mantengono i pixel.
                    for (int j = 0; j < hairs; j++)
                    {
                        if ((j & 15) == 0) cancellationToken.ThrowIfCancellationRequested();
                        DrawFiber(target, spine, sectionScales, j, hairs, colors, shadow, rnd, phase, depth, focus,
                            fiberWidth, diameterLimit, pigmentContrast, depthLighting, fray, relief,
                            micro, taper, fiberShadow, dryness,
                            bundleCompression, bundleVolume, bundleLighting,
                            0, featuredFibers, sectionLight, innerOcclusion, 1);
                    }
                }
                else
                {
                    // Ogni filamento ha un RNG indipendente dall'ordine di pittura;
                    // disegnando prima il fondo e per ultimo il fronte si genera
                    // autentica occlusione *tra fili dello stesso fascio*.
                    var fiberSeeds = new ulong[hairs];
                    for (int j = 0; j < hairs; j++) fiberSeeds[j] = rnd.NextUInt64();
                    for (int layer = 0; layer < 3; layer++)
                    {
                        for (int j = 0; j < hairs; j++)
                        {
                            if ((j & 15) == 0) cancellationToken.ThrowIfCancellationRequested();
                            if (FiberStratum(fiberSeeds[j]) != layer) continue;
                            var strandRandom = new SplitMix64Random(fiberSeeds[j]);
                            DrawFiber(target, spine, sectionScales, j, hairs, colors, shadow, strandRandom,
                                phase, depth, focus, fiberWidth, diameterLimit, pigmentContrast,
                                depthLighting, fray, relief, micro, taper, fiberShadow, dryness,
                                bundleCompression, bundleVolume, bundleLighting,
                                intraDepth, featuredFibers, sectionLight, innerOcclusion, layer);
                        }
                    }
                }

                // I puntini d'inchiostro appartengono a diversi strati, non a un unico overlay finale.
                int from = (int)((long)flecks * band / bands);
                int to = (int)((long)flecks * (band + 1) / bands);
                for (int i = from; i < to; i++)
                {
                    if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
                    float x = (float)(rnd.NextDouble() * recipe.Size.Width);
                    float y = (float)(rnd.NextDouble() * recipe.Size.Height);
                    float radius = (float)(.28 + 3.2 * Math.Pow(rnd.NextDouble(), 3));
                    double fleckAlpha = (.48 + .28 * (1 - focus)) * (.72 + .28 * rnd.NextDouble());
                    target.DrawCircle(x, y, radius, ArtworkPaint.Alpha(shadow, fleckAlpha), true);
                }
            }

            double blur = shortSide * rearSoftness * .015 * Math.Pow(1 - focus, 1.22);
            double opacity = .76 + .24 * focus;
            if (blur > .02 || opacity < .999)
                surface.DrawLayer(PaintBand, new LayerEffects(Blur: blur, Opacity: opacity), cancellationToken);
            else
                PaintBand(surface);
        }
    }


    private static double SmoothStep01(double value)
    {
        double x = Math.Clamp(value, 0, 1);
        return x * x * (3 - 2 * x);
    }

    private static void MarkCoverage(IReadOnlyList<BristleSpinePoint> spine, int[] cells,
        int columns, int rows, int width, int height)
    {
        // Non segniamo ripetutamente una stessa cella per il medesimo fascio.
        var touched = new bool[cells.Length];
        int stride = Math.Max(1, spine.Count / 110);
        for (int k = 0; k < spine.Count; k += stride)
        {
            BristleSpinePoint p = spine[k];
            for (int side = -1; side <= 1; side++)
            {
                Vector2 at = p.Position + p.Normal * (p.HalfWidth * .72f * side);
                if (at.X < 0 || at.Y < 0 || at.X >= width || at.Y >= height) continue;
                int x = Math.Clamp((int)(at.X * columns / width), 0, columns - 1);
                int y = Math.Clamp((int)(at.Y * rows / height), 0, rows - 1);
                touched[y * columns + x] = true;
            }
        }
        for (int i = 0; i < cells.Length; i++) if (touched[i]) cells[i]++;
    }

    private static void DrawOptionalBandBody(SkiaRasterSurface surface,
        IReadOnlyList<BristleSpinePoint> spine, Color32[] colors, Color32 shadow,
        double bandDepth, double opacity, double shadowStrength, float shortSide)
    {
        // Questo metodo non viene chiamato con opacity=0: non resta alcuna sagoma.
        var outline = new List<Vector2>(spine.Count * 2);
        foreach (BristleSpinePoint p in spine) outline.Add(p.Position + p.Normal * p.HalfWidth);
        for (int i = spine.Count - 1; i >= 0; i--)
            outline.Add(spine[i].Position - spine[i].Normal * spine[i].HalfWidth);

        if (shadowStrength > 0)
        {
            float offset = shortSide * .007f;
            Vector2 delta = new(offset * .5f, offset);
            surface.DrawPolygon(outline.Select(v => v + delta).ToArray(),
                ArtworkPaint.Alpha(shadow, opacity * shadowStrength * .42), Color32.Transparent, .1f);
        }
        Color32 body = ArtworkPaint.Mix(PatternPalette.Gradient(colors, .16 + .56 * bandDepth), shadow, .4);
        surface.DrawPolygon(outline, ArtworkPaint.Alpha(body, opacity), Color32.Transparent, .1f);
    }

    private static void DrawFiber(SkiaRasterSurface surface, IReadOnlyList<BristleSpinePoint> spine,
        IReadOnlyList<double> sectionScales,
        int index, int hairs, Color32[] palette, Color32 shadow, SplitMix64Random random,
        double bandPhase, double depth, double focus, float baseWidth, float diameterLimit,
        double pigmentContrast, double depthLighting, double fray, double relief, double micro,
        double taper, double fiberShadow, double dryness,
        double bundleCompression, double bundleVolume, double bundleLighting,
        double intraDepth, double featuredFibers, double sectionLight, double innerOcclusion,
        int fiberLayer)
    {
        double u = -1d + 2d * (index + .2 + .6 * random.NextDouble()) / hairs;
        double lateral = Math.Clamp((u + 1d) * .5, 0d, 1d);
        double compressedU = ShapeBundleLateral(u, bundleCompression);
        double crown = Math.Sqrt(Math.Max(0, 1 - compressedU * compressedU));
        // Un'unica normale di luce attraversa l'intero fascio: i fili non
        // ricevono più tutti lo stesso chiaroscuro indipendentemente da dove
        // si trovano nella sezione. Il valore è coerente per tutta la pennellata.
        double faceLight = Math.Clamp(.35 + .48 * crown - .42 * compressedU, 0, 1);
        // La luce attraversa la sezione e forma creste e avvallamenti: il
        // risultato cambia lungo l'ampiezza del fascio, non solo tra i fasci.
        double sectionCrown = Math.Clamp(.52 + .37 * crown - .27 * compressedU
            + .16 * Math.Sin(compressedU * 8.3 + bandPhase * .7), 0, 1);
        double phase = random.NextDouble() * Math.Tau;
        int trim = Math.Min(spine.Count / 3,
            (int)(spine.Count * (.007 + .085 * u * u + .02 * random.NextDouble())));
        int first = trim;
        int last = spine.Count - trim;
        if (last - first < 8) return;

        double baseTone = Math.Clamp(.18 + .63 * lateral + .12 * (random.NextDouble() - .5)
            + .09 * depth, 0, 1);
        Color32 ink = PatternPalette.Gradient(palette, baseTone);
        double rearShade = depthLighting * (.14 + .38 * (1 - focus));
        double frontLift = depthLighting * relief * (.08 + .24 * focus);
        Color32 dark = ArtworkPaint.Mix(ink, shadow, relief * (.12 + .26 * lateral) + rearShade * (.55 + .20 * lateral));
        Color32 lit = ArtworkPaint.Mix(dark, Color32.Opaque(250, 240, 224), frontLift * (1.08 - .30 * lateral));
        double toneVariance = (random.NextDouble() * 2 - 1) * pigmentContrast;
        Color32 sculpted = toneVariance < 0
            ? ArtworkPaint.Mix(lit, shadow, -toneVariance * (.34 + .18 * (1 - focus)))
            : ArtworkPaint.Mix(lit, Color32.Opaque(252, 238, 217), toneVariance * (.18 + .14 * focus));
        if (bundleLighting > 0)
        {
            // Il fascio arretrato assorbe luce, quello frontale espone una
            // cresta luminosa. Non si applica alcun overlay opaco alla sagoma.
            double darkening = bundleLighting * (.10 + .23 * (1 - focus)) * (1 - faceLight);
            double brightening = bundleLighting * (.14 + .34 * focus) * faceLight;
            sculpted = ArtworkPaint.Mix(sculpted, shadow, darkening);
            sculpted = ArtworkPaint.Mix(sculpted, Color32.Opaque(255, 250, 237), brightening);
        }
        if (sectionLight > 0)
        {
            double valley = 1 - sectionCrown;
            sculpted = ArtworkPaint.Mix(sculpted, shadow,
                sectionLight * (.13 + .38 * valley) * valley);
            sculpted = ArtworkPaint.Mix(sculpted, Color32.Opaque(255, 251, 238),
                sectionLight * (.11 + .36 * sectionCrown) * sectionCrown);
        }
        if (intraDepth > 0)
        {
            // Livelli posteriori con meno luce e contrasto; filamenti frontali
            // più definiti e luminosi, senza applicare una maschera piena.
            double rear = fiberLayer == 0 ? 1 : fiberLayer == 1 ? .42 : 0;
            double front = fiberLayer == 2 ? 1 : fiberLayer == 1 ? .20 : 0;
            sculpted = ArtworkPaint.Mix(sculpted, shadow, intraDepth * (.24 + .38 * rear) * rear);
            sculpted = ArtworkPaint.Mix(sculpted, Color32.Opaque(255, 249, 230),
                intraDepth * (.08 + .27 * front) * front * (.45 + .55 * sectionCrown));
        }
        double fiberOpacity = Math.Clamp(.58 + .26 * focus + .16 * random.NextDouble(), .36, .98);
        if (intraDepth > 0 && fiberLayer == 0)
            fiberOpacity *= 1 - .30 * intraDepth;
        ink = ArtworkPaint.Alpha(sculpted, fiberOpacity);
        float requestedWidth = Math.Max(.08f, baseWidth * (float)(.78 + .82 * random.NextDouble()));
        // Saturazione continua, senza soglia netta: anche un pennello di
        // 3 px resta una trama di filamenti se il passo reale è < 1 px.
        // Al tempo stesso hair-width continua ad avere effetto sul risultato.
        // La compressione cambia anche il passo reale fra fibre adiacenti.
        // Adattare il diametro evita che l'addensamento si trasformi in un
        // unico nastro pieno, soprattutto quando il fascio si restringe.
        double spacingRatio = 1 - bundleCompression * (1 - 3 * u * u);
        float localLimit = diameterLimit * (float)(spacingRatio * (1 - .27 * bundleVolume));
        float width = EffectiveFiberWidth(requestedWidth, localLimit);
        width = FeaturedFiberWidth(width, requestedWidth, localLimit, featuredFibers, index);
        double strandVolume = 1 + relief * (.34 + .28 * focus) + depthLighting * (.16 + .20 * focus);
        double bundleSeed = .5 + .5 * Math.Sin(index * .43 + bandPhase * .67 + phase * .31);
        var run = new List<FiberPoint>(last - first);

        for (int k = first; k < last; k++)
        {
            BristleSpinePoint p = spine[k];
            double progress = (k - first) / (double)Math.Max(1, last - first - 1);
            double slow = Math.Sin(k * .037 + phase) + .45 * Math.Sin(k * .091 + bandPhase + index * .11);
            double fine = Math.Sin(k * .71 + phase * 1.7) + .38 * Math.Sin(k * 1.94 + index * .61);
            // Variazione continua dell'ampiezza lungo la curva, identica per
            // tutte le fibre della sezione: non introduce attraversamenti
            // arbitrari né vuoti casuali fra filamenti adiacenti.
            double section = sectionScales[k];
            double displaced = compressedU * section + .027 * slow * micro;
            float edge = (float)(fray * (Math.Min(p.HalfWidth * .025f, width * 4f)) *
                (.65 * Math.Sin(k * .45 + phase) + .30 * fine));
            // Compressione naturale dei filamenti arretrati nei solchi della
            // sezione; la traslazione è limitata e solo in modalità R10.
            double layerRecess = intraDepth * (fiberLayer == 0 ? -.010 : fiberLayer == 2 ? .006 : 0)
                * p.HalfWidth * (1 - .4 * crown);
            Vector2 at = p.Position + p.Normal * ((float)displaced * p.HalfWidth + (float)layerRecess + edge);

            // La pressione cade progressivamente alle estremità; niente tagli piatti.
            double cap = Math.Min(1, Math.Min(progress, 1 - progress) / (.015 + .09 * taper));
            cap = cap * cap * (3 - 2 * cap);
            double pressure = (1 - taper) + taper * (.10 + .90 * cap);
            double grain = 1 + micro * (.20 * Math.Sin(k * .31 + phase)
                + .11 * Math.Sin(k * 1.39 + bandPhase * .7 + index));
            double bulge = 1
                + (.12 + .18 * bundleSeed) * Math.Sin(k * .058 + phase * .83 + index * .21)
                + .09 * Math.Sin(k * .14 + bandPhase * 1.7 + index * .11);
            bulge = Math.Max(.72, bulge);
            float radius = Math.Max(.08f, (float)(width * .5 * pressure * Math.Max(.18, grain) * strandVolume * bulge));
            radius *= (float)(1 + bundleVolume * (.035 + .12 * crown));

            // Brevi lacune asciutte disaccoppiano i filamenti come su un tessuto reale.
            double gap = Math.Sin(k * .12 + phase * 1.2 + index * .33) *
                         Math.Sin(k * .047 + index * 2.37 + bandPhase);
            bool shouldBreak = dryness > 0 && gap > .98 - dryness * .12;
            if (shouldBreak)
            {
                PaintRun(surface, run, ink, shadow, fiberShadow, relief, fray, micro, pigmentContrast,
                    phase, focus, depthLighting, bundleLighting, faceLight, innerOcclusion, intraDepth,
                    fiberLayer);
                run.Clear();
            }
            else run.Add(new FiberPoint(at, p.Normal, radius));
        }
        PaintRun(surface, run, ink, shadow, fiberShadow, relief, fray, micro, pigmentContrast,
            phase, focus, depthLighting, bundleLighting, faceLight, innerOcclusion, intraDepth,
            fiberLayer);
    }

    /// <summary>
    /// Compressione trasversale monotona, senza inversioni o pieghe dei fili.
    /// Valori positivi addensano il centro, negativi lo espandono; bordi fissi.
    /// Derivata minima >= 0.3 nell'intervallo di catalogo [-.35, .7].
    /// </summary>
    internal static double ShapeBundleLateral(double u, double compression)
    {
        if (!double.IsFinite(u) || Math.Abs(u) > 1 ||
            !double.IsFinite(compression) || compression is < -.35 or > .7)
            throw new ArgumentOutOfRangeException(nameof(compression));
        return u * (1 - compression * (1 - u * u));
    }

    /// <summary>Modulazione dolce e limitata dell'ampiezza del fascio lungo la curva.</summary>
    internal static double BundleSectionScale(int sample, double phase, double volume)
    {
        if (sample < 0 || !double.IsFinite(phase) || !double.IsFinite(volume) || volume is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(volume));
        return 1 + volume * (.17 * Math.Sin(sample * .025 + phase)
            + .10 * Math.Sin(sample * .047 + phase * .6));
    }

    /// <summary>
    /// Saturazione continua: a pari densità lo spessore nominale può aumentare,
    /// ma non cancellare completamente lo spazio tra filamenti. Usato soltanto
    /// dal renderer Micro; il pennello classico mantiene le regole storiche.
    /// </summary>
    internal static float EffectiveFiberWidth(float requestedWidth, float diameterLimit)
    {
        if (!float.IsFinite(requestedWidth) || requestedWidth <= 0 ||
            !float.IsFinite(diameterLimit) || diameterLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedWidth));
        return requestedWidth * diameterLimit / (requestedWidth + diameterLimit);
    }

    internal static int FiberStratum(ulong seed) => (int)((seed >> 32) % 3);

    // Una minoranza di fili riceve un diametro ben leggibile, mentre gli altri
    // restano microtrama. Non ispessiamo uniformemente 300 filamenti adiacenti.
    internal static float FeaturedFiberWidth(float fine, float nominal, float limit,
        double featured, int index)
    {
        if (!float.IsFinite(fine) || fine <= 0 || !float.IsFinite(nominal) || nominal <= 0 ||
            !float.IsFinite(limit) || limit <= 0 || !double.IsFinite(featured) ||
            featured is < 0 or > 1 || index < 0)
            throw new ArgumentOutOfRangeException(nameof(featured));
        if (featured == 0) return fine;
        float prominence = index % 9 == 0 ? 4.6f : index % 3 == 0 ? 1.25f : 0f;
        return Math.Min(nominal, fine + (float)featured * limit * prominence);
    }

    private static void PaintRun(SkiaRasterSurface surface, IReadOnlyList<FiberPoint> points,
        Color32 ink, Color32 shadow, double shadowDepth, double relief,
        double fray, double micro, double pigmentContrast, double phase,
        double focus, double depthLighting, double bundleLighting, double faceLight,
        double innerOcclusion, double intraDepth, int fiberLayer)
    {
        if (points.Count < 2) return;

        float avgRadius = points.Sum(p => p.Radius) / points.Count;
        double fiberContact = shadowDepth + innerOcclusion * intraDepth *
            (fiberLayer == 2 ? .52 : fiberLayer == 1 ? .25 : .08);

        // Ombra di contatto multi-pass: non è la sagoma dell'intero fascio,
        // ma una stratificazione locale lungo il lato in ombra di ogni fibra.
        // Serve a far sedere meglio i filamenti in primo piano sopra quelli retrostanti.
        if (fiberContact > 0)
        {
            var contactOuter = new List<Vector2>(points.Count);
            var contactMid = new List<Vector2>(points.Count);
            var darkPath = new List<Vector2>(points.Count);
            foreach (FiberPoint p in points)
            {
                contactOuter.Add(p.At + p.Normal * Math.Max(.40f, p.Radius * 1.30f));
                contactMid.Add(p.At + p.Normal * Math.Max(.34f, p.Radius * .96f));
                darkPath.Add(p.At + p.Normal * Math.Max(.28f, p.Radius * .70f));
            }

            double outerAlpha = fiberContact * (.035 + .085 * focus + .050 * relief + .040 * depthLighting);
            double midAlpha = fiberContact * (.07 + .11 * focus + .06 * relief + .05 * depthLighting);
            double coreAlpha = fiberContact * (.16 + .18 * focus + .05 * (1 - focus) + .06 * relief + .06 * depthLighting);

            Color32 outerShade = ArtworkPaint.Alpha(ArtworkPaint.Mix(ink, shadow, .95), outerAlpha);
            Color32 midShade = ArtworkPaint.Alpha(ArtworkPaint.Mix(ink, shadow, .94), midAlpha);
            Color32 coreShade = ArtworkPaint.Alpha(ArtworkPaint.Mix(ink, shadow, .92), coreAlpha);

            surface.DrawPolyline(contactOuter, 1, 1, outerShade, Math.Max(.24f, avgRadius * 2.85f));
            surface.DrawPolyline(contactMid, 1, 1, midShade, Math.Max(.22f, avgRadius * 2.05f));
            surface.DrawPolyline(darkPath, 1, 1, coreShade, Math.Max(.20f, avgRadius * 1.50f));
        }

        // Ogni filo è un nastro strettissimo: il bordo laterale ha micro-interruzioni
        // geometriche riproducibili, non il contorno di un unico grande pennello.
        var hull = new List<Vector2>(points.Count * 2);
        for (int i = 0; i < points.Count; i++)
        {
            FiberPoint p = points[i];
            float irregular = (float)(fray * micro * .42 *
                (Math.Sin(i * .89 + phase) + .44 * Math.Sin(i * 2.29 + phase * .73)));
            hull.Add(p.At + p.Normal * (p.Radius * Math.Max(.13f, 1 + irregular)));
        }
        for (int i = points.Count - 1; i >= 0; i--)
        {
            FiberPoint p = points[i];
            float irregular = (float)(fray * micro * .42 *
                (Math.Sin(i * 1.17 + phase * 1.3) + .34 * Math.Sin(i * 2.71 + phase)));
            hull.Add(p.At - p.Normal * (p.Radius * Math.Max(.13f, 1 + irregular)));
        }
        surface.DrawPolygon(hull, ink, Color32.Transparent, .1f);

        if (intraDepth > 0 && fiberLayer == 0)
        {
            // Velo morbido lungo il solo filo arretrato (nessun blur del fascio).
            // Si disegna prima dei riflessi e senza riempire la silhouette.
            var rearPath = points.Select(p => p.At).ToArray();
            surface.DrawPolyline(rearPath, 1, 1,
                ArtworkPaint.Alpha(ink, intraDepth * .10), Math.Max(.25f, avgRadius * 2.3f));
        }

        if (relief > 0 || depthLighting > 0)
        {
            var centerBody = new List<Vector2>(points.Count);
            var darkRim = new List<Vector2>(points.Count);
            var lightRim = new List<Vector2>(points.Count);
            foreach (FiberPoint p in points)
            {
                centerBody.Add(p.At);
                darkRim.Add(p.At + p.Normal * Math.Max(.08f, p.Radius * .52f));
                lightRim.Add(p.At - p.Normal * Math.Max(.06f, p.Radius * .22f));
            }

            Color32 bodyTone = ArtworkPaint.Alpha(
                ArtworkPaint.Mix(ink, Color32.Opaque(245, 238, 223), .10 + .14 * relief + .12 * depthLighting * focus),
                .04 + .10 * relief + .10 * depthLighting * focus);
            surface.DrawPolyline(centerBody, 1, 1, bodyTone, Math.Max(.28f, avgRadius * 1.55f));

            Color32 rimShade = ArtworkPaint.Alpha(
                ArtworkPaint.Mix(ink, shadow, .95),
                (.06 + .16 * relief + .12 * depthLighting) * (.55 + .45 * (1 - focus))
                + bundleLighting * .12 * (1 - faceLight));
            surface.DrawPolyline(darkRim, 1, 1, rimShade, Math.Max(.18f, avgRadius * .95f));

            Color32 rimLight = ArtworkPaint.Alpha(
                ArtworkPaint.Mix(ink, Color32.Opaque(255, 249, 230),
                    .26 + .22 * relief + .18 * depthLighting * focus + .12 * bundleLighting * faceLight),
                .03 + (.08 + .10 * relief + .10 * depthLighting) * (.30 + .70 * focus)
                    + .13 * bundleLighting * faceLight * focus);
            surface.DrawPolyline(lightRim, 1, 1, rimLight, Math.Max(.16f, avgRadius * .68f));
        }

        if (pigmentContrast > .001)
        {
            // Non tutta la fibra assorbe la stessa quantità di pigmento:
            // brevi striature scure seguono il filo e variano con il seed.
            // La geometria rimane indipendente dalle finiture globali.
            var deposits = new List<Vector2>(points.Count / 2);
            Color32 depositInk = ArtworkPaint.Alpha(
                ArtworkPaint.Mix(ink, shadow, .86), .05 + pigmentContrast * (.28 + .16 * (1 - focus)));
            float markWidth = Math.Max(.13f, avgRadius * .56f);
            for (int i = 0; i < points.Count; i++)
            {
                FiberPoint p = points[i];
                double stain = .67 * Math.Sin(i * .084 + phase * 1.37)
                    + .33 * Math.Sin(i * .29 + phase * .72);
                if (stain < .15)
                {
                    if (deposits.Count > 1)
                        surface.DrawPolyline(deposits, 1, 1, depositInk, markWidth);
                    deposits.Clear();
                    continue;
                }
                deposits.Add(p.At + p.Normal * (p.Radius * .15f));
            }
            if (deposits.Count > 1)
                surface.DrawPolyline(deposits, 1, 1, depositInk, markWidth);
        }

        if (relief > 0)
        {
            // Luce intermittente lungo la fibra: impedisce l'effetto di un bordo
            // sinteticamente uniforme e separa le diverse profondità della trama.
            var glint = new List<Vector2>(points.Count);
            Color32 light = ArtworkPaint.Alpha(
                ArtworkPaint.Mix(ink, Color32.Opaque(255, 247, 225), .24 + .34 * relief + .22 * depthLighting * focus),
                .04 + (.14 + .18 * focus) * relief);
            float shineWidth = Math.Max(.17f, avgRadius * .54f);
            for (int i = 0; i < points.Count; i++)
            {
                FiberPoint p = points[i];
                double brightness = .64 * Math.Sin(i * .051 + phase)
                    + .36 * Math.Sin(i * .137 + phase * 1.31);
                if (brightness < -.34)
                {
                    if (glint.Count >= 2) surface.DrawPolyline(glint, 1, 1, light, shineWidth);
                    glint.Clear();
                    continue;
                }
                glint.Add(p.At - p.Normal * Math.Max(.08f, p.Radius * .47f));
            }
            if (glint.Count >= 2) surface.DrawPolyline(glint, 1, 1, light, shineWidth);
        }

        if (fray > .01 && micro > .01)
        {
            // Filamenti periferici sottilissimi, con orientamento naturale del tessuto.
            var flyaway = new List<Vector2>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                FiberPoint p = points[i];
                float offset = p.Radius * (float)(1.5 + fray * (.8 * Math.Sin(i * .61 + phase)
                    + .33 * Math.Sin(i * 2.13 + phase * .8)));
                flyaway.Add(p.At + p.Normal * offset);
            }
            Color32 fuzz = ArtworkPaint.Alpha(ink, (.02 + .08 * fray) * (.45 + .55 * focus));
            surface.DrawPolyline(flyaway, 1, 1, fuzz, .24f);
        }
    }
}
