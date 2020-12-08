<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.publishedByRisk.title')"
    name="publishedByRisk-chart"
    :data="plotData"
    :description="$t('charts.ostanizdrav.publishedByRisk.description')"
    faqAnchor="ostanizdrav-publishedByRisk"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import publishedByRiskPlot from "@/vega/publishedByRisk";
import i18n from "@/i18n";

export default {
  name: "PublishedByRiskChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: publishedByRiskPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>