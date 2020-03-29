<template>
  <b-container v-if="loaded" class="mt-3">
    <b-row class="row-gap">
      <b-card-group deck class="col-12 mb-5">
        <Info-card title="Potrjeno okuženi" field="tests.positive.todate" />
        <Info-card title="Hospitalizirani" field="state.in_hospital" />
        <Info-card title="Intenzivna nega" field="state.icu" />
        <Info-card title="Umrli" field="state.deceased.todate" />
        <Info-card title="Ozdraveli" field="state.recovered.todate" good-direction="up" />
      </b-card-group>
    </b-row>
    <b-row cols="12" class="row-gap">
      <b-col>
        <div id="visualizations"></div>
      </b-col>
    </b-row>
  </b-container>
  <b-container v-else class="mt-3">
    <b-row>
      <b-col>
        <div class="d-flex justify-content-center mb-3">
          <b-spinner label="Loading..."></b-spinner>
        </div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import InfoCard from "components/cards/InfoCard";
import { Visualizations } from "visualizations/App.fsproj";

export default {
  name: "StatsPage",
  components: {
    InfoCard
  },
  props: {
    name: String,
  },
  data() {
    return {
      loaded: false,
    };
  },
  async mounted() {
    this.loaded = true;
    this.$nextTick(() => {
      Visualizations("visualizations");
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'
@import 'node_modules/bootstrap/scss/_mixins'

.row-gap
  margin-top: 70px

#visualizations
  $gap: $grid-gutter-width
  $font-size: 12px
  $primary-color: #17a2b8
  font-size: $font-size
  $button-color: $gray-600
  $inactive-button-color: $gray-300

  h2
    margin-bottom: $gap / 2
    text-align: center

  .table
    td, th
      padding: 6px 9px

  .scale-type-selector
    margin: 0 $gap/2 $gap/2 0
    text-align: right

    .scale-type-selector__item
      display: inline-block
      padding: 0px 7px
      margin: 0 3px
      border-radius: 3px
      border: solid 1px $inactive-button-color
      border-color: solid 1px $inactive-button-color

      &:hover
        cursor: pointer

      &.selected
        color: white
        border-color: $button-color
        background-color: $button-color

  .metrics-selectors
    margin-top: $gap/2
    display: flex
    flex-wrap: wrap
    justify-content: center

  .metric-selector
    margin: 0 $gap/6 $gap/3 $gap/6
    font-size: $font-size
    border-color: $inactive-button-color

    &:hover
      border-color: $button-color

  .metric-selector--selected
    color: white

  .metrics-comparison-chart
    margin-top: 0

  .regions-chart
    margin-top: $gap * 2

  .age-groups-chart
    margin-top: $gap * 2
    margin-bottom: $gap
    h2
      margin-bottom: $gap

  .patients-chart, .age-groups-chart
    margin-top: $gap * 2

    .metric-selector
      background-color: white
    .metric-selector--selected
      border-color: $gray-600
      background-color: $gray-600

  .municipalities-chart
    margin-top: $gap * 2
    $bar-color: #7B7226
    $bar-color-light: #B0AA74

    h2
      margin-bottom: $gap

    .municipalities
      display: flex
      flex-wrap: wrap

      .municipality
        width: calc(25% - #{$gap * 2})
        margin: $gap/2 $gap
        border-bottom: solid 1px $gray-300

        @include media-breakpoint-down(md)
          width: calc(33% - #{$gap * 2})

        @include media-breakpoint-down(sm)
          width: 100%
          margin: $gap/4 0

    .name
      color: $gray-700
      padding-right: $gap/2
      font-size: $font-size-lg
      text-overflow: ellipsis
      white-space: nowrap
      overflow: hidden

    .positive-tests
      padding-top: $gap/4
      display: flex
      align-items: flex-start
      justify-content: flex-end

    .total-and-date
      padding-left: $gap/4

      .total
        line-height: 1.5rem
        font-size: $font-size-lg
        color: $bar-color

      .date
        color: $gray-600
        white-space: nowrap

    .total-and-date--hover
      display: none
      position: absolute
      top: 0px
      right: 20px
      text-align: right
      text-shadow: 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white, 0px 0px 4px white

    .bars
      display: flex
      align-items: flex-end
      justify-content: flex-end
      height: 50px

    .bar-wrapper
      position: relative
      display: flex
      align-items: flex-end
      height: 50px

      &:last-child
        .bar
          background-color: $bar-color

      &:hover
        .total-and-date--hover
          display: block

        &:last-child
          .total-and-date--hover
            display: none

    .bar
      width: 5px
      margin-right: 1px
      background-color: $bar-color-light

      &:hover
        background-color: $bar-color

    .bar--empty
      background-color: gray

    .show-all
      display: flex
      justify-content: center
      margin-top: $gap * 1.2
      border-top: solid 1px $gray-300

      > div
        margin-top: -15px
        padding: 0 $gap
        background-color: white

      .btn
        font-size: $font-size
        border-color: $button-color
        background-color: $button-color

  .data-table
    margin-top: $gap

    h2
      margin-bottom: $gap

    .table
      font-size: $font-size

    thead
      th
        vertical-align: top
        border-bottom: none

    tbody
      td
        width: 11.1%

    tr
      &:hover
        background-color: $gray-100
</style>
