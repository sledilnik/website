<template>
  <div class="visualizations container">
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.published.title') }}</h2>
        </div>
      </div>
      <div id="publishedVis"></div>
    </section>
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.usercount.title') }}</h2>
        </div>
      </div>
      <div id="usercountVis"></div>
    </section>
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.userPublishedByCount.title') }}</h2>
        </div>
      </div>
      <div id="userPublishedByCountVis"></div>
    </section>
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.validByRisk.title') }}</h2>
        </div>
      </div>
      <div id="validByRiskVis"></div>
    </section>
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.publishedByRisk.title') }}</h2>
        </div>
      </div>
      <div id="publishedByRiskVis"></div>
    </section>
    <section>
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>{{ $t('charts.ostanizdrav.valid.title') }}</h2>
        </div>
      </div>
      <div id="validVis"></div>
    </section>
  </div>
</template>

<script>
import embed from 'vega-embed'

import validPlot from '@/vega/valid.json'
import validByRiskPlot from '@/vega/validByRisk.json'
import userPublishedByCountPlot from '@/vega/userPublishedByCount.json'
import usercountPlot from '@/vega/usercount.json'
import publishedByRiskPlot from '@/vega/publishedByRisk.json'
import publishedPlot from '@/vega/published.json'

export default {
  name: 'OstaniZdravPage',
  mounted() {
    embed('#validVis', validPlot, { actions: false })
    embed('#publishedVis', publishedPlot, { actions: false })
    embed('#validByRiskVis', validByRiskPlot, { actions: false })
    embed('#publishedByRiskVis', publishedByRiskPlot, { actions: false })
    embed('#usercountVis', usercountPlot, { actions: false })
    embed('#userPublishedByCountVis', userPublishedByCountPlot, {
      actions: false,
    })
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss" scoped>
.visualizations {
  display: flex;
  flex-direction: column;
  justify-content: center;
  margin: 0 auto !important;
  max-width: 930px;

  section {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
}

.vega-embed {
  display: flex;
  justify-content: center;
}
</style>
