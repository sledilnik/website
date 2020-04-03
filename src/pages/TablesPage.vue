<template>
  <div>
    <div class="table-container">
      <div class="table-wrapper">
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
        <div class="footnote">
          Viri podatkov:
          <a href="https://github.com/slo-covid-19/data/blob/master/csv/stats.csv">CSV</a>,
          <a href="https://covid19.rthand.com/api/stats">REST</a>,
          <a href="https://tinyurl.com/slo-covid-19">Google Sheet</a>
        </div>
      </div>
    </div>
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
      tableHeight: "100%"
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
<style scopped lang="sass">
//new
$table-text-c: rgba(0, 0, 0, 0.75)
$table-border: rgb(222,222 ,222)

.table-container
  margin: -24px auto 0
  max-width: 1110px
  background: #fff

  @media only screen and (min-width: 768px)
    position: static
    background: none
    padding: 0 15px
    margin: 0 auto

.table-wrapper
  background: #fff
  padding: 24px 0 12px 15px

  @media only screen and (min-width: 768px)
    box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.15)
    padding: 32px 0 32px 32px

  h1
    margin-bottom: 18px

    @media only screen and (min-width: 768px)
      margin-bottom: 48px

  .table-bordered
    td,
    th
      border: none
      color: $table-text-c
      font-size: 14px

  .border
    border: none !important
//table height
.b-table-sticky-header.table-responsive
  height: calc(100vh - 329px)
  margin-bottom: 0

  @media only screen and (min-width: 768px)
    height: calc(100vh - 500px)

  //tabs
.nav.nav-tabs
  display: flex
  flex-wrap: nowrap
  flex-wrap: wrap
  margin: 0 0 4px
  border: none

  @media only screen and (min-width: 768px)
    margin: 0 0 48px

  .nav-item
    flex: 50%
    margin-bottom: 4px

    & + .nav-item
      @media only screen and (min-width: 768px)
        margin-left: 32px

    @media only screen and (min-width: 768px)
      flex: none
      margin-bottom: 0

  .nav-link
    padding: 0
    border: none
    position: relative
    color: rgba(0, 0, 0, 0.5)
    line-height: 20px
    margin-right: 15px
    font-size: 14px
    display: inline-block

    &.active
      color: rgba(0, 0, 0)

    &:hover
      color: rgba(0, 0, 0)

    &:focus
      outline: none

    &.active
      &:after
        content: ""
        position: absolute
        display: block
        left: 0
        right: 0
        z-index: 0
        bottom: -5px
        border-bottom: 10px solid $yellow

.tabs *:focus
  outline: none

//top left cell
thead .table-grey.b-table-sticky-column
  background-image: none !important
  background: rgba(255,255 ,255 ,0.9)
  z-index: 3 !important
  border-bottom: 1px solid $table-border
  .text-nowrap
    display: none

thead .table-b-table-default
  background: rgba(255,255 ,255 ,0.9) !important
  z-index: 4 !important
  border-bottom: 1px solid $table-border !important

.text-nowrap
  color: $table-text-c
  font-size: 14px
  font-weight: 600

.table-wrapper
  .table
    tr:hover
      background: #ffd92242 !important

    td
      padding: 44px 24px 12px
      border-bottom: 1px solid $table-border

.table-grey.b-table-sticky-column
  .text-nowrap
    transform: translateY(-32px)
    font-weight: bold

.footnote
  font-size: 13px
  margin-top: 12px

  @media only screen and (min-width: 768px)
    margin-top: 32px

//alternating color
.table-wrapper
  .table-striped tbody tr:nth-of-type(odd)
    background-color: #fff

  .table-grey
    background : none

#app.tables
  background: #fff

  @media only screen and (min-width: 768px)
    background: $background

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
