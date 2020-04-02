<template>
  <div>
    <div class="table-container">
      <h1>Tabela</h1>
      <b-tabs>
        <b-tab title="Povzetek stanja" active>
          <tests-infections-table :csvdata="data" :tableHeight="tableHeight"></tests-infections-table>
        </b-tab>
        <b-tab title="Po regiji">
          <regional-overview-table :csvdata="data" :regions="regions" :tableHeight="tableHeight"></regional-overview-table>
        </b-tab>
        <b-tab title="Po starosti - Moški">
          <age-groups-males-table :csvdata="data" :table-height="tableHeight"></age-groups-males-table>
        </b-tab>
        <b-tab title="Po starosti - Ženske">
          <age-groups-females-table :csvdata="data" :table-height="tableHeight"></age-groups-females-table>
        </b-tab>
      </b-tabs>
    </div>

    <!-- <div class="footnote">
      Viri podatkov:
      <a href="https://github.com/slo-covid-19/data/blob/master/csv/stats.csv">CSV</a>,
      <a href="https://covid19.rthand.com/api/stats">REST</a>,
      <a href="https://tinyurl.com/slo-covid-19">Google Sheet</a>
    </div>-->
  </div>
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
  data: function() {
    return {
      tableHeight: "calc(100vh - 500px)"
    };
  },
  computed: {
    ...mapGetters("stats", ["data", "regions"])
  }
  // created() {
  //   window.addEventListener("resize", this.myEventHandler);
  //   // this.tableHeight = "calc(100vh - 500px)";
  // },
  // destroyed() {
  //   window.removeEventListener("resize", this.myEventHandler);
  // },
  // methods: {
  //   myEventHandler(e) {
  //     console.log(e);
  //     this.tableHeight = "calc(100vh - 500px)";
  //   }
  // }
};
</script>
<style lang="sass">
//new

.table-container
  position: absolute
  width: 95vw
  top: 120px
  bottom: 130px
  max-width: 1110px
  left: 50%
  transform: translateX(-50%)
  padding: 32px 0 32px 32px
  background: #fff
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.15)

  h1
    margin-bottom: 48px

//old
// .footnote {
  // font-size: $font-size-sm;
  // padding-right: 1.25rem;
// }

// $table__background: rgb(237, 237, 232);
// .tables-tabs {
//   .nav-pills {
//     .nav-item {
//       display: flex;
//       justify-content: center;
//       padding: 15px 15px;
//       margin-bottom: -10px;
//     }
//     .nav-link {
//       position: relative;
//       font-weight: 600;
//       color: $text-c;
//       z-index: 1;
//       background: none;
//       transition: all 0.35s ease-in-out;
//       box-shadow: inset 0 -4px 0 $table__background, inset 0 -7px $yellow;
//       text-decoration: none;
//       border-radius: 0;
//       padding: 0;

//       &:hover,
//       &.active {
//         background: none;
//         text-decoration: none;
//         color: $text-c;
//         font-weight: 600;
//         box-shadow: inset 0 -4px 0 $table__background, inset 0 -20px $yellow;
//       }
//     }
//   }
// }

// .table-b-table-default {
//   background: #dee2e6 !important;
// }

// .tables-page {
//   margin-top: -48px;
//   height: 100%;
//   .tables-tabs {
//     display: flex;
//     flex-direction: column;

//     .tab-content {
//       flex: 1;
//       overflow: auto;
//     }
//   }

//   @media all and (max-width: 500px) {
//     margin-top: 0;
//   }
//   @media all and (max-device-width: 400px) {
//     .nav-pills {
//       a {
//         padding: 5px 10px;
//         font-size: 13px;
//       }
//     }
//   }
// }
</style>
