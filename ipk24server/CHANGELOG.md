# Čo funguje:

1. TCP spojenie: Program vytvára TCP server, ktorý naslúcha na špecifikovanej adrese a porte. Dokáže akceptovať pripojenia od klientov a komunikovať s nimi cez TCP spojenie.

2. Prihlasovanie používateľov: Po pripojení klienta na server je mu umožnené autentifikovať sa pomocou správnej správy typu AUTH. Server kontroluje syntaktickú správnosť a unikátnosť používateľského mena.

3. Zmena kanálu: Klienti môžu meniť kanál, na ktorom komunikujú, pomocou správ typu JOIN. Server kontroluje existenciu kanálu a umožňuje klientom sa pripojiť alebo vytvoriť nový kanál.

4. Odosielanie správ: Klienti môžu posielať správy na aktuálnom kanáli pomocou správ typu MSG. Tieto správy sú broadcastované všetkým ostatným klientom na danom kanáli.

# Čo nefunguje:

1. UDP nie je implementované 