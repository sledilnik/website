<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.published.title')"
    name="published-chart"
    :data="plotData"
    :description="$t('charts.ostanizdrav.published.description')"
    faqAnchor="ostanizdrav-published"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import publishedPlot from "@/vega/published";
import i18n from "@/i18n";

export default {
  name: "PublishedChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: publishedPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>