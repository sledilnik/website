# Podatki

Da bi zagotovili maksimalno natančnost in zanesljivost podatkov v zbirki, skupnost Sledilnik za vnos in obdelavo podatkov uporablja (navzkrižno preverjane) javno dostopne podatke iz __uradnih virov__, kot so NIJZ, Vlada RS, Ministrstvo za zdravje itn., iz __administrativnih virov zdravstvenega sistema__, kot so UKC Ljubljana, UKC Maribor, UK Golnik in drugi, __virov Civilne zaščite__ ter iz __nacionalnih in lokalnih medijev__.

<a href="https://nijz.si"><img src="https://www.nijz.si/sites/www.nijz.si/files/uploaded/logotip-01.jpg" alt="NIJZ" width="300"/></a>
<a href="https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-zdravje/"><img src="https://www.skupine.si/mma/logo%20Ministrstvo%20za%20zdravje%20RS/2017102413574462/mid/
" alt="Ministrstvo za zdravje" width="300"/></a>

Zbrani in preverjeni podatki so na voljo v obliki **CSV datotek**, **REST API-ja** in **Google Docs preglednic**. Nadaljna uporaba podatkov kot tudi grafov in sodelovanje pri zbiranju, obdelavi in prikazu so dobrodošli in zaželjeni. Več informacij o načinu in pogojih uporabe ter sodelovanju je na voljo v poglavju [O projektu](/sl/about).

## Tabela

Podatke lahko neposredno pregledujete tudi na strani [Tabela](/sl/tables).

## CSV datoteke

