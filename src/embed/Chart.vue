<template>
  <b-container class="mt-3">
    <b-row cols="12">
      <b-col>
        <div id="visualizations"></div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { Visualizations } from "visualizations/App.fsproj";

export default {
  name: "ChartEmbed",
  async mounted() {
    //console.log(this.$route.params, this.$route.query)
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
      Visualizations("visualizations", this.$route.params.type);
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'
@import 'node_modules/bootstrap/scss/_mixins'

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
    $bar-color-light: #d5c668

    h2
      margin-bottom: $gap

    .municipalities
      display: flex
      flex-wrap: wrap

      .municipality
        width: calc(25% - #{$gap * 2})
        margin: $gap/2 $gap

        @include media-breakpoint-down(md)
          width: calc(50% - #{$gap * 2})

        @include media-breakpoint-down(sm)
          width: 100%
          margin: $gap/4 0

    .name
      color: $gray-700
      padding-right: $gap/2
      font-size: 1.1rem
      text-overflow: ellipsis
      white-space: nowrap
      overflow: hidden

    .positive-tests
      padding-top: $gap/4
      display: flex
      align-items: flex-start
      justify-content: flex-end
      border-bottom: solid 1px $gray-300

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

    .doubling-time
      margin-top: 5px
      text-align: right

      .label
        color: $gray-500

      .value
        font-weight: bold
        color: white
        padding: 1px 5px 2px 5px
        background-color: $button-color
        border-radius: 2px

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

  .exponential-explainer
    zIndex: 1000
    border: 1px solid #ddd
    background: rgba(220,220,220,0.75)
    padding: 2em
    backdrop-filter: blur(4px)

    h1
      color: #111
      margin-top: 1em
      font-size: 23px
      text-align: center
      font-weight: bold
      padding-bottom: 1.2em

    div.container
      display: flex
      justify-content: space-between
      flex-flow: row wrap

      div.box
        flex: none
        flex-basis: 0
        padding: 2.2em 2.2em 1.4em 2em
        border: 1px solid #ddd
        background: #fff
        text-align: center
        h2
          display: block
          font-size: 20px
          height: 3em
          border-bottom: 1px solid #ddd
        span
          display: block
          height: 1em
          margin-top: -1em
        p
          margin-top: -1em
          margin-bottom: 1em
        h4
          margin-top: 1.4em
          font-size: 24px
</style>
