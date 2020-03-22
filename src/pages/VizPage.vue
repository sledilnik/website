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
import { Visualizations } from 'visualizations/src/App.fsproj'
export default {
  name: "VizPage",
  mounted() {
    Visualizations("visualizations")
  }
}
</script>

<!-- <style scoped lang="sass"> -->
<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'
#visualizations
  $gap: $grid-gutter-width
  $font-size: 12px

  margin-top: $gap
  font-size: $font-size

  .metrics-selectors
    margin-top: $gap
    display: flex
    flex-wrap: wrap
    justify-content: center

  .metric-selector
    margin: 0 $gap/6 $gap/3 $gap/6
    border-color: $gray-300
    &:hover
      border-color: $gray-500

  .metric-selector--selected
    color: white

  .data-table
    margin-top: $gap
    font-size: $font-size

    thead
      th
        vertical-align: top

    tbody
      td
        width: 11.1%

</style>
