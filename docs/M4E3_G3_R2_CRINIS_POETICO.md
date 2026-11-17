# M4E.3-G3-R2 — Crinis poetico e tessile

Obiettivo: migliorare il generatore `crinis-bristles` sulla base del feedback visivo dell'utente.

## Interventi
- **Base opzionale**: nuovo parametro `body-opacity` (0..1) per rendere il corpo del fascio quasi impercettibile o del tutto invisibile.
- **Maggiore tridimensionalità**: nuovo parametro `fiber-relief` che applica ombre e luci locali sulle fibre.
- **Resa tessile**: nuovo parametro `fiber-fray` che introduce leggere irregolarità, contorni soffici e un aspetto più simile a fili di tessuto.

## Compatibilità
- Nessuna modifica agli altri 26 generatori.
- Preset personali invariati; aggiornato solo il preset incorporato di Crinis.
- Le ricette Crinis esistenti sono state estese con i nuovi parametri; il formato JSON resta v1.

## Nuove ricette
Cartella: `examples/recipes/m4e3-g3-r2/`
- `crinis-ethereal-weave-1920.json`
- `crinis-linen-drift-1920.json`
- `crinis-velvet-depth-1920.json`
- `crinis-moon-thread-1920.json`
