<template>
  <div @click="checkClick($event)">
    <Time-stamp />
    <b-container class="stats-page">
      <b-row cols="12">
      </b-row>
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

import TimeStamp from 'components/TimeStamp'

import { Visualizations } from 'visualizations/App.fsproj'

export default {
  name: 'WorldStatsPage',
  components: {
    TimeStamp,
  },
  data() {
    return {
      loaded: false,
    }
  },
  mounted() {
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations('visualizations', 'world', this.$route.query)
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

<style lang="sass"></style>
