<img src="https://covid-19.sledilnik.org/covid-19-logo.png" width="200">

# Embed Sledilnik.org charts - Examples

## Embedding visualizations into your webpage

You can embed visualizations from [Sledilnik.org](https://covid-19.sledilnik.org/) by using `<iframe>`.
For example, see [`embed.html`](https://github.com/sledilnik/website/tree/master/examples/embed.html) and [`embed_multi.html`](https://github.com/sledilnik/website/tree/master/examples/embed_multi.html).

## Cards
We prepared status cards for all hospitals that they can integrate on their internet/intranet.

For example, see [`embed_cards.html`](https://github.com/sledilnik/website/tree/master/examples/embed_cards.html).

## Charts

| Chart                       | Url                                                                |
|-----------------------------|--------------------------------------------------------------------|
| Stanje COVID-19 v Sloveniji | https://covid-19.sledilnik.org/embed.html#/chart/MetricsComparison |
| Stanje COVID-19 v Evropi    | https://covid-19.sledilnik.org/embed.html#/chart/EuropeMap         |
| Stanje COVID-19 v svetu     | https://covid-19.sledilnik.org/embed.html#/chart/WorldMap          |
| Hospitalizirani             | https://covid-19.sledilnik.org/embed.html#/chart/Patients          |
| Intenzivna terapija         | https://covid-19.sledilnik.org/embed.html#/chart/IcuPatients       |
| Negovalne bolnišnice        | https://covid-19.sledilnik.org/embed.html#/chart/CarePatients      |
| Kapacitete bolnišnic        | https://covid-19.sledilnik.org/embed.html#/chart/Hospitals         |
| Hospitalizirani starost     | https://covid-19.sledilnik.org/embed.html#/chart/PatientsAge      |
| Delež resnih primerov       | https://covid-19.sledilnik.org/embed.html#/chart/Ratios            |
| Testiranje                  | https://covid-19.sledilnik.org/embed.html#/chart/Tests             |
| Obravnava v ZD              | https://covid-19.sledilnik.org/embed.html#/chart/HCenters          |
| Potrjeni primeri            | https://covid-19.sledilnik.org/embed.html#/chart/Cases             |
| Prirast potrjeno okuženih   | https://covid-19.sledilnik.org/embed.html#/chart/Spread            |
| Struktura potrjeno okuženih | https://covid-19.sledilnik.org/embed.html#/chart/Infections        |
| Potrjeno okuženi po regijah | https://covid-19.sledilnik.org/embed.html#/chart/Regions           |
| Zemljevid po regijah        | https://covid-19.sledilnik.org/embed.html#/chart/RegionMap         |
| Zemljevid po občinah        | https://covid-19.sledilnik.org/embed.html#/chart/Map               |
| Primeri po občinah          | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities    |
| Stanje po šolah             | https://covid-19.sledilnik.org/embed.html#/chart/SchoolStatus      |
| Po starostnih skupinah      | https://covid-19.sledilnik.org/embed.html#/chart/AgeGroups         |

### SchoolStatus: show specific school
Parameters:
`schoolId`=
- `id` (ID of school - see [dict-schools.csv](https://github.com/sledilnik/data/blob/master/csv/dict-schools.csv) for list of school IDs)

| Chart                                  | Url                                                                         |
|----------------------------------------|-----------------------------------------------------------------------------|
| Vrtec Koper                            | https://covid-19.sledilnik.org/embed.html#/chart/SchoolStatus?schoolId=1065 |
| OŠ Komenda Moste Podružnica Moste      | https://covid-19.sledilnik.org/embed.html#/chart/SchoolStatus?schoolId=3545 |
| Mladinski dom Malči Beličeve Ljubljana | https://covid-19.sledilnik.org/embed.html#/chart/SchoolStatus?schoolId=3577 |

### Municipalities: filter specific region or change sort order
Parameters:
`sort`=
- `total-positive-tests`
- `weekly-growth`

`region`=
- `ms` (Pomurska)
- `mb` (Podravska)
- `sg` (Koroška)
- `ce` (Savinjska)
- `za` (Zasavska)
- `kk` (Posavska)
- `nm` (Jugovzhodna Slovenija)
- `lj` (Osrednjeslovenska)
- `kr` (Gorenjska)
- `po` (Primorsko-notranjska)
- `ng` (Goriška)
- `kp` (Obalno-kraška)

`search`=
- `search string`

| Chart                          | Url                                                                                           |
|--------------------------------|-----------------------------------------------------------------------------------------------|
| Občine - Pomurska              | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?region=ms                     |
| Občine - Savinjska, rast       | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?region=ce&sort=weekly-growth |
| Občine - rast                  | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?sort=weekly-growth           |
| Občine - Medvode               | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?search=medvode                |

### Resize

Javascript is used to resize iframe height to fit the chart upon loading. It is not mandatory, you can also use fixed height on iframe.

```
...
<script>
window.addEventListener("message", function(event) {
      if (event.data.type === "embed-size") {
        console.log("resize event recieved", event.data)
        var iframe = document.querySelector("iframe[name='" + event.data.name + "']");
        if (iframe != null) {
          iframe.style.height = event.data.height + "px";
        }
      }
    });
</script>
<iframe id="myFrame" frameBorder="0" width="100%" height="0" name="my-iframe" src="https://covid-19.sledilnik.org/embed.html#/chart/MetricsComparison"></iframe>
...
```
