<template>
  <b-container class="mt-3 stats-page">
    <div class="cards-wrapper latest-data-boxes">
      <Info-card title="Potrjeno okuženi" field="tests.positive.todate" />
      <Info-card title="Hospitalizirani" field="state.in_hospital" />
      <Info-card title="V intenzivni enoti" field="state.icu" />
      <Info-card title="Umrli" field="state.deceased.todate" />
      <Info-card title="Ozdraveli" field="state.recovered.todate" good-direction="up" />
    </div>
    <b-row v-show="loaded" cols="12">
      <b-col>
        <div id="visualizations" class="visualizations"></div>
      </b-col>
    </b-row>
    <loader v-show="!loaded"></loader>
  </b-container>
</template>

<script>
import Loader from 'components/Loader';
import InfoCard from 'components/cards/InfoCard';
import { Visualizations } from 'visualizations/App.fsproj';

import { mapState } from 'vuex';

export default {
  name: 'StatsPage',
  components: {
    InfoCard,
    Loader,
  },
  props: {
    name: String,
    content: Promise,
  },
  data() {
    return {
      loaded: false,
    };
  },
  computed: {
    ...mapState('stats', {
      cardsLoaded: 'loaded',
    }),
  },
  mounted() {
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations('visualizations');
    });

    // stupid spinner impl, but i do not know better (charts are react component, no clue when they are rendered)
    let checker = setInterval(() => {
      // search for class visualization
      let elm = document.querySelector('.visualization');
      if (elm) {
        this.loaded = true;
        clearInterval(checker);
      }
    }, 100);
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass">
@import 'node_modules/bootstrap/scss/_functions'
@import 'node_modules/bootstrap/scss/_variables'
@import 'node_modules/bootstrap/scss/_mixins'

h1,
h2,
h3,
h4
  font-family: 'IBM Plex Mono', monospace

$background: #f5f5f0
$yellow: #ffd922
$text-c: rgba(0, 0, 0, 0.7)
$box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22)

