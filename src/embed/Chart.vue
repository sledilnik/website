<template>
  <b-container class="mt-3">
    <b-row cols="12">
      <b-col class="embeded">
        <div id="visualizations" class="visualizations"></div>
        <!-- <a href>
          <img src="../assets/logo.png" class="embed-logo" />
        </a> -->
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { Visualizations } from 'visualizations/App.fsproj';

export default {
  name: 'ChartEmbed',
  async mounted() {
    // console.log(this.$route.params, this.$route.query)
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
      Visualizations('visualizations', 'chart', this.$route.query, this.$route.params.type);
    });
  },
};
</script>

<style lang="scss" scoped>
.embed-logo {
  position: absolute;
  width: 70px;
  // top: 24px;
  bottom: 24px;
  right: 70px;
  box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22);
}
</style>
