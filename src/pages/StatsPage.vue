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
    <loader v-show="!loaded"></loader>
  </b-container>
</template>

<script>
import { mapState } from 'vuex';

import Loader from 'components/Loader';
import InfoCard from 'components/cards/InfoCard';

import { Visualizations } from 'visualizations/App.fsproj';

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
      let elm = document.querySelector('.chart-display-property-selector');
      if (elm) {
        this.loaded = true;
        clearInterval(checker);
      }
    }, 80);
  },
};
</script>

<style scoped lang="sass">
.cards-wrapper
  display: flex
  max-width: 1140px
  flex-wrap: wrap
  margin: 0px auto 58px

.stats-page
  position: relative
</style>
