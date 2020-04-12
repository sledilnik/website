![logo](https://covid-19.sledilnik.org/covid-19-logo.png?v=4&s=200)

# Embed Sledilnik.org charts - Examples 

## Embedding visualizations into your webpage

You can embed visualizations from [Sledilnik.org](https://covid-19.sledilnik.org/) by using `iframe`. 
For example, see `example.html` in this directory.

### Charts

| Chart | Url |
| ----- | --- |
| Širjenje COVID-19 v Sloveniji | https://covid-19.sledilnik.org/embed.html#/chart/MetricsComparison |
| Obravnava hospitaliziranih | https://covid-19.sledilnik.org/embed.html#/chart/Patients |
| Prirast potrjeno okuženih | https://covid-19.sledilnik.org/embed.html#/chart/Spread |
| Potrjeno okuženi po regijah | https://covid-19.sledilnik.org/embed.html#/chart/Regions |
| Zemljevid potrjeno okuženih po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Map |
| Potrjeno okuženi po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities |
| Potrjeno okuženi po starostnih skupinah | https://covid-19.sledilnik.org/embed.html#/chart/AgeGroups |


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
