<template>
  <b-container class="mt-3 stats-page">
    <div class="cards-wrapper latest-data-boxes">
      <Info-card title="Potrjeno okuženi" field="tests.positive.todate" />
      <Info-card title="Hospitalizirani" field="state.in_hospital" />
      <Info-card title="V intenzivni enoti" field="state.icu" />
      <Info-card title="Umrli" field="state.deceased.todate" />
      <Info-card title="Ozdraveli" field="state.recovered.todate" good-direction="up" />
    </div>
    <b-row cols="12">
      <b-col>
        <div id="visualizations" class="visualizations"></div>
      </b-col>
    </b-row>
    <!-- <loader v-show="!loaded"></loader> -->
  </b-container>
</template>

<script>
import { mapState } from 'vuex';

// import Loader from 'components/Loader';
import InfoCard from 'components/cards/InfoCard';

import { Visualizations } from 'visualizations/App.fsproj';

export default {
  name: 'StatsPage',
  components: {
    InfoCard,
    // Loader,
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
      let elm = document.querySelector('.highcharts-point');
      if (elm) {
        document.querySelector('.stats-page').classList.add('loaded');
        this.loaded = true;
        clearInterval(checker);
      }
    }, 80);
  },
};
</script>

<style lang="sass">
$loader-width: 50px

.cards-wrapper
  display: flex
  max-width: 1140px
  flex-wrap: wrap
  margin: 0px auto 58px

.stats-page
  position: relative

  &.loaded
    section
      &::before,
      &::after
        display: none

  section
    position: relative

    &::before,
    &::after
      content: ""
      z-index: 99
      display: block
      position: absolute
      top: calc(50% - 25px)
      left:  calc(50% - 25px)
      width: $loader-width
      height: $loader-width
      background-size: cover

    &::after
      background-image: url(../assets/svg/covid-animation-1.svg)

      animation: rotate1 3s infinite
      animation-timing-function: linear

      @keyframes rotate1
        0%
          transform: rotate(0deg) scale(1)
        12.5%
          transform: rotate(45deg) scale(1.3)
        25%
          transform: rotate(90deg) scale(1)
        37.5%
          transform: rotate(135deg) scale(1.3)
        50%
          transform: rotate(180deg) scale(1)
        62.5%
          transform: rotate(225deg) scale(1.3)
        75%
          transform: rotate(270deg) scale(1)
        87.5%
          transform: rotate(315deg) scale(1.3)
        100%
          transform: rotate(360deg) scale(1)

    &::before
      background-image: url(../assets/svg/covid-animation-2.svg)
      animation: rotate2 3s infinite
      animation-timing-function: linear

      @keyframes rotate2
        0%
          transform: rotate(0deg) scale(1)
        50%
          transform: rotate(-180deg) scale(1)
        100%
          transform: rotate(-360deg) scale(1)
</style>
