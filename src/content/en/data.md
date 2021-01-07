# Data

In order to ensure a maximum accuracy and reliability of the data, Sledilnik community uses (cross-checked) publicly available data from __official governemnt sources__, such as the NIJZ, the Government of the Republic of Slovenia, the Ministry of Health, etc., from __health system sources__, such as UKC Ljubljana, UKC Maribor, UK Golnik and others, __Civil protection services__ and from the __national and local media__.

<a href="https://nijz.si"><img src="https://www.nijz.si/sites/www.nijz.si/files/uploaded/logotip-01.jpg" alt="NIJZ" width="300"/></a>
<a href="https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-zdravje/"><img src="https://www.skupine.si/mma/logo%20Ministrstvo%20za%20zdravje%20RS/2017102413574462/mid/
" alt="Ministrstvo za zdravje" width="300"/></a>

Collected and cross-checked data is available in the form of **CSV files**, **REST API** and **Google Docs sheets**. Reuse of data and visualizations is welcome and encouraged. For more information about the terms of reuse please see the [About page](/en/about).

## Table
You can also inspect the data directly on the [Tables page](/en/tables).

## CSV files

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
- **Dictionaries**:
    - [dict-hospitals.csv](https://github.com/sledilnik/data/blob/master/csv/dict-hospitals.csv)
    - [dict-retirement_homes.csv](https://github.com/sledilnik/data/blob/master/csv/dict-retirement_homes.csv)
    - [dict-region.csv](https://github.com/sledilnik/data/blob/master/csv/dict-region.csv)
    - [dict-muncipality.csv](https://github.com/sledilnik/data/blob/master/csv/dict-municipality.csv)
    - [dict-age-groups.csv](https://github.com/sledilnik/data/blob/master/csv/dict-age-groups.csv)
    - [dict-risk-factors-country.csv](https://github.com/sledilnik/data/blob/master/csv/dict-risk-factors-country.csv)


Complete data and source code used to process the data are available on [GitHub](https://github.com/sledilnik/data/).


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
- **Dictionaries**:
    - [hospitals-list](https://api.sledilnik.org/api/hospitals-list)
    - [retirement-homes-list](https://api.sledilnik.org/api/retirement-homes-list)


More information on how to use the API and the API source code are available on [GitHub](https://github.com/sledilnik/data-api/)-u.


## Google Docs Sheet


- [https://tinyurl.com/sledilnik-gdocs](https://tinyurl.com/sledilnik-gdocs)


## Measures and Restrictions

For collection of current measures and restrictions, see page [Measures and Restritions](/sl/restrictions).

## Data Sources

To ensure the precision and reliability of our data, we collect and cross-check the data from different official sources. In case we owerlooked any relevant source, please let us know at [info@sledilnik.org](mailto:info@sledilnik.org)

| Government                                                                                                                     | Civil protection and disaster relief services                                             |
| ------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------- |
| [NIJZ](https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19) ([Tw](https://twitter.com/NIJZ_pr/with_replies)) | [CZ Ilirska Bistrica](https://www.facebook.com/obcina.ilirskabistrica.73)                 |
| [Vlada RS](https://www.gov.si/teme/koronavirus/) ([Tw](https://twitter.com/vladaRS/with_replies))                              | [CZ Notranjska](https://www.facebook.com/regijskistabcznotranjska/)                       |
| [Ministrstvo za zdravje RS](https://www.gov.si/novice/?org[]=33) ([Tw](https://twitter.com/MinZdravje/with_replies))           | [CZ Sežana](https://www.facebook.com/civilnazascitasezana/)                               |
| [Tomaž Gantar - minister za zdravje](https://twitter.com/tomazgantar)                                                          | [CZ Žiri](https://www.facebook.com/groups/civilnazascitaziri/)                            |
| [Jelko Kacin - govorec Vlade RS za COVID-19](https://twitter.com/GovorecCOVID19/with_replies)                                  | [CZ Logatec](https://www.facebook.com/zascitaresevanjeLogatec/)                           |
| [Uprava RS za zaščito in reševanje](https://twitter.com/URS_ZR/with_replies)                                                   | [CZ Vrhnika](https://www.facebook.com/Civilna-za%C5%A1%C4%8Dita-Vrhnika-107764814187703/) |
| [Krizni štab RS](https://twitter.com/KrizniStabRS/with_replies) - ukinjen                                                      | [CZ Gorenjska](https://www.facebook.com/stabczgorenjska)                                  |


| Health system                                                                    | National media                                                                                                                                       |
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


| Local media                                                   |                                                                |
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


| Other sources                                                                                                                                                                |     |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --- |
| [Nova metodologija diagnosticiranja obolelih](https://www.gov.si/novice/2020-03-14-spremenjeno-diagnosticiranje-za-realnejse-nacrtovanje-ukrepov-za-obvladovanje-epidemije/) |     |
| [Tabele o poročanju - Navodila za organizacijo dela](https://www.gov.si/novice/2020-03-17-navodila-za-organizacijo-dela-obravnavo-bolnika-in-dnevno-porocanje/)              |     |
| [Pojasnilo UKC-LJ o hospitaliziranih pacientih](https://twitter.com/ukclj/status/1242123118161911808)                                                                        |     |
| [Register prostorskih enot, Geodetska uprava RS](https://www.e-prostor.gov.si/zbirke-prostorskih-podatkov/nepremicnine/register-prostorskih-enot/)                           |     |
