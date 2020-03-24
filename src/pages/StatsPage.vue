<template>
  <b-container v-if="loaded">
    <b-row>
      <b-card-group deck class="col-12 mb-5">
        <Info-card
          title="Pozitivnih testov"
          :value="positiveTestToDate.value"
          :value-date="positiveTestToDate.date"
        />
        <Info-card
          title="Hospitalizirani"
          :value="inHospitalToDate.value"
          :value-date="positiveTestToDate.date"
        />
        <Info-card title="Umrli" :value="deceasedToDate.value" :value-date="deceasedToDate.date" />
        <Info-card
          title="Odpuščeni iz bolnišnice"
          :value="recoveredToDate.value"
          :value-date="recoveredToDate.date"
        />
      </b-card-group>
    </b-row>
    <b-row cols="12">
      <b-col>
        <div id="visualizations"></div>
      </b-col>
    </b-row>
  </b-container>
  <b-container v-else>
    <b-row>
      <b-col>
        <div class="d-flex justify-content-center mb-3">
          <b-spinner label="Loading..."></b-spinner>
        </div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import InfoCard from "components/cards/InfoCard";
import { Visualizations } from "visualizations/App.fsproj";

import StatsData from "StatsData";

export default {
  name: "StatsPage",
  components: {
    InfoCard
  },
  props: {
    name: String,
    content: Promise
  },
  data() {
    return {
      loaded: false,
      csvdata: null
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
    }
  },
  methods: {
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
    this.$nextTick(() => {
      Visualizations("visualizations");
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'

#visualizations
  $gap: $grid-gutter-width
  $font-size: 12px

  font-size: $font-size

  h2
    margin-bottom: $gap
    text-align: center

  .table
    td, th
      padding: 6px 9px

  .metrics-selectors
      margin-top: $gap
      display: flex
      flex-wrap: wrap
      justify-content: center

  .metric-selector
      margin: 0 $gap/6 $gap/3 $gap/6
      border-color: $gray-300
      font-size: $font-size
      &:hover
      border-color: $gray-500

  .metric-selector--selected
      color: white

  .metric-comparison-chart
    margin-top: $gap

  .age-group-chart
    margin-top: $gap

  .regions-chart
    margin-top: $gap

  .data-table
    margin-top: $gap

    .table
      font-size: $font-size

      thead
        th
          vertical-align: top
          border-bottom: none

      tbody
        td
          width: 11.1%

      tr
        &:hover
          background-color: $gray-100
</style>
