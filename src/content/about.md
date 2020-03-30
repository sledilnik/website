## Podatki o širjenju bolezni COVID-19 v Sloveniji

Projekt zbira, analizira in objavlja podatke o širjenju koronavirusa SARS-CoV-2, ki povzroča bolezen COVID-19 v Sloveniji. Javnosti želimo omogočiti čim boljši pregled nad razsežnostjo težave in pravilno oceno tveganja.

### Zakaj zbiramo podatke

**Pravilno zbrani in ažurno ter transparentno objavljeni podatki so po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja.** Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov.

NIJZ žal trenutno ne objavlja popolnih in dovolj strukturiranih podatkov o številu okužb, zato jih zbiramo in dopolnjujemo iz različnih javnih virov ter posredujemo javnosti. Podatki v medijih in uradnih virih, iz katerih jih črpamo, so pogosto delni, nejasni in nedosledni, zaradi tega preglednica vključuje tudi opombe o virih in sklepanju na podlagi nepopolnih podatkov.

### Pomagajte – nam, sebi in drugim
Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa v njem prostovoljno in dejavno sodeluje od 20 do 45 ljudi, saj vnašanje in preverjanje podatkov in programiranje zahteva vedno več pozornosti. Gre za projekt, podprt z množičnim prostovoljnim sodelovanjem (t.i. "crowdsourcing"), [kjer **lahko prispeva vsak po svojih močeh** z viri ali podatki](#/team). Pridružite se in pomagajte!

### Zbrani podatki 

Vključeni so naslednji **dnevni podatki** (z zgodovino) iz [različnih javnih virov](#/sources):

-   število opravljenih testov in število potrjeno okuženih
-   število potrjeno okuženih po kategorijah: po starosti, spolu, regijah in občinah
-   evidenca o bolnišnični oskrbi pacientov s COVID-19: hospitalizirani, v intenzivni enoti, kritično stanje, odpuščeni iz bolniške oskrbe, ozdraveli
-   spremljanje posameznih primerov, še zlasti v kritičnih dejavnostih: zaposleni v zdravstvu, domovih starejših občanov, civilni zaščiti
-   zmogljivost zdravstvenega sistema: število postelj, enot intenzivne nege, respiratorjev za predihavanje ...

Sproti se trudimo dodajati tudi nove pomembne kategorije.

Vsi podatki so zbrani v [**GDocs preglednici**](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0)

Na voljo so v oblikah:
-   **CSV izvoz**: [stats.csv](https://github.com/slo-covid-19/data/blob/master/csv/stats.csv), [regions.csv](https://github.com/slo-covid-19/data/blob/master/csv/regions.csv), [patients.csv](https://github.com/slo-covid-19/data/blob/master/csv/patients.csv)
-   **REST API**: [stats](https://covid19.rthand.com/api/stats), [regions](https://covid19.rthand.com/api/regions), [patients](https://covid19.rthand.com/api/patients), [hospitals](https://covid19.rthand.com/api/hospitals), [hospitals-list](https://covid19.rthand.com/api/hospitals-list), [README](https://github.com/slo-covid-19/data-api/blob/master/README.md)

### Uporaba podatkov
Podatki se uporabljajo za različne vizualizacije in statistike:

-   [**Statistika**](#/stats) – grafi in infografike s podatki o potrjeno okuženih in stanju hospitaliziranih pacientov
-   [**Zemljevid**](#/map) – stanje potrjeno okuženih po občinah

Naše podatke uporabljajo tudi nekateri drugi portali - navedeni so na strani [Povezave](#/links).

### Kako urejamo podatke

[Bazo podatkov](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) urejamo s podatki Vlade RS (sumarni) in NIJZ (po kategorijah). Če so objavljeni vmesni podatki, začasno vnesemo tudi te. Podatki po regijah in starosti se včasih ne ujemajo s tistimi ob 23:59 (ker se zbirajo ob 10h); dopolnjujemo in navzkrižno preverjamo tudi s podatki, objavljenimi v medijih. S pomočjo OCR pretvorimo podatke po občinah v [tabelo Kraji](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Urejanje podatkov bolnišnične oskrbe – [tabela Pacienti](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

-   Spremljamo objave vseh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje) - okoli 14h.
-   Spremljamo število hospitaliziranih: vsi oddelki, hospitalizirani na intenzivni negi in v kritičnem stanju.
-   Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji (kadar je to mogoče zaznati).
-   Kjer so podatki nepopolni, s sklepanjem določimo vrednosti (uporabimo formulo).
-   Vsi viri in sklepanja so zabeleženi kot komentar v posameznih celicah (možnost preverjanja).
-   Podatke primerjamo s sumarnimi podatki o hospitaliziranih in intenzivni negi, ki jih Vlada RS objavlja ob 23:59.

### Pogoji uporabe

Uporaba in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse netržne vsebine ob navedbi vira – [**covid-19.sledilnik.org**](http://covid-19.sledilnik.org/).
Za izvoz podatkov v drugih oblikah ali uporabo za vizualizacije nas kontaktirajte na info@sledilnik.org.
