<template>
  <b-tabs justified class="tables-tabs" pills card @activate-tab="goToData">
    <b-tab title="Povzetek stanja" active>
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
        <template v-slot:table-caption>This is a table caption.</template>
      </b-table>
    </b-tab>
    <b-tab
      title="Prenos podatkov"
      title-item-class="ml-auto data-redirect-link"
      no-body
    ></b-tab>
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
  },
  methods: {
    goToData(newTabIndex, prevTabIndex, bvEvent) {
      if (
        document
          .querySelector(`.tables-tabs li:nth-child(${newTabIndex + 1})`)
          .classList.contains("data-redirect-link")
      ) {
        bvEvent.preventDefault();
        window.location = "/data";
      }
    }
  }
};
</script>
<style lang="scss">
.tables-tabs{
  .nav-pills {
    .nav-item {
      display: flex;
      justify-content: center;
      padding: 15px 15px;
      margin-bottom: -10px;
    }
    .nav-link {
      border-radius: 0;
      color: #000000;
      line-height: 0.8;
      border-bottom: 10px solid transparent;
      padding: 0;
    }
    .nav-link:hover,
    .nav-link.active {
      color: #000000;
      font-weight: bold;
      background-color: transparent;
      border-bottom: 10px solid $yellow;
    }
  }
}

</style>