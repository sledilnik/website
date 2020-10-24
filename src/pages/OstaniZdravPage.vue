<template>
  <div>
    <div class="custom-container">
      <section class="static-page-wrapper">
        <h1>
          {{ $t("charts.ostanizdrav.title") }}
        </h1>
        <div v-html-md="$t('charts.ostanizdrav.description')"></div>
      </section>
    </div>
    <div class="visualizations container">
      <PublishedChart />
      <UserCountChart />
      <UserPublishedByCountChart />
      <PublishedByRiskChart />
      <ValidChart />
      <ValidByRiskChart />
      <div class="credits" v-html-md="$t('charts.ostanizdrav.footer')"></div>
    </div>
  </div>
</template>

<script>
import PublishedChart from "@/components/charts/ostanizdrav/PublishedChart";
import UserCountChart from "@/components/charts/ostanizdrav/UserCountChart";
import UserPublishedByCountChart from "@/components/charts/ostanizdrav/UserPublishedByCountChart";
import PublishedByRiskChart from "@/components/charts/ostanizdrav/PublishedByRiskChart";
import ValidChart from "@/components/charts/ostanizdrav/ValidChart";
import ValidByRiskChart from "@/components/charts/ostanizdrav/ValidByRiskChart";

export default {
  name: "OstaniZdravPage",
  components: {
    PublishedChart,
    UserCountChart,
    UserPublishedByCountChart,
    PublishedByRiskChart,
    ValidChart,
    ValidByRiskChart,
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss" scoped>
.visualizations {
  display: flex;
  flex-direction: column;
  justify-content: center;
  margin: 30px auto 0;
  max-width: 930px;

  @media only screen and (min-width: 768px) {
    margin: 88px auto 0;
  }
}
</style>