.visualizations
  $gap: $grid-gutter-width
  $font-size: 12px
  $primary-color: #17a2b8
  font-size: $font-size
  $button-color: $gray-600
  $inactive-button-color: $gray-300
  $text-c: rgba(0, 0, 0, 0.7)

  section
    background: #fff
    margin-bottom: 88px
    position: relative
    padding: 70px 30px 30px

  h2
    position: absolute
    z-index: 10
    left: 32px
    top: 0
    transform: translateY(-50%)
    margin-bottom: $gap / 2
    font-family: 'IBM Plex Sans', sans-serif
    font-size: 18px
    font-weight: bold
    padding: 12px 16px
    border: 2px solid $yellow
    background: #fff
    box-shadow: $box-shadow

  p
    color: $text-c

  .btn.btn-sm.metric-selector
    border: none
    box-shadow: 0 3px 5px rgba(0,0,0,0.20)
    transition: all 0.2s

    &:hover
      box-shadow: 0 3px 11px rgba(0,0,0,0.20)

    &.metric-selector--selected
      box-shadow: none

      &:hover
        color: #fff

  .table
    td, th
      padding: 6px 9px

  .scale-type-selector
    margin: 0 $gap/2 $gap/2 0
    text-align: right
    color: $text-c
    font-size: 14px

    .scale-type-selector__item
      display: inline-block
      text-transform: capitalize
      line-height: 2
      margin-left: 24px
      color: rgba(0, 0, 0, 0.5)

      &::selection
        background: rgba(0, 0 , 0 , 0)

      &:hover
        cursor: pointer
        color: #000
        // box-shadow: inset 0 1px 0 white, inset 0 1px $yellow
        // animation: link-hover
        // animation-name: link-hover
        // animation-duration: 3s
        // animation-timing-function: ease-out
        // box-shadow: inset 0 1px 0 white, inset 0 -28px $yellow

        // @keyframes link-hover
        //   0%
        //     box-shadow: inset 0 1px 0 white, inset 0 1px $yellow
        //   100%
        //     box-shadow: inset 0 1px 0 white, inset 0 -28px $yellow

      &.selected
        color: #000
        box-shadow: inset 0 1px 0 white, inset 0 -9px $yellow

        &:hover
          animation: none

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

  .patients-chart, .age-groups-chart, .hospitals-chart
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

    .filters
      display: flex
      flex-wrap: wrap
      margin-bottom: $gap / 2

    .filters__query
      margin-left: $gap/2
      width: 15rem
      margin-bottom: $gap / 2

    .filters__region
      margin-left: $gap/2
      width: 15rem
      margin-bottom: $gap / 2

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
      width: 4px
      margin-right: 1px
      background-color: $bar-color-light

      &:hover
        background-color: $bar-color

    .bar--empty
      background-color: gray

    .show-all
      margin-top: 48px
      text-align: center

      > div
        margin-top: -15px
        padding: 0 $gap
        background-color: white

      .btn
        font-size: 14px
        font-family: 'IBM Plex Mono', monospace
        background-color: $yellow
        color: #000
        padding: 15px 16px
        border-radius: 0
        border: none
        box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22)
        &:active
          background-color: $yellow !important
          color: #000
        &:hover
          box-shadow: 0 6px 38px -14px rgba(0, 0, 0, 0.3), 0 11px 12px -10px rgba(0, 0, 0, 0.22)
        &:focus
          border: none
          outline: none

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
    box-shadow: 0 10px 20px rgba(0,0,0,0.19), 0 6px 6px rgba(0,0,0,0.23)
    background: #fff
    padding: 0
    bottom: 0
    top: -34px


    @media (min-width: 992px)
      padding: 2em
      top: 0
      bottom: 0

    h1
      font-family: "IBM Plex Sans", sans-serif
      font-weight: bold
      color: #111
      font-size: 19px
      text-align: center
      font-weight: bold
      margin-top: 0.5em
      padding: 0 1em 1em 1em

      @media (min-width: 992px)
        font-size: 23px
        margin-top: 1em
        padding: 0 0 1.2em 0

    div.container
      display: flex
      flex-flow: row wrap
      padding-left: 0
      padding-right: 0

      @media (min-width: 992px)
        justify-content: space-between
        flex-flow: row nowrap
        padding-left: 15px
        padding-right: 15px

      div.box
        flex: 0 0 calc(100% / 3)
        padding: 0
        background: #fff
        text-align: center
        margin-bottom: 1.5em

        &:nth-child(2),
        &:nth-child(5)
          border-left: 2px solid $yellow
          border-right: 2px solid $yellow

        h3
          font-family: "IBM Plex Sans", sans-serif
          font-weight: bold
          display: block
          font-size: 13px

        span
          display: block
          height: 1em
          padding-top: 0.2em
        p
          margin-top: -1em
          margin-bottom: 1em

        div:last-child
          p:last-child
           margin-bottom: 0

        h4
          margin-top: 0.8em
          font-size: 24px

        @media (min-width: 992px)
          flex: none
          flex-basis: 0
          padding: 2.2em 2.2em 1.4em 2em
          border: none
          background: #fff
          text-align: center
          box-shadow: 0 10px 20px rgba(0,0,0,0.19), 0 6px 6px rgba(0,0,0,0.23)

          &:nth-child(2),
          &:nth-child(5)
            border: none

          h3
            font-family: "IBM Plex Sans", sans-serif
            font-weight: bold
            display: block
            font-size: 18px
            min-width: 5.2em
            height: 3em
            border-bottom: 1px solid #ddd
          span
            display: block
            height: 1em
            padding-top: 0.6em
          p
            margin-top: -1em
            margin-bottom: 1em
          h4
            margin-top: 1.4em
            font-size: 24px

.cards-wrapper
  display: flex
  max-width: 1140px
  flex-wrap: wrap
  margin: 0 auto 58px
</style>

<style lang="scss">
// don't know sass
.latest-data-boxes {
  .hp-card {
    height: 100%;
  }
}
@media all and (max-width: 1200px) {
  .visualizations .container {
    max-width: 100%;
  }
  .latest-data-boxes .hp-card-holder {
    flex: 0 0 33.333%;
  }
}
@media all and (max-width: 700px) {
  .visualizations {
    margin: 0 -15px;
    .visualization section {
      padding: 70px 15px 10px;
    }
  }
  .latest-data-boxes .hp-card-holder {
    flex: 0 0 50%;
  }
  .stats-page.container-fluid {
    > .row {
      margin-left: 0;
      margin-right: 0;
    }
    padding: 0;
  }
}
@media all and (max-width: 500px) {
  .latest-data-boxes {
    .hp-card-holder,
    .hp-card {
      padding: 15px;
    }
    .hp-card-holder {
      flex: 0 0 100%;
    }
  }
}
</style>
