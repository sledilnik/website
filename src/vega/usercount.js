export default function ($t) {
  return {
    "$schema": "https://vega.github.io/schema/vega-lite/v4.json",
    "width": "container",
    "height": 250,
    "data": {
      "url": "https://ostanizdrav.sledilnik.org/plots/data.json"
    },
    "transform": [
      {
        "window": [
          {
            "op": "sum",
            "field": "users_published",
            "as": "sum_published"
          }
        ]
      }
    ],
    "layer": [
      {
        "encoding": {
          "x": {
            "title": $t('charts.ostanizdrav.usercount.date'),
            "timeUnit": "utcyearmonthdate",
            "field": "date",
            "type": "temporal"
          },
          "y": {
            "field": "users_published",
            "type": "quantitative",
            "axis": {
              "title": $t('charts.ostanizdrav.usercount.users'),
              "titleColor": "steelblue"
            }
          },
          "text": {
            "field": "users_published",
            "type": "quantitative"
          }
        },
        "layer": [
          {
            "mark": {
              "type": "bar"
            }
          },
          {
            "mark": {
              "type": "text",
              "color": "steelblue",
              "align": "right",
              "baseline": "middle",
              "fontSize": 9,
              "angle": 90,
              "dx": -3,
              "dy": -4
            }
          }
        ]
      },
      {
        "encoding": {
          "x": {
            "timeUnit": "utcyearmonthdate",
            "field": "date",
            "type": "temporal",
            "axis": {
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
            "field": "sum_published",
            "type": "quantitative",
            "axis": {
              "title": $t('charts.ostanizdrav.usercount.runningTotal'),
              "titleColor": "#b4464b"
            }
          },
          "text": {
            "field": "sum_published",
            "type": "quantitative"
          }
        },
        "layer": [
          {
            "mark": {
              "type": "line",
              "interpolate": "monotone",
              "point": {
                "color": "#b4464b"
              },
              "color": "#b4464b"
            }
          }
        ]
      }
    ],
    "resolve": {
      "scale": {
        "y": "independent"
      }
    }
  }
}