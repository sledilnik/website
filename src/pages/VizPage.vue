<template>
  <b-container fluid class="h-100">
    <b-row cols="12" class="h-100">
      <b-col>
        <div id="visualizations"></div>
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { Visualizations } from 'visualizations/App.fsproj'
export default {
  name: "VizPage",
  mounted() {
    Visualizations("visualizations")
  }
}
</script>

<!-- TODO <style scoped lang="sass"> ne deluje -->

<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'
#visualizations
  $gap: $grid-gutter-width
  $font-size: 12px

  font-size: $font-size

  h2
    margin-bottom: $gap
    text-align: center

  .table
    td, th
      padding: 6px 9px

  .metric-comparison-chart
    margin-top: $gap

    .metrics-selectors
      margin-top: $gap
      display: flex
      flex-wrap: wrap
      justify-content: center

    .metric-selector
      margin: 0 $gap/6 $gap/3 $gap/6
      border-color: $gray-300
      font-size: $font-size
      &:hover
        border-color: $gray-500

    .metric-selector--selected
      color: white

  .age-group-chart
    margin-top: $gap

  .data-table
    margin-top: $gap

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
