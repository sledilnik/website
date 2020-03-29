<template>
  <b-container fluid>
    <b-row>
      <b-col class="px-0" cols="12">
        <b-tabs justified class="tables-tabs" pills card>
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
        </b-tabs>
      </b-col>
    </b-row>
    <b-row>
      <b-col cols="12">
        <div class="footnote float-right">
          Viri podatkov:
          <a href="https://github.com/slo-covid-19/data/blob/master/csv/stats.csv">CSV</a>,
          <a href="https://covid19.rthand.com/api/stats">REST</a>,
          <a href="https://tinyurl.com/slo-covid-19">Google Sheet</a>
        </div>
      </b-col>
    </b-row>
    {{ thingy }}
  </b-container>
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
    ...mapGetters(["csvdata"]),
    thingy: function(){
      console.log(this.csvdata);
      return this.csvdata
    }
  },
};
</script>
<style lang="scss">

.footnote {
  font-size: $font-size-sm;
  // padding-right: 1.25rem;
}

.tables-tabs {
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