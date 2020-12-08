<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.validByRisk.title')"
    name="validByRisk-chart"
    :data="plotData"
    :description="$t('charts.ostanizdrav.validByRisk.description')"
    faqAnchor="ostanizdrav-validByRisk"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import validByRiskPlot from "@/vega/validByRisk";
import i18n from "@/i18n";

export default {
  name: "ValidByRiskChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: validByRiskPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>