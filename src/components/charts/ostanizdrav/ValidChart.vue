<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.valid.title')"
    name="valid-chart"
    :data="plotData"
    :description="$t('charts.ostanizdrav.valid.description')"
    faqAnchor="ostanizdrav-valid"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import validPlot from "@/vega/valid";
import i18n from "@/i18n";

export default {
  name: "ValidChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: validPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>