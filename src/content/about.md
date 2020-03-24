# **Podatki o širjenju bolezni COVID-19 v Sloveniji**

Projekt zbira, analizira in objavlja podatke o širjenju koronavirusa SARS-CoV-2 oz. virusa, ki povzroča bolezen COVID-19 v Sloveniji z namenom omogočiti javnosti čim boljši pregled nad obsegom težave in pravilno oceno tveganosti.

Vključeni so sledeči **dnevni podatki** (z zgodovino):

-   število opravljenih testov
-   število potrjeno okuženih: v Sloveniji, po starosti, spolu, posameznih regijah, in občinah
-   evidenca o bolnišnični oskrbi pacientov s COVID-19 (hospitalizirani, intenzivna nega, kritično stanje, odpuščeni iz bolniške oskrbe)
-   spremljanje posameznih primerov (posebno kritičnih oseb: zaposleni v zdravstvu, DSO, CZ...)
-   kapaciteta zdravstvenega sistema (število postelj, enot intenzivne nege, ventilatorjev...)

Sproti se trudimo dodajati tudi nove oz. relevantne kategorije.

Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa na njem prostovoljno in dejavno dela od 15 do 25 ljudi, saj vnašanje in preverjanje podatkov in programiranje zahteva vedno več pozornosti. Gre za projekt, podprt z množičnim zunanjim izvajanjem (t.i. "crowdsourcing"), [kjer **lahko prispeva vsak po svojih močeh** z viri ali podatki](https://slo-covid-19.rtfm.si/#/team). Pridružite se in pomagajte!

## Zbrani podatki  - [GDocs preglednica](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0)

-   **CSV izvoz**: [stats.csv](https://github.com/slo-covid-19/data/blob/master/csv/stats.csv), [regions.csv](https://github.com/slo-covid-19/data/blob/master/csv/regions.csv), [patients.csv](https://github.com/slo-covid-19/data/blob/master/csv/patients.csv)
-   **REST API**: [stats](https://covid19.rthand.com/api/stats), [regions](https://covid19.rthand.com/api/regions), [patients](https://covid19.rthand.com/api/patients)

### Podatki se že uporabljajo za različne vizualizacije in statistike:

-   [**Infografika**](https://slo-covid-19.rtfm.si/#/viz) - nekaj grafov z našimi podatki
-   [**Analiza širjenja koronavirusa**](https://covid19.alpaka.si) - postavil [Nace Štruc](http://www.nace.si/)
-   [**COVID-19 / Slovenia**](https://joahim.github.io/covid-19/) - vizualizacije od [TwoForSeven](https://twitter.com/TwoForSeven)
-   [**Nadzorna plošča Covid 19 SLO**](https://app.powerbi.com/view?r=eyJrIjoiMWE2NGNmZWMtMjcxZC00MzkxLWIyMTUtYjExYjI2YTg4NzA0IiwidCI6IjkxMGYyNzY0LWEyZGItNGM2Mi04OGM0LWE1ZTcwYzMzNjVjNCIsImMiOjl9&nbsp)  ([API](https://bettercare365-my.sharepoint.com/:x:/g/personal/emilp_better_care/EeZA7U_tdFpPjftMy3X2_koBrgpHfQKQvtQMRXPmQakFNw?rtime=eJWxRL3J10g)) - postavil [Better.Care](https://www.better.care/)
-   [**Reported COVID-19 cases in the Slovenia**](http://milosp.info/maps/interactive/covid19svn/covid19svn.html) - zemljevid po dnevih, animacija rasti, postavil [Miloš Popovič](https://twitter.com/milos_agathon)
-   [**Ustavimo Korono**](https://ustavimokorono.si/) - portal z navodili za javnost

Če bi podatke radi uporabili oz. potrebujete drugače strukturirane podatke za namene podatkovne vizualizacije, napovednega modeliranja ali podobno - kontaktirajte [@LukaRenko](https://twitter.com/lukarenko).

## Zakaj?

**Pravilno zbrani in ažurno ter transparentno objavljeni podatki so po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja.** Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov.

NIJZ žal trenutno ne objavlja popolnih in dovolj strukturiranih podatkov o številu okužb, zato jih zbiramo in dopolnjujemo iz različnih javnih virov ter posredujemo javnosti. Podatki v medijih in uradnih virih, iz katerih jih črpamo, so pogosto delni, nejasni in nedosledni, zaradi tega preglednica vključuje tudi opombe o virih in sklepanju na podlagi nepopolnih podatkov.  
[Zato tudi računamo na vašo pomoč ]([https://slo-covid-19.rtfm.si/#/team](https://slo-covid-19.rtfm.si/#/team))- tako pri zbiranju in preverjanju podatkov, kot programiranju, analiziranju, pa tudi apeliranju na odločevalce, da ustrezno struktururane podatke objavijo! 

##

## Kako urejamo podatke:

[Bazo podatkov](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) urejamo praviloma po 14:00 s podatki iz NIJZ. Če so objavljeni vmesni podatki, začasno vnesemo te. Podatki po regijah in starosti se včasih ne ujemajo s tistimi ob 14h (ker se zbirajo ob 10h); dopolnjujemo in navzkrižno preverjamo tudi s podatki, objavljenimi v medijih. S pomočjo OCR tudi pretvorimo podatke po občinah v [tabelo Kraji](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Kako urejamo bolnišnično oskrbo - [tabela Pacienti](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

-   Spremljamo objave vseh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje)
-   Spremljamo število hospitaliziranih (vsi oddelki), na intenzivni negi in v kritičnem stanju
-   Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji (kadar je to mogoče zaznati).
-   Kjer so podatki nepopolni, s sklepanjem določimo vrednosti (izvedemo s formulo)
-   Vsi viri in sklepanja so zavedena kot komentar na posameznih celicah (za možnost preverjanja)

Kako urejamo posamezne primere - [tabela Primeri](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=1419250136):

-   Posamezne primere spremljamo iz podatkov iz tiskovne konference in objav medijev (link na vir).
-   Zabeležimo predvsem identifikacijske podatke in sledimo izvoru in žariščem (kjer je možno).
-   Rezerviramo mesta z minimalnimi podatki (Izvor - na podlagi podatka po regijah), ki jih v naslednjih dneh dopolnjujemo.
-   Žarišča vodimo iz podatkov o neposrednih okužbah in posplošenih podatkov NIJZ (takrat dodamo žarišče rezervirana mesta).
-   [K:žarišče](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=337671621), [K:regija](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=931207160) in [K:kraj](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=1657814423) so kontrolne pivot tabele za validacijo posameznih primerov.

## Ostale zanimive vsebine

V zavihku [**Viri**](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=328677411) si lahko ogledate tudi naše vire podatkov. 

Na strani [**Povezave**](/#/links) si lahko ogledate zbirko povezav na ostale statistike, podatkovne vizualizacije in zanimive objave o metodah in ukrepih, ki se nanašajo na COVID-19 pri nas in po svetu in so vplivale tudi na naše zbiranje podatkov. 

## Pogoji uporabe

Uporaba in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse netržne vsebine ob navedbi vira - [**slo-covid-19.rtfm.si**](http://slo-covid-19.rtfm.si/).  
Za izvoz podatkov v drugih formatih ali uporabo za vizualizacije nas kontaktirajte na [@LukaRenko](https://twitter.com/lukarenko)
