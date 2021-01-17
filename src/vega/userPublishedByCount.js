export default function ($t) {
  return {
    "$schema": "https://vega.github.io/schema/vega-lite/v4.json",
    "width": "container",
    "height": 250,
    "data": {
      "url": "https://ostanizdrav.sledilnik.org/plots/usersByCount.json"
    },
    "transform": [
      {
        "calculate": $t('charts.ostanizdrav.userPublishedByCount.tooltip'),
        "as": "tt"
      }
    ],
    "layer": [
      {
        "encoding": {
          "x": {
            "title": $t('charts.ostanizdrav.userPublishedByCount.date'),
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
            "field": "key_count",
            "type": "quantitative",
            "scale": {
              "domain": [
                0,
                14
              ]
            },
            "axis": {
              "title": $t('charts.ostanizdrav.userPublishedByCount.count')
            }
          },
          "color": {
            "title": $t('charts.ostanizdrav.userPublishedByCount.users'),
            "field": "user_count",
            "type": "quantitative"
          }
        },
        "layer": [
          {
            "mark": "point"
          },
          {
            "selection": {
              "label": {
                "type": "single",
                "nearest": true,
                "on": "mouseover",
                "encodings": [
                  "x"
                ],
                "empty": "none"
              }
            },
            "mark": "point",
            "encoding": {
              "opacity": {
                "condition": {
                  "selection": "label",
                  "value": 1
                },
                "value": 0
              }
            }
          }
        ]
      },
      {
        "mark": {
          "type": "line",
          "interpolate": "monotone",
          "point": true
        },
        "encoding": {
          "x": {
            "timeUnit": "utcyearmonthdate",
            "field": "date",
            "type": "temporal"
          },
          "y": {
            "field": "average_key_count_per_user",
            "type": "quantitative"
          }
        }
      },
      {
        "transform": [
          {
            "filter": {
              "selection": "label"
            }
          }
        ],
        "layer": [
          {
            "mark": {
              "type": "rule",
              "color": "gray"
            },
            "encoding": {
              "x": {
                "timeUnit": "utcyearmonthdate",
                "field": "date",
                "type": "temporal",
                "axis": {
                  "labelAngle": 0
                }
              }
            }
          },
          {
            "encoding": {
              "text": {
                "type": "text",
                "field": "tt"
              },
              "x": {
                "timeUnit": "utcyearmonthdate",
                "field": "date",
                "type": "temporal",
                "axis": {
                  "labelAngle": 0
                }
              },
              "y": {
                "type": "quantitative",
                "field": "key_count"
              }
            },
            "layer": [
              {
                "mark": {
                  "type": "text",
                  "stroke": "white",
                  "strokeWidth": 2,
                  "align": "right",
                  "dx": -5,
                  "dy": -5
                }
              },
              {
                "mark": {
                  "type": "text",
                  "align": "right",
                  "dx": -5,
                  "dy": -5
                },
                "encoding": {
                  "color": {
                    "type": "quantitative",
                    "field": "user_count",
                    "scale": {
                      "scheme": {
                        "name": "blues",
                        "extent": [
                          0.25,
                          1
                        ]
                      }
                    }
                  }
                }
              }
            ]
          }
        ]
      }
    ]
  }
}