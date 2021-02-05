<template>
  <div>
    <div class="table-container">
      <div class="table-wrapper">
        <h1>{{ $t("tables.title") }}</h1>
        <b-tabs>
          <b-tab :title="$t('tables.tabSummary')" active>
            <tests-infections-table :tableHeight="tableHeight"></tests-infections-table>
          </b-tab>
          <b-tab :title="$t('tables.tabByRegion')">
            <regional-overview-table :tableHeight="tableHeight"></regional-overview-table>
          </b-tab>
          <b-tab :title="$t('tables.tabByAgeMale')">
            <age-groups-males-table :table-height="tableHeight"></age-groups-males-table>
          </b-tab>
          <b-tab :title="$t('tables.tabByAgeFemale')">
            <age-groups-females-table :table-height="tableHeight"></age-groups-females-table>
          </b-tab>
        </b-tabs>
        <Loader v-if="!loaded" />
        <div class="footnote">
          {{ $t("tables.source") }}:
          <router-link to="datasources">{{ $t("tables.sourceLink") }}</router-link>
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
import Loader from '../components/Loader';

export default {
  components: {
    TestsInfectionsTable,
    RegionalOverviewTable,
    AgeGroupsMalesTable,
    AgeGroupsFemalesTable,
    Loader
  },
  data: function() {
    return {
      tableHeight: '100%',
      loaded: false
    };
  },
  created() {
    this.dispatch()
  },
  mounted() {
    this.$store.dispatch("tableData/refreshDataEvery", 1200);
  },
  methods: {
    async dispatch() {
      await this.$store.dispatch("tableData/fetchData");
      this.loaded = true;
    }
  }
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
  margin: 24px auto 0
  max-width: 1110px
  background: #fff

  @include mobile-break
    position: static
    background: none
    padding: 0 15px
    margin: 48px auto 64px

.table-wrapper
  background: #fff
  padding: 30px 0 15px 15px
  min-height: calc(100vh - 300px)
  display: flex
  flex-direction: column

  @include mobile-break
    box-shadow: $element-box-shadow
    padding: 32px 0 32px 32px

  h1
    margin-bottom: 30px

    @include mobile-break
      margin-bottom: 32px

  .table-bordered
    td,
    th
      border: none
      color: $table-text-c !important
      font-size: 14px

      &:not(.b-table-sticky-column)
        color: rgba(0, 0, 0, 0.7) !important
        text-align: right


  .border
    border: none !important

  //tabs
.nav.nav-tabs
  overflow-y: hidden
  overflow-x: scroll
  display: flex
  flex-wrap: nowrap
  margin: 0 0 26px
  border: none

  @include mobile-break
    margin: 0 0 32px

  .nav-item
    a
      white-space: nowrap

    & + .nav-item
      @include mobile-break
        margin-left: 32px

    @include mobile-break
      flex: none

  .nav-link
    padding: 0
    border: none
    position: relative
    color: rgba(0, 0, 0, 0.56)
    line-height: 29px
    margin-right: 15px
    font-size: 14px
    display: inline-block

    &.active
      color: black
      box-shadow: inset 0 -10px 0 $yellow

    &:hover
      color: black

    &:focus
      outline: none

.tabs
  margin-bottom: 0px

  @include mobile-break
    margin-bottom: 16px

.tabs *:focus
  outline: none

tr:hover
  .table-b-table-default.b-table-sticky-column
    background: #fff !important

thead .table-b-table-default
  background: white !important
  z-index: 5 !important
  border-bottom: 1px solid $table-border !important

  &.b-table-sticky-column
    @include mobile-break
      z-index: 6 !important
      background: $white-gradient !important

.text-nowrap
  color: $table-text-c
  font-size: 14px
  font-weight: 600

.table-wrapper
  .table
    th
      padding: 0.75rem 31px 10px 0
      text-align: left

      @include mobile-break
        padding: 12px 7px 18px 7px

      &:first-child
        div
          width: 150px

    tr:hover
      background: none

      .b-table-sticky-column
        @include mobile-break
          background: $white-gradient !important

    thead tr:hover
      background: #fff !important

      .b-table-sticky-column
        background: #fff !important

    td
      white-space: nowrap
      padding: 38px 31px 10px 0
      text-align: left
      border-bottom: 1px solid $table-border

      @include mobile-break
        padding: 12px 7px

      &:nth-child(even)
        background: none

        @include mobile-break
          background-color: rgba(0, 0, 0, 0.03)

    td[aria-colindex="1"].b-table-sticky-column
        padding: 10px 32px 38px 0px
        font-weight: bold
        text-align: left
        background: none !important

        @include mobile-break
          padding: 10px 64px 10px 10px
          background: $white-gradient !important

.table-grey.b-table-sticky-column
  .text-nowrap
    transform: translateY(-24px)
    font-weight: bold



.footnote
  font-size: 13px
  margin-top: auto

//alternating color
.table-wrapper
  .table-striped tbody tr:nth-of-type(odd)
    background-color: rgb(250, 250, 250)

  .table-grey
    background : none

#app.tables
  background: #fff

  @include mobile-break
    background: $background
</style>
