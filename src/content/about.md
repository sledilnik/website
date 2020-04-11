# Podatki o širjenju bolezni COVID-19 v Sloveniji

*Projekt zbira, analizira in objavlja podatke o širjenju koronavirusa SARS-CoV-2, ki povzroča bolezen COVID-19 v Sloveniji. Javnosti želimo omogočiti čim boljši pregled nad razsežnostjo težave in pravilno oceno tveganja.*

## Zakaj?

Pravilno zbrani in ažurno ter transparentno objavljeni podatki so po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja. Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov.

Podatke zbiramo iz različnih javno dostopnih virov, od sobote, 28. marca dalje pa imamo vzpostavljeno tudi direktno povezavo z zdravstvenimi zavodi in [NIJZ](https://www.nijz.si/). Ti nam pošiljajo strukturirane podatke, ki jih potem validiramo in oblikujemo v obliko, primerno za vizualizacije in  predstavitev javnosti, kakor tudi za nadaljne delo pri razvoju modelov in napovedi. Ker so podatki iz medijev in nekaterih drugih virov kdaj tudi nejasni in nedosledni, [preglednica]([https://covid-19.sledilnik.org/#/tables](https://covid-19.sledilnik.org/#/tables)  vključuje tudi opombe o virih in sklepanju na podlagi nepopolnih podatkov.

##### Pomagajte tudi vi – nam, sebi in drugim

Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa v njem prostovoljno in dejavno sodeluje od 20 do 45 ljudi, saj vnašanje in preverjanje podatkov ter programiranje zahteva vedno več pozornosti. Projekt nastaja z množičnim prostovoljnim sodelovanjem (t.i. "crowdsourcing"), [kjer lahko prispeva vsak po svojih močeh z viri ali podatki](#/team). Pridružite se in pomagajte!

## Zbrani podatki

Vključeni so naslednji dnevni podatki (z zgodovino) iz [NIJZ in različnih javnih virov](#/sources):

- število opravljenih testov in število potrjeno okuženih
- število potrjeno okuženih po kategorijah: po starosti, spolu, regijah in občinah
- evidenca o bolnišnični oskrbi pacientov s COVID-19: hospitalizirani, v intenzivni enoti, kritično stanje, odpuščeni iz bolniške oskrbe, ozdraveli
- spremljanje posameznih primerov, še zlasti v kritičnih dejavnostih: zaposleni v zdravstvu, domovih starejših občanov, civilni zaščiti
- zmogljivost zdravstvenega sistema: število postelj, enot intenzivne nege, respiratorjev za predihavanje ...

Sproti se trudimo dodajati tudi nove pomembne kategorije.

Vsi podatki so zbrani v [**GDocs preglednici**](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0).

Na voljo so v oblikah:

- **CSV izvoz**: 
  - [stats.csv](https://github.com/slo-covid-19/data/blob/master/csv/stats.csv), [regions.csv](https://github.com/slo-covid-19/data/blob/master/csv/regions.csv), [patients.csv](https://github.com/slo-covid-19/data/blob/master/csv/patients.csv), [hospitals.csv](https://github.com/slo-covid-19/data/blob/master/csv/hospitals.csv), [retirement_homes.csv](https://github.com/slo-covid-19/data/blob/master/csv/retirement_homes.csv), [safety_measures.csv](https://github.com/slo-covid-19/data/blob/master/csv/safety_measures.csv)
  - [dict-region.csv](https://github.com/slo-covid-19/data/blob/master/csv/dict-region.csv), [dict-muncipality.csv](https://github.com/slo-covid-19/data/blob/master/csv/dict-municipality.csv), [dict-hospitals.csv](https://github.com/slo-covid-19/data/blob/master/csv/dict-hospitals.csv), [retirement_homes.csv](https://github.com/slo-covid-19/data/blob/master/csv/retirement_homes.csv), 
- **REST API**: 
  - [stats](https://covid19.rthand.com/api/stats), [regions](https://covid19.rthand.com/api/regions), [patients](https://covid19.rthand.com/api/patients), [hospitals](https://covid19.rthand.com/api/hospitals), [retirement-homes](https://covid19.rthand.com/api/retirement-homes)
  - [hospitals-list](https://covid19.rthand.com/api/hospitals-list), [retirement-homes-list](https://covid19.rthand.com/api/retirement-homes-list) in drugi, več informacij v [README](https://github.com/slo-covid-19/data-api/blob/master/README.md)

## Kako urejamo podatke

[Bazo podatkov](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) urejamo s podatki NIJZ (po kategorijah). Podatki po regijah in starosti kdaj tudi kasneje dopolnjujemo in navzkrižno preverjamo, ko se spremenijo zaradi epidemioloških raziskav. Podatke o občinah sledimo v [tabeli Kraji](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Urejanje podatkov bolnišnične oskrbe – [tabela Pacienti](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

- Spremljamo objave vseh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje) - okoli 14h.
- Spremljamo število hospitaliziranih: vsi oddelki, hospitalizirani na intenzivni negi in v kritičnem stanju.
- Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji (kadar je to mogoče zaznati).
- Kjer so podatki o prehodih (sprejem/odpust) nepopolni, s sklepanjem določimo vrednosti (uporabimo formulo).
- Vsi viri in sklepanja so zabeleženi kot komentar v posameznih celicah (možnost preverjanja).
- Podatke primerjamo s sumarnimi podatki o hospitaliziranih in intenzivni negi, ki jih Vlada RS objavlja ob 23:59.

## 

## Uporaba grafov in vizualizacij

Naše grafe in prikaze lahko na svojo spletni strani uporabite tudi vi. Vgradite lahko  poljuben graf ali prikaz; opis, kako to storite, lahko najdete [na strani s primeri](https://github.com/slo-covid-19/website/tree/master/examples); če želite objavo vašega projekta na naši strani, nas prosimo kontaktirajte. 

## Uporaba podatkov

Zbrane podatki uporabljamo za različne vizualizacije in statistike:

- [**Statistika**](#/stats) – grafi, infografike in zemljevidi s podatki o potrjeno okuženih in stanju hospitaliziranih pacientov

Naše podatke uporabljajo tudi nekateri drugi portali - navedeni so na strani [Povezave](#/links); veseli bomo, če nas o uporabi obvestite, da lahko objavimo tudi vaš projekt. 

**Pozor**: Informacije, objavljene na naši spletni strani, vključno s povezavami na modele in druge strani, s katerimi nismo neposredno povezani, so pripravljene z največjo mogočo skrbnostjo ob uporabi razpoložljivih virov podatkov, znanja, metodologij in tehnologij, upoštevajoč znanstvene standarde. 
Verjamemo, da lahko prikazi in modeli pomagajo razložiti različne dejavnike širjenja virusa, med drugim tudi vpliv sprejetih in mogočih nadaljnjih ukrepov, s čimer želimo poudariti, da imamo v tej pandemiji vsi pomembno vlogo.
 Kljub temu ne moremo 100 % oz. v celoti zagotoviti točnosti, popolnosti ali uporabnosti informacij na teh spletnih straneh, in izrecno zavračamo kakršno koli odgovornost za nadaljnje interpretacije in simulacije, ki naše prikaze navajajo kot vir.

## Pogoji uporabe

Uporaba podatkov, grafov in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse netržne vsebine ob navedbi vira – [**covid-19.sledilnik.org**](http://covid-19.sledilnik.org/). Če ni določeno drugače, velja za vso vsebino na tej strani licenca Creative Commons: [Priznanje avtorstva-Deljenje pod enakimi pogoji 3.0](https://creativecommons.org/licenses/by-sa/3.0/deed.sl).

Za izvoz podatkov v drugih oblikah, uporabo za vizualizacije ali druge oblike sodelovanja nas kontaktirajte na info@sledilnik.org.

## 
