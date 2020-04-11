<template>
  <div>
    <Time-stamp />
    <b-container class="stats-page">
      <div class="time-stamp"></div>
      <div class="cards-wrapper latest-data-boxes">
        <Info-card title="Potrjeno okuženi" field="tests.positive.todate" series-type="cum" />
        <Info-card title="Hospitalizirani" field="state.in_hospital" series-type="state" />
        <Info-card title="V intenzivni enoti" field="state.icu" series-type="state" />
        <Info-card title="Umrli" field="state.deceased.todate" series-type="cum" />
        <Info-card title="Ozdraveli" field="state.recovered.todate" good-trend="up" series-type="cum" />
      </div>
      <b-row cols="12">
        <b-col>
          <div id="visualizations" class="visualizations"></div>
        </b-col>
      </b-row>
      <!-- <loader v-show="!loaded"></loader> -->
    </b-container>
  </div>
</template>

<script>
import { mapState } from 'vuex';

// import Loader from 'components/Loader';
import InfoCard from 'components/cards/InfoCard';
import TimeStamp from 'components/TimeStamp';

import { Visualizations } from 'visualizations/App.fsproj';

export default {
  name: 'StatsPage',
  components: {
    InfoCard,
    TimeStamp,
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

<style lang="sass"></style>
