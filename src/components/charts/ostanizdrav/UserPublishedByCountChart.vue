<template>
  <VegaChart
    :title="$t('charts.ostanizdrav.userPublishedByCount.title')"
    name="userPublishedByCount"
    :data="plotData"
    :description="$t('charts.ostanizdrav.userPublishedByCount.description')"
    faqAnchor="ostanizdrav-userPublishedByCount"
  />
</template>
<script>
import VegaChart from "@/components/charts/VegaChart";
import userPublishedByCountPlot from "@/vega/userPublishedByCount";
import i18n from "@/i18n";

export default {
  name: "UserPublishedByCountChart",
  components: {
    VegaChart,
  },
  data() {
    return {
      plotData: userPublishedByCountPlot((id) => i18n.i18next.t(id)),
    };
  },
};
</script>