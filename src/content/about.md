# **Podatki o širjenju bolezni COVID-19 v Sloveniji**

Projekt zbira, analizira in objavlja podatke o širjenju koronavirusa SARS-CoV-2 oz. virusa, ki povzroča bolezen COVID-19 v Sloveniji, z namenom omogočiti javnosti čim boljši pregled nad razsežnostjo težave in pravilno oceno tveganja.

### Kateri podatki 
Vključeni so naslednji **dnevni podatki** (z zgodovino) iz različnih javnih virov:

-   število opravljenih testov
-   število potrjeno okuženih po kategorijah: v Sloveniji, po starosti, spolu, posameznih regijah in občinah
-   evidenca o bolnišnični oskrbi pacientov s COVID-19: hospitalizirani, intenzivna nega, kritično stanje, odpuščeni iz bolniške oskrbe
-   spremljanje posameznih primerov, še zlasti v kritičnih dejavnostih: zaposleni v zdravstvu, domovih starejših občanov, civilni zaščiti
-   zmogljivost zdravstvenega sistema: število postelj, enot intenzivne nege, ventilatorjev za predihavanje ...

Sproti se trudimo dodajati tudi nove pomembne kategorije.

### Zbrani podatki  
Vsi podatki so zbrani v [GDocs preglednici](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0)

Na voljo so v oblikah:
-   **CSV izvoz**: [stats.csv](https://github.com/slo-covid-19/data/blob/master/csv/stats.csv), [regions.csv](https://github.com/slo-covid-19/data/blob/master/csv/regions.csv), [patients.csv](https://github.com/slo-covid-19/data/blob/master/csv/patients.csv)
-   **REST API**: [stats](https://covid19.rthand.com/api/stats), [regions](https://covid19.rthand.com/api/regions), [patients](https://covid19.rthand.com/api/patients)

### Uporaba podatkov
Podatki se uporabljajo za različne vizualizacije in statistike:

-   [**Statistika**](https://covid-19.sledilnik.org/#/stats) – interaktivni graf s podatki o številu testiranih in njihovem stanju
-   [**Vizualizacije**](https://covid-19.sledilnik.org/#/viz) – infografike s pregledom okuženih, številu testov ... 
-   [**Zemljevid**](https://covid-19.sledilnik.org/#/map) – stanje okužb po občinah
-   [**Analiza širjenja koronavirusa**](https://covid19.alpaka.si) – postavil [Nace Štruc](http://www.nace.si/)

Naše podatke uporabljajo tudi [Ustavimo Korono](https://ustavimokorono.si/), [Better.Care COVID-19 Slo](https://app.powerbi.com/view?r=eyJrIjoiMWE2NGNmZWMtMjcxZC00MzkxLWIyMTUtYjExYjI2YTg4NzA0IiwidCI6IjkxMGYyNzY0LWEyZGItNGM2Mi04OGM0LWE1ZTcwYzMzNjVjNCIsImMiOjl9&nbsp), [Reported COVID-19 cases in the Slovenia - map](http://milosp.info/maps/interactive/covid19svn/covid19svn.html)

### Zakaj zbiramo podatke

**Pravilno zbrani in ažurno ter transparentno objavljeni podatki so po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja.** Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov.

NIJZ žal trenutno ne objavlja popolnih in dovolj strukturiranih podatkov o številu okužb, zato jih zbiramo in dopolnjujemo iz različnih javnih virov ter posredujemo javnosti. Podatki v medijih in uradnih virih, iz katerih jih črpamo, so pogosto delni, nejasni in nedosledni, zaradi tega preglednica vključuje tudi opombe o virih in sklepanju na podlagi nepopolnih podatkov.

## Kako urejamo podatke

[Bazo podatkov](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) urejamo praviloma po 14. uri s podatki NIJZ. Če so objavljeni vmesni podatki, začasno vnesemo tudi te. Podatki po regijah in starosti se včasih ne ujemajo s tistimi ob 14h (ker se zbirajo ob 10h); dopolnjujemo in navzkrižno preverjamo tudi s podatki, objavljenimi v medijih. S pomočjo OCR pretvorimo podatke po občinah v [tabelo Kraji](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Urejanje podatkov bolnišnične oskrbe – [tabela Pacienti](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

-   Spremljamo objave vseh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje).
-   Spremljamo število hospitaliziranih: vsi oddelki, hospitalizirani na intenzivni negi in v kritičnem stanju.
-   Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji (kadar je to mogoče zaznati).
-   Kjer so podatki nepopolni, s sklepanjem določimo vrednosti (uporabimo formulo).
-   Vsi viri in sklepanja so zabeleženi kot komentar v posameznih celicah (možnost preverjanja).

Kako urejamo posamezne primere – [tabela Primeri](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=1419250136):

-   Posamezne primere spremljamo na osnovi podatkov tiskovnih konferenc in objav medijev (povezava na vir).
-   Zabeležimo predvsem identifikacijske podatke in sledimo izvoru in žariščem (kjer je mogoče).
-   Rezerviramo mesta z minimalnimi podatki (izvor – na podlagi podatka po regijah), ki jih v naslednjih dneh dopolnjujemo.
-   Žarišča vodimo na osnovi podatkov o neposrednih okužbah in posplošenih podatkov NIJZ (takrat dodamo žarišče rezervirana mesta).
-   [K:žarišče](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=337671621), [K:regija](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=931207160) in [K:kraj](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=1657814423) so kontrolne pivot tabele za preverbo posameznih primerov.

### Pomagajte – nam, sebi in drugim
[Zato tudi računamo na vašo pomoč ]([https://covid-19.sledilnik.org/#/team](https://covid-19.sledilnik.org/#/team)) – tako pri zbiranju in preverjanju podatkov kot pri programiranju, analiziranju in tudi apeliranju na odločevalce, da ustrezno strukturirane podatke objavijo. 

### Pobudniki in sodelavci
Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa v njem prostovoljno in dejavno sodeluje od 15 do 25 ljudi, saj vnašanje in preverjanje podatkov in programiranje zahteva vedno več pozornosti. Gre za projekt, podprt z množičnim prostovoljnim sodelovanjem (t.i. "crowdsourcing"), [kjer **lahko prispeva vsak po svojih močeh** z viri ali podatki](https://covid-19.sledilnik.org/#/team). Pridružite se in pomagajte!

### Druge zanimive vsebine

V zavihku [**Viri**](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=328677411) si lahko ogledate naše vire podatkov. 

Na strani [**Povezave**](/#/links) si lahko ogledate zbirko povezav na druge statistike, podatkovne vizualizacije in zanimive objave o metodah in ukrepih, ki se nanašajo na COVID-19 pri nas in po svetu in so vplivale tudi na naše zbiranje podatkov. 

### Uporaba podatkov COVID-19 Sledilnika
Če bi podatke radi uporabili oz. če potrebujete drugače strukturirane podatke za podatkovno vizualizacijo, napovedno modeliranje in podobno, kontaktirajte [@LukaRenko](https://twitter.com/lukarenko).

## Pogoji uporabe

Uporaba in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse netržne vsebine ob navedbi vira – [**covid-19.sledilnik.org**](http://covid-19.sledilnik.org/).  
Za izvoz podatkov v drugih oblikah ali uporabo za vizualizacije nas kontaktirajte na [@LukaRenko](https://twitter.com/lukarenko)
