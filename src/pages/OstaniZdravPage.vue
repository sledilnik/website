<template>
  <div class="visualizations container">
    <section id="published">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#published">{{ $t('charts.ostanizdrav.published.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-published`">?</a></div>
          </div>
        </div>
      </div>
      <div id="publishedVis"></div>
    </section>
    <section id="usercount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#usercount">{{ $t('charts.ostanizdrav.usercount.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-usercount`">?</a></div>
          </div>
        </div>
      </div>
      <div id="usercountVis"></div>
    </section>
    <section id="userPublishedByCount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#userPublishedByCount">{{ $t('charts.ostanizdrav.userPublishedByCount.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-userPublishedByCount`">?</a></div>
          </div>
        </div>
      </div>
      <div id="userPublishedByCountVis"></div>
    </section>
    <section id="publishedByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#publishedByRisk">{{ $t('charts.ostanizdrav.publishedByRisk.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-publishedByRisk`">?</a></div>
          </div>
        </div>
      </div>
      <div id="publishedByRiskVis"></div>
    </section>
    <section id="validByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#validByRisk">{{ $t('charts.ostanizdrav.validByRisk.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-validByRisk`">?</a></div>
          </div>
        </div>
      </div>
      <div id="validByRiskVis"></div>
    </section>
    <section id="valid">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2><a href="#valid">{{ $t('charts.ostanizdrav.valid.title') }}</a></h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div><a class="faq-link" target="_blank" :href="`/${$i18n.i18next.language}/faq#ostanizdrav-valid`">?</a></div>
          </div>
        </div>
      </div>
      <div id="validVis"></div>
    </section>
  </div>
</template>

<script>
import embed from 'vega-embed'

import validPlot from '@/vega/valid'
import validByRiskPlot from '@/vega/validByRisk'
import userPublishedByCountPlot from '@/vega/userPublishedByCount'
import usercountPlot from '@/vega/usercount'
import publishedByRiskPlot from '@/vega/publishedByRisk'
import publishedPlot from '@/vega/published'

import i18n from '@/i18n'

export default {
  name: 'OstaniZdravPage',
  async mounted() {

    let $t = (id) => i18n.i18next.t(id)

    embed('#validVis', validPlot($t), { actions: false })
    embed('#publishedVis', publishedPlot($t), { actions: false })
    embed('#validByRiskVis', validByRiskPlot($t), { actions: false })
    embed('#publishedByRiskVis', publishedByRiskPlot($t), { actions: false })
    embed('#usercountVis', usercountPlot($t), { actions: false })
    embed('#userPublishedByCountVis', userPublishedByCountPlot($t), {
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
