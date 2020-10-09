<template>
  <div class="visualizations container">
    <section id="published">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#published">{{ $t('charts.ostanizdrav.published.title') }}</a></h2>
        </div>
      </div>
      <div id="publishedVis"></div>
    </section>
    <section id="usercount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#usercount">{{ $t('charts.ostanizdrav.usercount.title') }}</a></h2>
        </div>
      </div>
      <div id="usercountVis"></div>
    </section>
    <section id="userPublishedByCount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#userPublishedByCount">{{ $t('charts.ostanizdrav.userPublishedByCount.title') }}</a></h2>
        </div>
      </div>
      <div id="userPublishedByCountVis"></div>
    </section>
    <section id="validByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#validByRisk">{{ $t('charts.ostanizdrav.validByRisk.title') }}</a></h2>
        </div>
      </div>
      <div id="validByRiskVis"></div>
    </section>
    <section id="publishedByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#publishedByRisk">{{ $t('charts.ostanizdrav.publishedByRisk.title') }}</a></h2>
        </div>
      </div>
      <div id="publishedByRiskVis"></div>
    </section>
    <section id="valid">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#valid">{{ $t('charts.ostanizdrav.valid.title') }}</a></h2>
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
