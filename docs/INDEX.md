# Indice della documentazione

Questa pagina separa le **fonti operative** (da aggiornare a ogni incremento), i **contratti tecnici** e il materiale di **studio/storico**. Non usare piani o stato contenuti nei documenti tecnici più vecchi per sovrascrivere `ROADMAP.md` e `CURRENT_STATE.md`.

**Stato corrente:** M4D.3-R1: profondità con carta ritagliata, bokeh e paesaggi migliorati. **17 generatori, 73 ricette, build Release senza avvisi/errori e 801/801 test Windows** (Codex, SDK 10.0.201). Dieci nuove ricette Full HD, due fine-2x, controlli Studio e ripristino JSON verificati. Quindici PNG precedenti identici a seed 42 / 160×120. Guida [`M4D3_PROFONDITA.md`](M4D3_PROFONDITA.md), evidenze `docs/validation/M4D3_R1_*`. Livelli SrcOver, blur gaussiano e ombre riutilizzabili; editor di livelli/maschere/blend M4C.3 ancora aperto. Desktop/DPI reali, nuovi poster 4K e altri OS da verificare. Cache NuGet locale, telemetria build e audit online disattivati.

## Fonti operative — leggere in ordine

| File | Scopo |
| --- | --- |
| [`../README.md`](../README.md) | Presentazione, prerequisiti e comandi funzionanti |
| [`CURRENT_STATE.md`](CURRENT_STATE.md) | Versione confermata, limiti e prossima attività |
| [`HANDOFF.md`](HANDOFF.md) | Ripresa nella nuova chat, decisioni vincolanti e checklist |
| [`NUOVA_CHAT.md`](NUOVA_CHAT.md) | Messaggio pronto da incollare nella prossima conversazione insieme allo ZIP |
| [`ROADMAP.md`](ROADMAP.md) | **Unica roadmap** con ordine milestone e criteri di chiusura |
| [`TESTING.md`](TESTING.md) | Comandi e stato della validazione |
| [`QUALITY_GATES.md`](QUALITY_GATES.md) | Condizioni di accettazione di ogni fase |
| [`CHANGELOG.md`](CHANGELOG.md) | Cronologia delle consegne |
| [`M4D3_PROFONDITA.md`](M4D3_PROFONDITA.md) | Carta/bokeh, paesaggi migliorati, ombre/blur per piano, dieci ricette, budget e verifiche |
| [`M4D2_PITTURA_EVOLUZIONE.md`](M4D2_PITTURA_EVOLUZIONE.md) | Pennelli/paesaggi, primi generatori ampliati, 14 ricette, confronto prima/dopo e verifiche |
| [`M4D1_DETTAGLIO_CONTROLLO.md`](M4D1_DETTAGLIO_CONTROLLO.md) | Guilloché/incisione, controlli dettagliati, Domain Warp esteso, otto ricette e verifiche |
| [`M4B1_DOMAIN_WARP_LIC.md`](M4B1_DOMAIN_WARP_LIC.md) | Domain Warp/LIC, sette ricette, streamlines fino a 10000, picker, budget, test e smoke |
| [`M4A4_LABORATORIO_ARTISTICO.md`](M4A4_LABORATORIO_ARTISTICO.md) | Laboratorio artistico R1 validato, qualità di stampa e roadmap anti-alias |
| [`M4A4_R2_COLORI_HSV.md`](M4A4_R2_COLORI_HSV.md) | Colori completi R2, picker visuale, esempi, validazione e limiti |
| [`M4A4_R3_SUPERSAMPLING.md`](M4A4_R3_SUPERSAMPLING.md) | **R3**: campionamento 2×, pixel budget, PNG alpha-safe, test e smoke Windows |
| [`M4C1_GRANA_DITHERING.md`](M4C1_GRANA_DITHERING.md) | **M4C.1-R1**: grana seeded, dithering Bayer, alpha, compatibilità, budget e gate Windows |
| [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md) | **M4C.2-R1**: carta cotone/ruvida, granulazione, assorbimento, esempi e test |
| [`M4C2_R3_PREVIEW_E_BUDGET.md`](M4C2_R3_PREVIEW_E_BUDGET.md) | **M4C.2-R3**: correzione test JSON, poster 5 MP, anteprima regolabile e controlli fit/100% |

## Visione, progettazione e decisioni

