<template>
  <b-container fluid class="tables-page">
    <b-row>
      <b-col class="px-0" cols="12">
        <b-tabs justified class="tables-tabs" pills card>
          <b-tab title="Povzetek stanja" active>
            <tests-infections-table :csvdata="csvdata"></tests-infections-table>
          </b-tab>
          <b-tab title="Po regiji">
            <regional-overview-table :csvdata="csvdata"></regional-overview-table>
          </b-tab>
          <b-tab title="Po starosti - Moški">
            <age-groups-males-table :csvdata="csvdata"></age-groups-males-table>
          </b-tab>
          <b-tab title="Po starosti - Ženske">
            <age-groups-females-table :csvdata="csvdata"></age-groups-females-table>
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
  </b-container>
</template>

<script>
import TestsInfectionsTable from "../components/tables/TestsInfections";
import RegionalOverviewTable from "../components/tables/RegionalOverview";
import AgeGroupsMalesTable from "../components/tables/AgeGroupsMales";
import AgeGroupsFemalesTable from "../components/tables/AgeGroupsFemales";

import { mapGetters } from "vuex";
export default {
  components: {
    TestsInfectionsTable,
    RegionalOverviewTable,
    AgeGroupsMalesTable,
    AgeGroupsFemalesTable
  },
  computed: {
    ...mapGetters(["csvdata"])
  }
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

.table-b-table-default {
  background: #dee2e6 !important;
}

.tables-page  {
  margin-top: -48px;
  height: 100%;
  .tables-tabs {
    display: flex;
    flex-direction: column;

    .tab-content {
      flex: 1;
      overflow: auto;
    }
  }

  @media all and (max-width: 500px) {
    margin-top: 0;
  }
  @media all and (max-device-width: 400px) {
    .nav-pills {
      a {
        padding: 5px 10px;
        font-size: 13px;
      }
    }
  }
}
</style>