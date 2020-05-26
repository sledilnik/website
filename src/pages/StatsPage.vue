<template>
  <div>
    <Time-stamp />
    <b-container class="stats-page">
      <b-row cols="12">
        <b-col>
          <Notice />
        </b-col>
      </b-row>
      <div class="cards-wrapper latest-data-boxes">
        <Info-card :title="$t('infocard.confirmedToDate')" field="cases.confirmedToDate" series-type="state" />
        <Info-card :title="$t('infocard.recoveredToDate')" field="cases.recoveredToDate" good-trend="up" series-type="state" />
        <Info-card :title="$t('infocard.inHospital')" field="statePerTreatment.inHospital" totalIn="total.inHospital.in" totalOut="total.inHospital.out" series-type="state" />
        <Info-card :title="$t('infocard.icu')" field="statePerTreatment.inICU" totalIn="total.icu.in" totalOut="total.icu.out" series-type="state" />
        <Info-card :title="$t('infocard.deceasedToDate')" field="statePerTreatment.deceasedToDate" series-type="state" />
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
import Notice from 'components/Notice';

import { Visualizations } from 'visualizations/App.fsproj';

export default {
  name: 'StatsPage',
  components: {
    InfoCard,
    TimeStamp,
    Notice,
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
      statsLoaded: 'loaded',
    }),
    ...mapState('patients', {
      patientsLoaded: 'loaded',
    }),
  },
  mounted() {    
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations('visualizations', this.$route.query);
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
