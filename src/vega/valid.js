export default function ($t) {
  return {
    "$schema": "https://vega.github.io/schema/vega-lite/v4.json",
    "width": "container",
    "height": 250,
    "data": {
      "url": "https://ostanizdrav.sledilnik.org/plots/data.json"
    },
    "mark": {
      "type": "line",
      "interpolate": "monotone",
      "point": true
    },
    "encoding": {
      "x": {
        "timeUnit": "utcyearmonthdate",
        "field": "date",
        "type": "temporal",
        "axis": {
          "title": $t('charts.ostanizdrav.valid.date'),
          "labelAngle": 0,
          "tickCount": "week",
          "labelExpr": "[timeFormat(datum.value, '%d'), timeFormat(datum.value, '%d') <= '07' ? timeFormat(datum.value, '%b') : '']",
          "gridDash": {
            "condition": {
              "test": {
                "field": "value",
                "timeUnit": "day",
                "equal": 1
              },
              "value": []
            },
            "value": [
              2,
              2
            ]
          },
          "tickDash": {
            "condition": {
              "test": {
                "field": "value",
                "timeUnit": "day",
                "equal": 1
              },
              "value": []
            },
            "value": [
              2,
              2
            ]
          }
        }
      },
      "y": {
        "field": "valid",
        "type": "quantitative",
        "axis": {
          "title": $t('charts.ostanizdrav.valid.perDay')
        }
      }
    }
  }
}