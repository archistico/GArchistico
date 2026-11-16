# M4B.1-R1 — Domain Warp, LIC Flow e streamlines dense

16 novembre 2026. Base M4C.2-R4 (521/521). L'utente richiede nuovi algoritmi/ricette, pulsanti del picker sempre visibili e molte più streamlines nel Full HD. Questo incremento sviluppa M4B.1; livelli e maschere M4C.3 restano da implementare.

## Algoritmi e opere

`DomainWarpNoise2D` in Algorithms è un campo scalare riutilizzabile: fBm Simplex a 1–6 ottave, deformato da due noise seeded indipendenti con 1–3 livelli annidati. Ampiezza zero restituisce fBm non deformato. L'artwork `domain-warp` offre nubi, creste e venature sinusoidali, con frequenza, ampiezza e numero delle venature regolabili. Default scelto sul rendering Full HD: frequenza 0,001, tre ottave, ampiezza 1,3 e quattro venature. Deformazioni/frequenze molto alte producono dettagli fitti: ridurle per campiture ampie.

`LineIntegralConvolution` in Algorithms applica un kernel triangolare simmetrico a una texture scalare lungo streamlines RK2 normalizzate, con campionamento bilineare e rinormalizzazione dei pesi ai bordi. Campo nullo o zero passi preservano la texture. È una discretizzazione autonoma del principio LIC descritto da [Cabral e Leedom, Imaging Vector Fields Using Line Integral Convolution (1993)](https://cgl.ethz.ch/teaching/scivis_common/Literature/CabralLeedom93.pdf); non importa codice o immagini del paper e non implementa fast-LIC.

L'artwork `lic-flow` genera texture deterministica da seed/coordinate e la filtra lungo un campo curl Simplex o un vortice. `length` controlla i passi per direzione, `contrast` il contrasto e `texture-scale` la dimensione della texture. `frequency` agisce sul campo curl; nel vortice il campo è geometrico. Entrambe le opere coprono il canvas e supportano tre palette (ocean/ember/graphite), quattro override colore con alpha, standard/fine-2x, carta, pigmenti, grana e dithering.

Il catalogo ha **11 generatori**. JSON resta v1; i nove ID precedenti e i loro default rimangono disponibili.

## Streamlines distanziate

- `max-lines`: 1–10.000, contro il precedente massimo 500. Default storico 210 conservato.
- `separation`: 1–64 px, default 13; `line-width`: 0,1–12 px, default 1,15. Parametri facoltativi nei JSON storici.
- Con impostazioni estese, candidati fino a 200.000; quando il reticolo eccede il budget i semi vengono distribuiti sull'intero canvas anziché interrompersi nelle prime righe. I vecchi parametri/default mantengono il percorso storico.
- Cancellazione controllata durante l'integrazione, non solo dopo il tracing. Rimangono i tetti per passi, punti indicizzati e candidati. Il massimo delle linee non è un numero garantito di tracce: distanza e geometria limitano quelle accettate.

Per Full HD provare 8000 linee/distanza 5/spessore 1,35; per una trama più fine 10000/3,5/0,9. Le ricette pronte sono elencate sotto.

## Picker colore

Area editor scorrevole in una riga espandibile e footer fisso in una riga Auto. Annulla e Conferma colore restano visibili anche riducendo l'altezza. Dimensione iniziale 520×620 DIP, minimo 440×280; altezza massima adattata all'area utile del monitor tenendo conto della scala DPI. Campi RGBA su pannello che va a capo, Invio/Esc per conferma/annullamento. Il colore viene applicato solo confermando; il testo RGB/HEX richiede ancora Applica come in precedenza.

Smoke headless con gli stili reali a larghezza 440 e altezze 280/320/480/620: entrambi i pulsanti dentro il client, conferma conserva `#A04080C0`, annulla non applica nulla. Questo verifica layout ed eventi; una sessione desktop reale su monitor/DPI diversi resta da provare.

## Sette nuove ricette Full HD

| File in examples/recipes | Disegno | Secondi misurati |
| --- | --- | ---: |
| domain-warp-ocean-marble-1920.json | Venature oceaniche, carta e grana | 1,07 |
| domain-warp-ember-clouds-1920.json | Nubi calde con pigmenti | 1,31 |
| domain-warp-graphite-ridges-1920.json | Creste grafite su carta ruvida | 1,35 |
| lic-curl-ocean-1920.json | Fibre curl, 24 passi | 9,53 |
| lic-vortex-graphite-1920.json | Fibre circolari, 32 passi | 12,38 |
| spaced-streamlines-dense-1920.json | 8000 linee massime, distanza 5 | 5,54 |
| spaced-streamlines-silk-1920.json | 10000 linee massime, distanza 3,5 | 15,90 |

PNG generati e decodificati a 1920×1080 su Windows; anteprime ridotte in `examples/previews`. Tempi di un singolo processo locale, non benchmark universali. Il picco working set del processo cumulativo arriva a circa 175 MiB; include immagini/renderer usati in precedenza e non misura isolatamente ciascun algoritmo. Dati grezzi in [`validation/M4B1_R1_render-report.json`](validation/M4B1_R1_render-report.json).

Per le due ricette streamlines sono stati controllati 64 blocchi 240×135: il blocco meno coperto contiene rispettivamente 46,3% e 48,5% di pixel diversi dallo sfondo. Nessuna zona vuota per un reticolo di semi troncato. I margini intenzionali restano.

## Budget e verifica

Rimangono i limiti globali 4096×4096/16.777.216 pixel e fine-2x con massimo 16.777.216 sottopixel. Domain Warp aggiunge un tetto di 400 milioni di campioni noise stimati da pixel fisici × ottave × (1 + 2 × livelli). LIC limita a 400 milioni le posizioni del kernel stimate da pixel fisici × (2 × passi + 1), e a 16.777.216 i pixel fisici. Il budget viene verificato prima di allocare la superficie o scrivere output; Full HD fine-2x con default consentito, combinazioni estreme su poster rifiutate.

**Codex, Windows, SDK 10.0.201:** build Release con zero avvisi/errori, **558/558 test** (Core 115, Algorithms 150, Application 126, Rendering.Skia 167). Test su fBm senza warp, seed/continuità, kernel LIC, simmetria, bordi, parametri effettivi, alpha/materiale in fine-2x, JSON, cancellazione, budget e copertura del tracing oltre 500 linee. Log completo: [`validation/M4B1_R1_windows.log`](validation/M4B1_R1_windows.log).

Validazione normale:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Smoke opzionale riproducibile, separato dalla solution e dai test ordinari:

```powershell
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- picker .
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- render .
```

Scrive solo in `artifacts/m4b1-smoke`. Il comando render valida tutte le ricette d'esempio, genera i sette nuovi PNG, controlla dimensioni/copertura e produce JSON con tempi e anteprime. Il comando picker usa Avalonia.Headless 12.1.4 come dipendenza del solo tool; il prodotto conserva Avalonia 12.1.6.

Nell'ambiente ristretto Codex: restore da cache locale in `artifacts/nuget`, sorgente NuGet locale, audit online disabilitato e `AVALONIA_TELEMETRY_OPTOUT=1` applicati al processo. Nessun cambio allo script canonico o alle dipendenze di produzione. Non sono certificati audit NuGet online, Linux/macOS, DPI desktop reali o rendering poster 4K dei nuovi artwork.
