<template>
  <b-tabs pills card>
    <b-tab title="Testiranja in okužbe" active>
      <tests-infections-table :csvdata="csvdata"></tests-infections-table>
    </b-tab>
    <b-tab title="Okužbe po regiji">
      <regional-overview-table :csvdata="csvdata"></regional-overview-table>
    </b-tab>
    <b-tab title="Okužbe po starosti">
      <age-groups-table :csvdata="csvdata"></age-groups-table>
    </b-tab>
    <b-tab title="Okužbe po spolu">
      <gender-overview-table :csvdata="csvdata"></gender-overview-table>
    </b-tab>
    <b-tab title="Vse">
      <b-table
        responsive
        bordered
        outlined
        striped
        hover
        sort-by="date"
        :sort-desc="true"
        sticky-header="600px"
        :items="csvdata"
      >
        <template v-slot:head()="scope">
          <div class="text-nowrap">{{ scope.label }}</div>
        </template>
      </b-table>
    </b-tab>
  </b-tabs>
</template>

<script>
import TestsInfectionsTable from "../components/tables/TestsInfections";
import RegionalOverviewTable from "../components/tables/RegionalOverview";
import GenderOverviewTable from "../components/tables/GenderOverview";
import AgeGroupsTable from "../components/tables/AgeGroups";

import { mapGetters } from "vuex";
export default {
  components: {
    TestsInfectionsTable,
    RegionalOverviewTable,
    GenderOverviewTable,
    AgeGroupsTable
  },
  computed: {
    ...mapGetters(["csvdata"])
  }
};
</script>