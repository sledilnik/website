<template>
  <b-container v-if="loaded">
    <b-row>
      <h1>Statistika</h1>
    </b-row>
    <b-row>
      <b-card-group deck class="col-12 mb-5">
        <Info-card
          title="Pozitivnih testov"
          :value="positiveTestToDate.value"
          :value-date="positiveTestToDate.date"
        />
        <Info-card
          title="Trenutno v bolnišnici"
          :value="inHospitalToDate.value"
          :value-date="positiveTestToDate.date"
        />
        <Info-card title="Mrtvi" :value="deceasedToDate.value" :value-date="deceasedToDate.date" />
        <Info-card
          title="Odpuščeni iz bolnišnice"
          :value="recoveredToDate.value"
          :value-date="recoveredToDate.date"
        />
      </b-card-group>
    </b-row>
    <b-row>
      <b-col class="col-6">
        <b-card title="Testiranje" sub-title="Skupno stevilo testiranih in pozitivnih testov">
          <b-card-body>
            <LineChart :chartdata="chartTests.data" :options="chartTests.options" />
          </b-card-body>
        </b-card>
      </b-col>
      <b-col class="col-6">
        <b-card title="Po regijah" sub-title="Pozitivni testi po regijah">
          <b-card-body>
            <LineChart :chartdata="chartRegions.data" :options="chartRegions.options" />
          </b-card-body>
        </b-card>
      </b-col>
    </b-row>
  </b-container>
  <b-container v-else>
    <div>
      <div class="d-flex justify-content-center mb-3">
        <b-spinner label="Loading..."></b-spinner>
      </div>
    </div>
  </b-container>
</template>

<script>
import * as d3 from "d3";
import InfoCard from "components/cards/InfoCard";
import LineChart from "components/charts/LineChart";

import StatsData from "StatsData";

export default {
  name: "StatsPage",
  components: {
    InfoCard,
    LineChart
  },
  props: {
    name: String,
    content: Promise
  },
  data() {
    return {
      loaded: false,
      csvdata: null,
      charts: {
        tests: {
          data: null,
          options: null
        },
        regions: {
          data: null,
          options: null
        }
      }
    };
  },
  computed: {
    positiveTestToDate() {
      return this.getLastValue(this.csvdata, "tests.positive.todate");
    },
    inHospitalToDate() {
      return this.getLastValue(this.csvdata, "state.in_hospital");
    },
    deceasedToDate() {
      return this.getLastValue(this.csvdata, "state.deceased.todate");
    },
    recoveredToDate() {
      return this.getLastValue(this.csvdata, "state.out_of_hospital.todate");
    },
    chartTests() {
      // if (!this.csvdata) {
      //   return {};
      // }
      let seriesPositive = this.csvdata.map(p => {
        return {
          t: new Date(Date.parse(p["date"])),
          y: p["tests.positive.todate"]
        };
      });

      let seriesPositiveNew = this.csvdata.map(p => {
        return {
          t: new Date(Date.parse(p["date"])),
          y: p["tests.positive"]
        };
      });

      return {
        data: {
          datasets: [
            {
              label: "Pozitivnih testov",
              data: seriesPositive,
              backgroundColor: "rgba(153,255,51,0.4)",
              yAxisID: "positive"
            },
            {
              label: "Novih pozitivnih testov",
              data: seriesPositiveNew,
              backgroundColor: "rgba(50,255,91,0.4)",
              yAxisID: "positive"
            }
          ]
        },
        options: {
          responsive: true,
          label: "Graf",
          scales: {
            yAxes: [
              {
                id: "positive",
                type: "linear"
              }
            ],
            xAxes: [
              {
                type: "time",
                time: {
                  unit: "day"
                }
              }
            ]
          }
        }
      };
    },
    chartRegions() {
      // if (!this.csvdata) {
      //   return {};
      // }
      let seriesRegions = [];

      this.csvdata.columns.forEach(col => {
        if (col.startsWith("region.")) {
          seriesRegions[col] = this.csvdata.map(p => {
            return {
              t: new Date(Date.parse(p["date"])),
              y: p[col]
            };
          });
        }
      });

      let labelMap = {
        "region.lj.todate": "Ljubljana",
        "region.mb.todate": "Maribor",
        "region.nm.todate": "Novo mesto",
        "region.kr.todate": "Kranj",
        "region.za.todate": "Zagorje",
        "region.ce.todate": "Celje",
        "region.sg.todate": "Slovenj Gradec",
        "region.po.todate": "Postojna",
        "region.ms.todate": "Murska Sobota",
        "region.kp.todate": "Koper",
        "region.ng.todate": "Nova Gorica",
        "region.kk.todate": "Krsko",
        "region.foreign.todate": "Tujina",
        "region.unknown.todate": "Neznano"
      };

      let translateKey = key => {
        return labelMap[key];
      };

      let datasets = Object.keys(seriesRegions).map(key => {
        return {
          label: translateKey(key),
          data: seriesRegions[key],
          yAxisID: "count"
        };
      });

      return {
        data: {
          datasets: datasets
        },
        options: {
          responsive: true,
          label: "Graf",
          scales: {
            yAxes: [
              {
                id: "count",
                type: "linear"
              }
            ],
            xAxes: [
              {
                type: "time",
                time: {
                  unit: "day"
                }
              }
            ]
          }
        }
      };
    }
  },
  methods: {
    // loadData() {
    //   return d3.csv(
    //     "https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"
    //   );
    // },
    getLastValue(csvdata, field) {
      if (csvdata && csvdata.length > 0) {
        let i = 0;
        // find last non null value
        for (i = csvdata.length - 1; i > 0; i--) {
          let row = csvdata[i];
          if (row[field]) {
            break;
          }
        }

        let lastRow = csvdata[i];
        let value = lastRow[field] || "N/A";
        return {
          date: new Date(Date.parse(lastRow["date"])),
          value: value
        };
      } else {
        return {
          date: new Date(),
          value: "N/A"
        };
      }
    }
  },
  async mounted() {
    this.csvdata = await StatsData.data();
    this.loaded = true;
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
