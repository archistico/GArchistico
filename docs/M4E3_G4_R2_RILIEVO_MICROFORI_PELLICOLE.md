# M4E.3-G4-R2 — Ocellus: interstizi e rilievo; Rete: nodi e pellicole

**Data:** 17 novembre 2026 · **Stack:** C#/.NET 10, Avalonia, SkiaSharp.

**Esito successivo alla consegna:** l’utente ha confermato che **G4-R2 compila e tutti i test passano su Windows**. Il numero dei test non è stato comunicato. Il confronto estetico A/B delle nuove ricette resta da documentare; le frasi sotto che descrivono il gate come aperto si riferiscono allo stato antecedente a questa conferma.

## Decisione e baseline

L'utente conferma che **G4-R1.2 compila e tutti i test passano su Windows**. Questa è la baseline validata da cui parte la R2. Crinis R10 resta conservato; la valutazione estetica di R10 non è stata dichiarata conclusa. Sviluppo G4-R2 portato avanti senza richiedere un'altra verifica preliminare.

## Ocellus — `ocellus-eyespots`

- `OcellusField.SignedDistance`: espone lo stesso smooth minimum usato finora dalla copertura. Il renderer usa **la distanza già calcolata** anche per il rilievo, evitando di campionare due volte tutti gli occhi.
- **Luce sul bordo (`rim-light`, `rim-width`):** sottile fascia luminosa all'interno del bordo del livello fuso. Segue dunque la *unione* SDF, non la circonferenza isolata.
- **Interstizi (`pore-strength`, `pore-radius`):** usa le tre superfici esterne più vicine. Un'apertura locale appare solo quando le distanze sono quasi pari e vicine al bordo esterno. È un modello continuo/euristico, **non** un flood-fill topologico di tutti i vuoti: il flood-fill fedele a Gȧrt resta un possibile perfezionamento R3.
- Nessun cambio del seme, dell'ordine di collocazione, dei raggi o del numero massimo di livelli (13). La copertura originale resta invariata se i controlli nuovi sono neutri.

| Controllo | Range | Default catalogo v1 | Preset iniziale Studio R2 |
|---|---:|---:|---:|
| `pore-strength` | 0–1 | 0 | 0.62 |
| `pore-radius` | 1–30 px | 9 | 11 |
| `rim-light` | 0–1 | 0 | 0.55 |
| `rim-width` | 0.5–12 px | 3 | 3.5 |

## Rete — `rete-foam`

- **Nodi a tre celle (`junction-strength`, `junction-radius`):** il campo `ReteFoamField.JunctionIntensity` rileva quando tre superfici sono quasi equidistanti. La parete s'ingrossa in modo continuo solo nella vicinanza dei nodi.
- **Luce ai nodi (`node-glow`):** composizione separata del colore chiaro sui punti di contatto, utilizzando un buffer opzionale `float[]` allocato **solo quando il controllo è maggiore di zero**.
- **Pellicola interna (`membrane-opacity`):** sottili variazioni di luce riempiono le zone vicine alla superficie interna delle celle, attenuate al centro e annullate sulle pareti opache. Il vecchio `film` continua a controllare i riflessi del bordo.
- Il numero di strati, la sfocatura dei livelli lontani, l'impacchettamento e le palette originali restano inalterati. Non è stata introdotta un'illuminazione 3D fisicamente corretta: il modello è pittorico/procedurale.

| Controllo | Range | Default catalogo v1 | Preset iniziale Studio R2 |
|---|---:|---:|---:|
| `junction-strength` | 0–1 | 0 | 0.65 |
| `junction-radius` | 1–18 px | 5 | 5 |
| `node-glow` | 0–1 | 0 | 0.48 |
| `membrane-opacity` | 0–0.85 | 0 | 0.36 |

## Compatibilità e budget

