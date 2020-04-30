<img src="https://covid-19.sledilnik.org/covid-19-logo.png" width="200">

# Embed Sledilnik.org charts - Examples

## Embedding visualizations into your webpage

You can embed visualizations from [Sledilnik.org](https://covid-19.sledilnik.org/) by using `<iframe>`.
For example, see [`embed.html`](https://github.com/sledilnik/website/tree/master/examples/embed.html) and [`embed_multi.html`](https://github.com/sledilnik/website/tree/master/examples/embed_multi.html).

## Charts

| Chart | Url |
| ----- | --- |
| Širjenje COVID-19 v Sloveniji | https://covid-19.sledilnik.org/embed.html#/chart/MetricsComparison |
| Obravnava hospitaliziranih | https://covid-19.sledilnik.org/embed.html#/chart/Patients |
| Testiranje | https://covid-19.sledilnik.org/embed.html#/chart/Tests |
| Potrjeni primeri | https://covid-19.sledilnik.org/embed.html#/chart/Cases |
| Prirast potrjeno okuženih | https://covid-19.sledilnik.org/embed.html#/chart/Spread |
| Struktura potrjeno okuženih | https://covid-19.sledilnik.org/embed.html#/chart/Infections |
| Potrjeno okuženi po regijah | https://covid-19.sledilnik.org/embed.html#/chart/Regions |
| Zemljevid potrjeno okuženih po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Map |
| Potrjeno okuženi po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities |
| Po starostnih skupinah | https://covid-19.sledilnik.org/embed.html#/chart/AgeGroups |

### Municipalities: filter specific region or change sort order
Parameters:
`sort`=
- `total-positive-tests`
- `time-to-double`

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

| Chart | Url |
| ----- | --- |
| Občine - Pomurska | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?region=ms |
| Občine - Savinjska, podvojitev | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?region=ce&sort=time-to-double |
| Občine - podvojitev | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities?sort=time-to-double |

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
