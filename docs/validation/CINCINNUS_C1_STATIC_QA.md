# Verifica statica — Cincinnus C1 (17/11/2026)

**Oggetto:** consegna kernel geometrico C# C1 su G4-R2-H3. **Metodo:** controllo sorgente/archivio e confronto byte-per-byte delle parti precedenti. **Limite:** non sostituisce `dotnet build` né `dotnet test` e non attesta qualità estetica.

## Sorgenti nuovi

- `src/GenerativeArt.Algorithms/Cincinnus/CincinnusFlowField.cs`
- `src/GenerativeArt.Algorithms/Cincinnus/CincinnusTraceOptions.cs`
- `src/GenerativeArt.Algorithms/Cincinnus/CincinnusPathGeometry.cs`
- `tests/GenerativeArt.Algorithms.Tests/CincinnusGeometryTests.cs`

Sorgenti e configurazioni preesistenti rimangono invariati. Nessun nuovo pacchetto NuGet, nessun altro linguaggio di implementazione, nessun asset inventato.

## Proprietà previste dai test xUnit aggiunti

1. Cinque vortici fuori canvas con range esterni corretti per quadrato, Full HD e UHD.
2. Riproducibilità dello stesso seed, differenza dei seed e campioni finiti.
3. Consistenza della legge del singolo vortice rispetto a `VectorFields.Vortex`.
4. Somma di vortici opposti senza drift artificiale.
5. Snapshot immutabile dei vortici passati dal chiamante.
6. Tracce avanti/indietro, conciliazione e clipping nel bordo esteso.
7. Integrazione RK4 stabile su orbite circolari.
8. Riproducibilità delle traiettorie prodotte dal campo generato.
9. Ritorno di un percorso chiuso dopo sufficiente allontanamento.
10. Raggio di curvatura sulle linee rette e su archi regolari.
11. Rifiuto di NaN, budget invalidi e opzioni fuori limite.
12. Cancellazione in creazione e tracciamento.

**Stato effettivo:** questi sono casi di test scritti nel progetto; non sono risultati già superati. Per il gate Windows usare la procedura della [scheda C1](../CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md).

## Esito effettivo del confronto archivio

- Confronto byte-per-byte con G4-R2-H3: **288 file preesistenti sotto `src/` e `tests/`**, **0 modificati**, **0 mancanti**.
- Nuovi file C#: **3** in Algorithms e **1** nella suite xUnit.
- Ricette JSON preesistenti: **173**; preset embedded preesistenti: **29**. **Tutti i 202 file JSON superano il parsing con `jq -e`**.
- Disponibilità SDK .NET nell'ambiente di preparazione: **assente**. Non abbiamo eseguito alcuna compilazione o test xUnit C1.
- Manifest SHA-256: ricalcolato a fine consegna, escluso il manifest stesso per evitare checksum circolare. Verifica completa richiesta prima della pubblicazione ZIP.

Il controllo byte-per-byte protegge da modifiche accidentali ai precedenti file, ma non dimostra l'assenza di problemi di compilazione dei nuovi file o la compatibilità dei test xUnit aggiunti: serve il gate reale Windows.