- [`VISIONE_PRODOTTO.md`](VISIONE_PRODOTTO.md) — il prodotto finale, priorità e release.
- [`ESPERIENZA_STUDIO.md`](ESPERIENZA_STUDIO.md) — flussi d'uso e layout dell'app futura.
- [`M3D2_EXPORT_PARAMETRI.md`](M3D2_EXPORT_PARAMETRI.md) — export cancellabile, parametri esatti e test M3D.2-R1.
- [`M3D2_PREFERENZE_TEMI.md`](M3D2_PREFERENZE_TEMI.md) — preferiti, recenti, tema, persistenza e gate M3D.2-R2.
- [`M3D1_STUDIO.md`](M3D1_STUDIO.md) — Studio Avalonia, anteprima, playback e interazioni mouse.
- [`M3C3_RUNTIME.md`](M3C3_RUNTIME.md) — motore di rendering condiviso, Plasma e ricette.
- [`M4A1_ISOLINEE.md`](M4A1_ISOLINEE.md) — Marching Squares, generatori topografici, limiti e gate M4A.1.
- [`M4A2_SDF_POISSON.md`](M4A2_SDF_POISSON.md) — SDF/CSG e Poisson disk, due nuove opere, limiti, test e comandi PowerShell per il gate M4A.2-R1.
- [`M4A3_VORONOI_TASSELLAZIONI.md`](M4A3_VORONOI_TASSELLAZIONI.md) — Voronoi, Lloyd, mosaico, test ed export poster per M4A.3-R1.
- [`ARCHITETTURA_PRODOTTO.md`](ARCHITETTURA_PRODOTTO.md) — **stato effettivo** vs architettura proposta.
- [`ADR.md`](ADR.md) — registro delle decisioni tecniche adottate, storico incluso.
- [`REGISTRO_DECISIONI_P2.md`](REGISTRO_DECISIONI_P2.md) — decisioni di prodotto/proposte e problemi aperti.
- [`POLITICA_REPOSITORY.md`](POLITICA_REPOSITORY.md) — vincoli linguistici.
- [`RIFERIMENTI_ARTISTICI_PREFERITI.md`](RIFERIMENTI_ARTISTICI_PREFERITI.md) — opere Gȧrt/Go, Crinis, loop animati.
- [`CATALOGO_PRIORITA.md`](CATALOGO_PRIORITA.md) — matrice concettuale delle funzioni da includere.

## Specifiche tecniche, ancora utili ma con stato storico

- [`M1C_SPEC.md`](M1C_SPEC.md), [`M1D_SPEC.md`](M1D_SPEC.md): trasformazioni/colori e Simplex.
- [`M2A_SPEC.md`](M2A_SPEC.md), [`M2B_SPEC.md`](M2B_SPEC.md): buffer pixel e SkiaSharp.
- [`M3A_SPEC.md`](M3A_SPEC.md), [`M3B_SPEC.md`](M3B_SPEC.md): campi vettoriali e streamlines.
- [`M3C1_CATALOGO_ANIMAZIONI.md`](M3C1_CATALOGO_ANIMAZIONI.md), [`M3C2_RICETTE.md`](M3C2_RICETTE.md): catalogo, tempo e JSON v1.

**Nota:** le diciture originali «test da eseguire» delle specifiche storiche si riferiscono al momento della loro stesura, non allo stato della baseline corrente. Consultare `CURRENT_STATE.md`.

## Audit, riferimenti ed evidenze

- [`AUDIT_JDXYW_GENERATIVEART.md`](AUDIT_JDXYW_GENERATIVEART.md) e [`AUDIT_JDXYW_SORGENTI.md`](AUDIT_JDXYW_SORGENTI.md): ricerche sul repository Go, non documenti di rilascio.
- [`CATALOGO_JDXYW_GENERATORI.csv`](CATALOGO_JDXYW_GENERATORI.csv), [`MATRICE_FUSIONE_GART_JDXYW_CSHARP.csv`](MATRICE_FUSIONE_GART_JDXYW_CSHARP.csv), [`FEATURE_MATRIX_GART_GO_CSHARP.csv`](FEATURE_MATRIX_GART_GO_CSHARP.csv): inventari/comparazioni storiche, non registri di runtime C#.
- [`STORICO_CORREZIONI.md`](STORICO_CORREZIONI.md): fix di compilazione/test già risolti; non sono bug attualmente aperti.
- [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md), `../licenses/`: attribuzioni e licenze dei riferimenti.
- `../reference/gart/*.json`: dati golden riutilizzati dai test C#, non codice Kotlin.

Nell'handoff H1 non è stato eliminato nessun documento storico, sorgente, test o esempio; è stato aggiunto `NUOVA_CHAT.md` e riallineati i documenti operativi.

