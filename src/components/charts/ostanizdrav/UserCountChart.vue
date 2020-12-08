<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.usercount.title')"
    name="usercount-chart"
    :data="plotData"
    :description="$t('charts.ostanizdrav.usercount.description')"
    faqAnchor="ostanizdrav-usercount"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import usercountPlot from "@/vega/usercount";
import i18n from "@/i18n";

export default {
  name: "UserCountChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: usercountPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>