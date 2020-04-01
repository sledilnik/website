<template>
  <b-container class="mt-3">
    <b-row cols="12">
      <b-col class="embeded">
        <div id="visualizations" class="visualizations"></div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { Visualizations } from 'visualizations/App.fsproj';

export default {
  name: 'ChartEmbed',
  async mounted() {
    //console.log(this.$route.params, this.$route.query)
    this.$nextTick(() => {
      /* Available charts:
         - MetricsComparison
         - Patients
         - Spread
         - Regions
         - Municipalities
         - AgeGroups
         - Hospitals
      */
      Visualizations('visualizations', this.$route.params.type);
    });
  },
};
</script>
