<template>
  <div @click="checkClick($event)">
    <Time-stamp />
    <b-container class="stats-page">
      <b-row cols="12">
        <b-col>
          <Notice />
        </b-col>
      </b-row>
      <div class="cards-wrapper latest-data-boxes">
<!--  
        <Info-card
          :title="$t('infocard.tests')"
          field="tests.performed.today"
          good-trend="up"
          series-type="state"
        />
-->
        <Info-card
          :title="$t('infocard.confirmedToDate')"
          field="cases.confirmedToDate"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.recoveredToDate')"
          field="cases.recoveredToDate"
          good-trend="up"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.active')"
          field="cases.active"
          good-trend="down"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.inHospital')"
          field="statePerTreatment.inHospital"
          totalIn="total.inHospital.in"
          totalOut="total.inHospital.out"
          totalDeceased="total.deceased.hospital.today"
          series-type="state"
        />
<!--  
        <Info-card
          :title="$t('infocard.icu')"
          field="statePerTreatment.inICU"
          totalIn="total.icu.in"
          totalOut="total.icu.out"
          totalDeceased="total.deceased.hospital.icu.today"
          series-type="state"
        />
-->
        <Info-card
          :title="$t('infocard.deceasedToDate')"
          field="statePerTreatment.deceasedToDate"
          series-type="state"
        />
      </div>
      <b-row cols="12">
        <b-col>
          <div id="visualizations" class="visualizations"></div>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import { mapState } from 'vuex'

import InfoCard from 'components/cards/InfoCard'
import TimeStamp from 'components/TimeStamp'
import Notice from 'components/Notice'

import { Visualizations } from 'visualizations/App.fsproj'

export default {
  name: 'StatsPage',
  components: {
    InfoCard,
    TimeStamp,
    Notice,
  },
  data() {
    return {
      loaded: false,
    }
  },
  mounted() {
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations('visualizations', this.$route.query)
    })

    // stupid spinner impl, but i do not know better (charts are react component, no clue when they are rendered)
    let checker = setInterval(() => {
      let elm = document.querySelector('.highcharts-point')
      if (elm) {
        document.querySelector('.stats-page').classList.add('loaded')
        this.loaded = true
        clearInterval(checker)
      }
    }, 80)
  },
  methods: {
    checkClick(e) {
      const dropdownAll = this.$el.querySelectorAll('.share-dropdown-wrapper')

      // ignore click if the clicked element is share button or its icon or caption
      if (
        e.target.classList.contains('share-button-wrapper') ||
        e.target.classList.contains('share-button-icon') ||
        e.target.classList.contains('share-button-caption')
      ) {
        return
      }

      // else check if any of the dropdowns is opened and close it/them
      dropdownAll.forEach((el) => {
        if (el.classList.contains('show')) {
          el.classList.remove('show')
          el.classList.add('hide')
        }
      })

      // TODO: there is still an issue where if you immediatelly click on the same
      // share button again, it won't open the dropdown because ShareButton.fs
      // component is not aware that the dropdown was closed within this method.
      // I think the right way to do this would be to listen for clicks within App.fs
    },
  },
}
</script>

<style lang="sass"></style>
