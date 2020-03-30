<template>
  <highcharts :options="chartOptions"></highcharts>
</template>

<script>
import { mapGetters } from "vuex";
import Highcharts from "highcharts";

export default {
  name: "Chart",
  props: {
    title: String,
    dataseries: Array,
    type: {
      default: 'line',
      type: String
    },
    highchartOptions: Object
  },
  data() {
    return {}
  },
  computed: {
    ...mapGetters("hospitals", {
      data: "data",
      getSeries: "getSeries",
      hospitalName: "hospitalName"
    }),
    chartOptions() {
      let chartSeries = this.generateSeries(this.dataseries)

      let opts = {
        chart: {
          type: this.type,
        },
        title: {
          text: this.title,
        },
        xAxis: {
          type: "datetime",
          labels: {
            formatter: function() {
              return Highcharts.dateFormat("%a %d %b", this.value)
            }
          }
        },
        series: chartSeries
      };

      return Object.assign({}, this.highchartOptions, opts)
    }
  },
  methods: {
    seriesKey(hospitalId, suffix) {
      if (hospitalId != "") {
        return `hospital.${hospitalId}.${suffix}`;
      } else {
        return `hospital.${suffix}`;
      }
    },
    generateSeries(dataseries) {
      return dataseries.map(obj => {
        return {
          name: obj.name,
          data: this.getSeries(obj.key)
        };
      });
    }
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="sass">
</style>