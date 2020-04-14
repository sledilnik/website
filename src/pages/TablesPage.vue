<template>
  <div>
    <div class="table-container">
      <div class="table-wrapper">
        <h1>Tabela</h1>
        <b-tabs>
          <b-tab title="Povzetek stanja" active>
            <tests-infections-table :tableHeight="tableHeight"></tests-infections-table>
          </b-tab>
          <b-tab title="Po regiji">
            <regional-overview-table :tableHeight="tableHeight"></regional-overview-table>
          </b-tab>
          <b-tab title="Po starosti - Moški">
            <age-groups-males-table :table-height="tableHeight"></age-groups-males-table>
          </b-tab>
          <b-tab title="Po starosti - Ženske">
            <age-groups-females-table :table-height="tableHeight"></age-groups-females-table>
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
import TestsInfectionsTable from '../components/tables/TestsInfections';
import RegionalOverviewTable from '../components/tables/RegionalOverview';
import AgeGroupsMalesTable from '../components/tables/AgeGroupsMales';
import AgeGroupsFemalesTable from '../components/tables/AgeGroupsFemales';

import { mapGetters } from 'vuex';
export default {
  components: {
    TestsInfectionsTable,
    RegionalOverviewTable,
    AgeGroupsMalesTable,
    AgeGroupsFemalesTable,
  },
  data: function() {
    return {
      tableHeight: '100%',
    };
  },
};
</script>
<style scopped lang="sass">
$white-gradient: linear-gradient(90deg, rgba(255,255,255,1) 0%, rgba(255,255,255,0.9) 80%, rgba(255,255,255,0) 100%, rgba(255,255,255,1) 100%)
$table-text-c: rgba(0, 0, 0, 0.75)
$table-border: rgb(222,222 ,222)

@mixin mobile-break
  @media only screen and (min-width: 768px)
    @content

.table-container
  margin: -24px auto 0
  max-width: 1110px
  background: #fff

  @media only screen and (min-width: 768px)
    position: static
    background: none
    padding: 0 15px
    margin: 0 auto 64px

.table-wrapper
  background: #fff
  padding: 30px 0 15px 15px

  @media only screen and (min-width: 768px)
    box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.15)
    padding: 32px 0 32px 32px

  h1
    margin-bottom: 32px

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

  //tabs
.nav.nav-tabs
  overflow: hidden
  display: flex
  flex-wrap: nowrap
  margin: 0 0 28px
  border: none

  @media only screen and (min-width: 768px)
    margin: 0 0 48px

  .nav-item
    margin-bottom: 4px
    a
      white-space: nowrap

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

tr:hover
  .table-b-table-default.b-table-sticky-column
    background: #fff !important

thead .table-b-table-default
  background: white !important
  z-index: 5 !important
  border-bottom: 1px solid $table-border !important

.text-nowrap
  color: $table-text-c
  font-size: 14px
  font-weight: 600
.table-wrapper
  .table
    th
      padding: 0.75rem 31px 0.75rem 0
      text-align: left
      &:first-child
        div
          width: 150px
    tr:hover
      background: rgba(0, 0, 0, 0.03)

      .b-table-sticky-column
        background: none !important

    thead tr:hover
      background: #fff !important
      .b-table-sticky-column
        background: #fff !important

    td
      min-width: 95px
      white-space: nowrap
      padding: 38px 31px 10px 0
      text-align: left
      border-bottom: 1px solid $table-border

      // @include mobile-break
      //   padding: 10px 0.75rem 10px 0

      &:nth-child(even)
        background: none

        // @include mobile-break
        //   background-color: rgba(0, 0, 0, 0.03)

    td[aria-colindex="1"].b-table-sticky-column
        padding: 10px 32px 38px 0px
        font-weight: bold
        text-align: left
        background: none

        // @include mobile-break
        //   padding: 10px 32px 10px 10px
        //   background: $white-gradient

.table-grey.b-table-sticky-column
  .text-nowrap
    transform: translateY(-24px)
    font-weight: bold



.footnote
  font-size: 13px
  margin-top: 15px

  @media only screen and (min-width: 768px)
    margin-top: 32px

//alternating color
.table-wrapper
  .table-striped tbody tr:nth-of-type(odd)
    background-color: rgb(250, 250, 250)

  .table-grey
    background : none

#app.tables
  background: #fff

  @media only screen and (min-width: 768px)
    background: $background
</style>
