<template>
  <b-card :title="title" class="card-info">
    <b-card-text
      class="text-center"
      :class="{'text-danger': diff > 0, 'text-info': diff==0, 'text-success': diff < 0}"
    >
      <font-awesome-icon icon="arrow-circle-up" v-if="diff > 0" />
      <font-awesome-icon icon="arrow-circle-down" v-if="diff < 0" />
      <font-awesome-icon icon="arrow-circle-right" v-if="diff == 0" />&nbsp;
      <span>{{ value }}</span>
      <span v-if="diff != 0">
      [{{ diff | prefixDiff }} | {{ percentDiff | prefixDiff }}%]
      </span>
    </b-card-text>
    <b-card-text class="data-time text-center">{{ formattedDate }}</b-card-text>
  </b-card>
</template>
<script>
import Vue from "vue";
import moment from "moment";
import StatsData from "StatsData";
import LineChart from "components/charts/LineChart";

Vue.filter("prefixDiff", function(value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});

export default {
  components: {
    LineChart
  },
  props: ["title", "field"],
  data() {
    return {
      valueDate: null,
      value: null,
      percentDiff: null,
      diff: null,
      chartdata: {
        labels: ["January", "February"],
        datasets: [
          {
            data: [
              {
                t: new Date(2020, 3, 25),
                y: 10
              },
              {
                t: new Date(2020, 3, 24),
                y: 3
              },
              {
                t: new Date(2020, 3, 23),
                y: 1
              }
            ]
          }
        ]
      },
      chartoptions: {
        padding: 0,
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          yAxes: [
            {
              gridLines: {
                drawBorder: false,
                drawOnChartArea: false,
                display: false
              }
            }
          ],
          xAxes: [
            {
              type: "time",
              gridLines: {
                drawBorder: false,
                drawOnChartArea: false,
                display: false
              }
            }
          ]
        }
      }
    };
  },
  computed: {
    formattedDate() {
      let dateFormatted = moment(this.date).calendar(null, {
        lastDay: "[včeraj]",
        sameDay: "[danes]",
        lastWeek: "[last] dddd",
        nextWeek: "dddd",
        sameElse: "L"
      });
      return `Podatki do ${dateFormatted}`;
    }
  },
  async mounted() {
    let data = await StatsData.getLastValue(this.field);
    this.date = data.date;
    this.value = data.value;

    let dayBefore = moment(data.date)
      .subtract(1, "days")
      .toDate();
    let dataBefore = await StatsData.getValueOn(this.field, dayBefore);

    this.diff = this.value - dataBefore.value;
    this.percentDiff = Math.round((this.diff / this.value) * 1000) / 10;

    console.log(this.field, this.date, this.diff, this.percentDiff);
  }
};
</script>

<style scoped lang="scss">
@import "node_modules/bootstrap/scss/bootstrap";

.card.card-info {
  color: $text-muted;

  .card-title {
    text-align: center;
    font-size: $font-size-base;

    // text-transform: uppercase;
  }

  .data-time {
    font-size: $font-size-sm * 0.7;
  }
}
</style>