1. Tutti gli otto controlli sono `optionalInJson: true`, mantenendo formato e versione algoritmo **v1**. Un vecchio documento senza queste proprietà è valido e riceve i default neutri.
2. I preset Full HD di Studio per **questi due soli generatori** ora accendono le funzioni R2, senza aggiornare i documenti personali preesistenti né le altre 27 opere. Le otto ricette G4-R1 sono rimaste invariate.
3. Rete resta entro **4.194.304 pixel** e il budget `pixel × layers × bubbles`; Ocellus conserva il limite preventivo di lavoro di R1. Le opzioni R2 aggiungono aritmetica per pixel; non incrementano il numero di livelli/occhi/celle. Rete alloca il buffer nodi solo quando necessario.
4. La preview esatta continua a usare la ricetta completa. Il controllo **Adatta** non ridimensiona la geometria renderizzata. L'export deve coincidere pixel per pixel con la preview nativa a pari ricetta/risoluzione.

## Ricette per confronto A/B

In `examples/recipes/m4e3-g4-r2/` sono presenti **sei** ricette derivate dalle G4-R1 con seed e dimensioni conservati:

- `ocellus-corallo-rilievo-1920.json` / riferimento `m4e3-g4/ocellus-corallo-liquido-1920.json`;
- `ocellus-solare-interstizi-1920.json` / riferimento `m4e3-g4/ocellus-solare-1920.json`;
- `ocellus-grafite-bordi-1920.json` / riferimento `m4e3-g4/ocellus-incisione-grigia-1920.json`;
- `rete-notturna-pellicole-1920.json` / riferimento `m4e3-g4/rete-notturna-1920.json`;
- `rete-glaciale-nodi-1920.json` / riferimento `m4e3-g4/rete-glaciale-1920.json`;
- `rete-seppia-membrane-1920.json` / riferimento `m4e3-g4/rete-seppia-1920.json`.

**Totale:** 29 generatori, 29 preset embedded, **173 ricette** esempio, 17 preset storici intatti.

## Test e criteri di accettazione

- `GartG4GeometryTests`: distanza firmata/coprertura equivalenti, cavità solo in prossimità di tre bordi, intensità nodo vicino a tre superfici e rifiuto del raggio nullo.
- `GartG4RenderingTests`: rendering seeded deterministico, nuovi effetti che cambiano il raster, **vecchi JSON senza campi R2 pixel-identici ai controlli neutri**, finiture, cancellazione, budget e preview/export già testati dalla suite precedente.
- `GartG4CatalogTests`: sei fixture JSON R2 valide con roundtrip e preset Studio intenzionalmente diversi dai default neutri del catalogo.
- Gate richiesto: `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella root. **Gate Windows superato per riscontro successivo dell’utente** (17/11/2026). I confronti visivi restano da eseguire/registrare.
- Confronto visivo: aprire la ricetta R1 e la corrispondente R2 con **stesso seed**, usare *Adatta* per la composizione e *100%* per gli interstizi/nodi; controllare inoltre la resa della pellicola con `membrane-opacity=0` e `>0`.

## Successivamente

1. Valutare i sei confronti visivi; aumentare incisività delle cavità solo se non spariscono nell'anti-alias, evitando buchi artificiali.
2. Ocellus R3 eventuale flood-fill geometrico/topologico dei vuoti effettivi; Rete R3 curvature e tensione delle pellicole, giunzioni più fedeli e colore della membrana indipendente dalla luce dei bordi.
3. Tornare allo sviluppo di altre opere preferite Gȧrt (Nimbus, Pebble, Accretion, Glst); più avanti Fase B compositore, Fase D varianti/confronto/galleria, e animazione avanzata soltanto dopo.

## Nota UI: parametri condizionali

`StudioParameterApplicability` mostra `pore-radius` come inattivo se `pore-strength=0`, `rim-width` se `rim-light=0`, e `junction-radius` se **entrambi** `junction-strength` e `node-glow` sono zero. L'inattività è solo visiva: valori e JSON restano invariati, e il test `G4R2DependentControlsAreDisabledOnlyWhenTheirEffectsAreZero` la verifica.

`StudioExactPreviewRenderingTests.G4R2AdvancedControlsKeepExactPreviewAndExportEqual` confronta i byte PNG per entrambi i generatori con le opzioni R2 accese, in aggiunta ai casi preesistenti con i valori neutri.