- [stats.csv](https://github.com/sledilnik/data/blob/master/csv/stats.csv)
- [stats-weekly.csv](https://github.com/sledilnik/data/blob/master/csv/stats-weekly.csv)
- [region-cases.csv](https://github.com/sledilnik/data/blob/master/csv/region-cases.csv)
    - [region-active.csv](https://github.com/sledilnik/data/blob/master/csv/region-active.csv)
    - [region-confirmed.csv](https://github.com/sledilnik/data/blob/master/csv/region-confirmed.csv)
    - [region-deceased.csv](https://github.com/sledilnik/data/blob/master/csv/region-deceased.csv)
- [municipality-cases.csv](https://github.com/sledilnik/data/blob/master/csv/municipality-cases.csv)
    - [municipality-active.csv](https://github.com/sledilnik/data/blob/master/csv/municipality-active.csv)
    - [municipality-confirmed.csv](https://github.com/sledilnik/data/blob/master/csv/municipality-confirmed.csv)
    - [municipality-deceased.csv](https://github.com/sledilnik/data/blob/master/csv/municipality-deceased.csv)
- [vaccination.csv](https://github.com/sledilnik/data/blob/master/csv/vaccination.csv)
- [patients.csv](https://github.com/sledilnik/data/blob/master/csv/patients.csv)
    - [icu.csv](https://github.com/sledilnik/data/blob/master/csv/icu.csv)
- [hospitals.csv](https://github.com/sledilnik/data/blob/master/csv/hospitals.csv)
- [health_centers.csv](https://github.com/sledilnik/data/blob/master/csv/health_centers.csv)
- [retirement_homes.csv](https://github.com/sledilnik/data/blob/master/csv/retirement_homes.csv)
- [safety_measures.csv](https://github.com/sledilnik/data/blob/master/csv/safety_measures.csv)
- **Slovarji**:
    - [dict-hospitals.csv](https://github.com/sledilnik/data/blob/master/csv/dict-hospitals.csv)
    - [dict-retirement_homes.csv](https://github.com/sledilnik/data/blob/master/csv/dict-retirement_homes.csv)
    - [dict-region.csv](https://github.com/sledilnik/data/blob/master/csv/dict-region.csv)
    - [dict-muncipality.csv](https://github.com/sledilnik/data/blob/master/csv/dict-municipality.csv)
    - [dict-age-groups.csv](https://github.com/sledilnik/data/blob/master/csv/dict-age-groups.csv)
    - [dict-risk-factors-country.csv](https://github.com/sledilnik/data/blob/master/csv/dict-risk-factors-country.csv)


Celotni podatki skupaj z izvorno kodo za njihovo obdelavo so na voljo na [GitHub](https://github.com/sledilnik/data/)-u.


## REST API
- [summary](https://api.sledilnik.org/api/summary)
- [stats](https://api.sledilnik.org/api/stats)
- [stats-weekly](https://api.sledilnik.org/api/stats-weekly)
- [lab-tests](https://api.sledilnik.org/api/lab-tests)
- [regions](https://api.sledilnik.org/api/regions)
- [municipalities](https://api.sledilnik.org/api/municipalities)
- [patients](https://api.sledilnik.org/api/patients)
- [hospitals](https://api.sledilnik.org/api/hospitals)
- [health-centers](https://api.sledilnik.org/api/health-centers)
- [monthly-deaths-slovenia](https://api.sledilnik.org/api/monthly-deaths-slovenia)
- [daily-deaths-slovenia](https://api.sledilnik.org/api/daily-deaths-slovenia)
- [age-daily-deaths-slovenia](https://api.sledilnik.org/api/age-daily-deaths-slovenia)
- **Slovarji**:
    - [hospitals-list](https://api.sledilnik.org/api/hospitals-list)
    - [retirement-homes-list](https://api.sledilnik.org/api/retirement-homes-list)

Izvorna kodo in več informacij o REST API-ju je na voljo na [GitHub](https://github.com/sledilnik/data-api/)-u.


## Google Docs preglednica

Izvirna Google Docs preglednica, ki jo je začel sestavljati Luka Renko 11.3.2020, je še vedno dostopna na prvotni povezavi:

- [https://tinyurl.com/sledilnik-gdocs](https://tinyurl.com/sledilnik-gdocs)

Zaradi obsežnosti in tehničnih omejitev smo jo 11.12.2020 prenehali osveževati, podatke pa migrirali in prilagodili novim potrebam.


## Ukrepi in omejitve

Ob uporabi razpoložljivih virov podatkov smo se trudili kar se da celovito zbrati in povzeti trenutno veljavne ukrepe, ki jih je sprejela slovenska vlada kot odgovor na pandemijo covid-19, predvsem na izbranih področjih, ki se najbolj dotikajo vsakdanjega življenja. Trenutni ukrepi so zbrani na strani [Ukrepi in omejitve](/sl/restrictions).

## Viri podatkov

Da bi zagotovili maksimalno natančnost in zanesljivost podatkov v zbirki, podatke zbiramo in primerjamo iz večih uradnih virov. Če smo kak relevanten vir spregledali, nas lahko o tem, prosimo, obvestite na [info@sledilnik.org](mailto:info@sledilnik.org)

| Vlada                                                                                                                          | Ostale službe                                                                             |
| ------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------- |
| [NIJZ](https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19) ([Tw](https://twitter.com/NIJZ_pr/with_replies)) | [CZ Ilirska Bistrica](https://www.facebook.com/obcina.ilirskabistrica.73)                 |
| [Vlada RS](https://www.gov.si/teme/koronavirus/) ([Tw](https://twitter.com/vladaRS/with_replies))                              | [CZ Notranjska](https://www.facebook.com/regijskistabcznotranjska/)                       |
| [Ministrstvo za zdravje RS](https://www.gov.si/novice/?org[]=33) ([Tw](https://twitter.com/MinZdravje/with_replies))           | [CZ Sežana](https://www.facebook.com/civilnazascitasezana/)                               |
| [Tomaž Gantar - minister za zdravje](https://twitter.com/tomazgantar)                                                          | [CZ Žiri](https://www.facebook.com/groups/civilnazascitaziri/)                            |
| [Jelko Kacin - govorec Vlade RS za COVID-19](https://twitter.com/GovorecCOVID19/with_replies)                                  | [CZ Logatec](https://www.facebook.com/zascitaresevanjeLogatec/)                           |
| [Uprava RS za zaščito in reševanje](https://twitter.com/URS_ZR/with_replies)                                                   | [CZ Vrhnika](https://www.facebook.com/Civilna-za%C5%A1%C4%8Dita-Vrhnika-107764814187703/) |
| [Krizni štab RS](https://twitter.com/KrizniStabRS/with_replies) - ukinjen                                                      | [CZ Gorenjska](https://www.facebook.com/stabczgorenjska)                                  |


| Zdravstveni sistem                                                               | Nacionalni mediji                                                                                                                                    |
| -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| [UKC Ljubljana](https://twitter.com/ukclj/with_replies)                          | [Delo](https://www.delo.si/tag/koronavirus)                                                                                                          |
| [UKC Maribor](https://twitter.com/UKCMaribor/with_replies)                       | [RTVSLO.si](https://www.rtvslo.si/zdravje/novi-koronavirus)                                                                                          |
| [UK Golnik](https://www.klinika-golnik.si/novice)                                | [24ur.com](https://www.24ur.com/novice)                                                                                                              |
| [SB Celje](https://twitter.com/CeljeSb/with_replies)                             | [Dnevnik](https://www.dnevnik.si/slovenija)                                                                                                          |
| [SB Novo Mesto](https://twitter.com/sbnovomesto/with_replies)                    | [Večer](https://www.vecer.com/koronavirus-novice)                                                                                                    |
| [SB Brežice](https://www.sb-brezice.si/)                                         | [Žurnal24](https://www.zurnal24.si/slovenija)                                                                                                        |
| [SB Izola](https://www.sb-izola.si/si/aktualno/)                                 | [STA](https://www.sta.si/v-srediscu/koronavirus2020)                                                                                                 |
| [SB Jesenice](https://www.sb-je.si/aktualno/aktualne_novice/)                    | [Pod črto](https://podcrto.si/dosje/koronavirus/)  ([Tw](https://twitter.com/podcrto?lang=en))                                                       |
| [SB Murska Sobota](https://www.sb-ms.si/mediji-in-javnost/sporocila-za-javnost/) | [Necenzurirano](https://necenzurirano.si/rubrika/dosjeji/koronavirus) ([Tw](https://twitter.com/necenzurirano_/with_replies))                        |
| [SB Ptuj](http://www.sb-ptuj.si/aktualno/novice/novice/)                         |                                                                                                                                                      |
| [SB Slovenj Gradec](https://www.sb-sg.si/)                                       |                                                                                                                                                      |
| [SB Šempeter pri Novi Gorici](http://www.bolnisnica-go.si/aktualno)              |                                                                                                                                                      |
| [SB Trbovlje](http://www.sb-trbovlje.si/)                                        |                                                                                                                                                      |


| Lokalni mediji                                                |                                                                |
| ------------------------------------------------------------- | -------------------------------------------------------------- |
| [Gorenjski glas](http://www.gorenjskiglas.si/)                | [Primorske novice](https://www.primorske.si/)                  |
| [Domžalec](https://domzalec.si/)                              | [Regional obala](https://www.regionalobala.si/)                |
| [Domžalsko-kamniške novice](https://www.domzalske-novice.si/) | [Lokalne Ajdovščina](https://www.lokalne-ajdovscina.si/)       |
| [Kamnik.info](https://www.kamnik.info/novice_kamnik/)         | [Idrija.com](https://www.idrija.com/)                          |
| [Radio Sora](https://www.radio-sora.si/novice)                | [Notranjsko primorske novice](https://notranjskoprimorske.si/) |
| &nbsp;                                                        | &nbsp;                                                         |
| [Koroške Novice](https://www.koroskenovice.si/)               | [Maribor24.si](https://maribor24.si/)                          |
| [Savinjsko-Šaleške Novice](https://sasa-novice.si/)           | [Celje.info](https://www.celje.info/)                          |
| [Savinjske novice](http://savinjske.com/)                     | [Novi Tednik Celje](http://www.nt-rc.si/novi-tednik/)          |
| &nbsp;                                                        | [Kozjansko.info](https://kozjansko.info/)                      |
| &nbsp;                                                        | &nbsp;                                                         |
| [Moja-dolenjska](https://moja-dolenjska.si/)                  | [Sobotainfo](https://sobotainfo.com/)                          |
| [Dolenjski list](https://www.dolenjskilist.si/si/novice/)     | [Pomurec](https://www.pomurec.com/)                            |
| [ePosavje](https://www.eposavje.com/)                         | [Lendavainfo](http://lendavainfo.com/)                         |
| [Posavski obzornik](https://www.posavskiobzornik.si/)         | [Prlekija-on.net](https://www.prlekija-on.net/)                |
| [Naše Zasavje](https://nase-zasavje.si/)                      | [Vestnik MS](https://vestnik.si/)                              |


| Ostali viri informacij                                                                                                                                                       |     |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --- |
| [Nova metodologija diagnosticiranja obolelih](https://www.gov.si/novice/2020-03-14-spremenjeno-diagnosticiranje-za-realnejse-nacrtovanje-ukrepov-za-obvladovanje-epidemije/) |     |
| [Tabele o poročanju - Navodila za organizacijo dela](https://www.gov.si/novice/2020-03-17-navodila-za-organizacijo-dela-obravnavo-bolnika-in-dnevno-porocanje/)              |     |
| [Pojasnilo UKC-LJ o hospitaliziranih pacientih](https://twitter.com/ukclj/status/1242123118161911808)                                                                        |     |
| [Register prostorskih enot, Geodetska uprava RS](https://www.e-prostor.gov.si/zbirke-prostorskih-podatkov/nepremicnine/register-prostorskih-enot/)                           |     |
