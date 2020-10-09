<template>
  <b-container>
    <b-row class="d-flex justify-content-center">
      <div id="validVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="publishedVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="validByRiskVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="publishedByRiskVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="usercountVis"></div>
    </b-row>
    <b-row class="d-flex justify-content-center">
      <div id="userPublishedByCountVis"></div>
    </b-row>
  </b-container>
</template>

<script>
import embed from "vega-embed";

import validPlot from "@/vega/valid.json";
import validByRiskPlot from "@/vega/validByRisk.json";
import userPublishedByCountPlot from "@/vega/userPublishedByCount.json";
import usercountPlot from "@/vega/usercount.json";
import publishedByRiskPlot from "@/vega/publishedByRisk.json";
import publishedPlot from "@/vega/published.json";

export default {
  name: "OstaniZdravPage",
  mounted() {
    embed("#validVis", validPlot, {"actions": false});
    embed("#publishedVis", publishedPlot, {"actions": false});
    embed("#validByRiskVis", validByRiskPlot, {"actions": false});
    embed("#publishedByRiskVis", publishedByRiskPlot, {"actions": false});
    embed("#usercountVis", usercountPlot, {"actions": false});
    embed("#userPublishedByCountVis", userPublishedByCountPlot, {"actions": false});
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
</style>
