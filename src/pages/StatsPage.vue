<template>
  <div @click="checkClick($event)">
    <Time-stamp />
    <b-container class="stats-page">
      <b-row cols="12">
        <b-col>
          <Notice />
        </b-col>
      </b-row>
      <div class="cards-wrapper">
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
<!--  
        <Info-card
          :title="$t('infocard.recoveredToDate')"
          field="cases.recoveredToDate"
          good-trend="up"
          series-type="state"
        />
-->
        <Info-card
          :title="$t('infocard.active')"
          field="cases.active"
          field-new-cases="cases.confirmedToday"
          field-deceased="statePerTreatment.deceased"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.inHospital')"
          field="statePerTreatment.inHospital"
          total-in="total.inHospital.in"
          total-out="total.inHospital.out"
          total-deceased="total.deceased.hospital.today"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.icu')"
          field="statePerTreatment.inICU"
          total-in="total.icu.in"
          total-out="total.icu.out"
          total-deceased="total.deceased.hospital.icu.today"
          series-type="state"
        />
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
      Visualizations('visualizations', 'local', this.$route.query)
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
      if (e.target.classList.contains('share-button-')) return

      // else check if any of the dropdowns is opened and close it/them
      dropdownAll.forEach((el) => {
        el.classList.contains('show')
          ? el.classList.remove('show')
          : el.classList.add('hide')
      })

      // TODO: there is still an issue where if you immediately click on the same
      // share button again, it won't open the dropdown because ShareButton.fs
      // component is not aware that the dropdown was closed within this method.
      // I think the right way to do this would be to listen for clicks within App.fs
    },
  },
}
</script>

<style lang="sass">
.cards-wrapper
  display: flex
  flex-wrap: wrap
  margin: 0px -15px 58px

  @media only screen and (min-width: 768px)
    margin: 0px auto 58px

$loader-width: 50px

.stats-page
  margin-top: 48px
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
      z-index: 90
      display: block
      position: absolute
      top: calc(50% - 25px)
      left: calc(50% - 25px)
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
