<template>
  <b-container>
    <b-row class="d-flex justify-content-center">
      <div id="validVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="validByRiskVis"></div>
    </b-row>
  </b-container>
</template>

<script>
import embed from "vega-embed";

import validPlot from "@/vega/valid.json";
import validByRiskPlot from "@/vega/validByRisk.json";

export default {
  name: "OstaniZdravPage",
  mounted() {
    embed("#validVis", validPlot);
    embed("#validByRiskVis", validByRiskPlot);
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
</style>
