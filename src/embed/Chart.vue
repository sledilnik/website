<template>
  <b-container class="mt-3">
    <b-row cols="12">
      <b-col v-if="!isOstaniZdrav" class="embeded">
        <div id="visualizations" class="visualizations"></div>
        <!-- <a href>
          <img src="../assets/logo.png" class="embed-logo" />
        </a> -->
      </b-col>
      <b-col v-if="isOstaniZdrav" class="embeded">
        <div class="visualizations">
          <PublishedChart v-if="$route.params.type == 'OstaniZdravPublished'" />
          <UserCountChart v-if="$route.params.type == 'OstaniZdravUserCount'" />
          <UserPublishedByCountChart
            v-if="$route.params.type == 'OstaniZdravUserPublishedByCount'"
          />
          <PublishedByRiskChart
            v-if="$route.params.type == 'OstaniZdravPublishedByRisk'"
          />
          <ValidChart v-if="$route.params.type == 'OstaniZdravValid'" />
          <ValidByRiskChart
            v-if="$route.params.type == 'OstaniZdravValidByRisk'"
          />
        </div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { Visualizations } from "visualizations/App.fsproj";

// components containing #ostanizdrav charts
import PublishedChart from "@/components/charts/ostanizdrav/PublishedChart";
import UserCountChart from "@/components/charts/ostanizdrav/UserCountChart";
import UserPublishedByCountChart from "@/components/charts/ostanizdrav/UserPublishedByCountChart";
import PublishedByRiskChart from "@/components/charts/ostanizdrav/PublishedByRiskChart";
import ValidChart from "@/components/charts/ostanizdrav/ValidChart";
import ValidByRiskChart from "@/components/charts/ostanizdrav/ValidByRiskChart";
import { API_ENDPOINT_BASE } from '../services/api.service';

export default {
  name: "ChartEmbed",
  components: {
    PublishedChart,
    UserCountChart,
    UserPublishedByCountChart,
    PublishedByRiskChart,
    ValidChart,
    ValidByRiskChart,
  },
  computed: {
    isOstaniZdrav() {
      return (
        this.$route.params.type &&
        this.$route.params.type.startsWith("OstaniZdrav")
      );
    },
  },
  async mounted() {
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
      Visualizations(
        "visualizations",
        "chart",
        this.$route.query,
        API_ENDPOINT_BASE,
        this.$route.params.type
      );
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
  box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3),
    0 11px 12px -12px rgba(0, 0, 0, 0.22);
}
</style>
