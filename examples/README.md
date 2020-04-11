# Examples

## Embedding visualizations into your webpage

You can embed visualizations by using iframe. For example, see `example.html` in this directory.

### Charts

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

| Chart | Url |
| ----- | --- |
| Širjenje COVID-19 v Sloveniji | https://covid-19.sledilnik.org/embed.html#/chart/MetricsComparison |
| Obravnava hospitaliziranih | https://covid-19.sledilnik.org/embed.html#/chart/Patients |
| Prirast potrjeno okuženih | https://covid-19.sledilnik.org/embed.html#/chart/Spread |
| Potrjeno okuženi po regijah | https://covid-19.sledilnik.org/embed.html#/chart/Regions |
| Potrjeno okuženi po starostnih skupinah | https://covid-19.sledilnik.org/embed.html#/chart/AgeGroups |
| Potrjeno okuženi po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Municipalities |
| Zemljevid potrjeno okuženih po občinah | https://covid-19.sledilnik.org/embed.html#/chart/Map |