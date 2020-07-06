<template>
  <div class="container">
    <div class="d-flex embed-controls">
      <div class="p-3">
        <h1>{{ $t("embedMaker.title") }}</h1>
        <b-form-select v-model="chosenChartValue" :options="charts"></b-form-select>
        <span v-html="$t('embedMaker.description')"/>
      </div>
      <div class="p-3">
        <h3 v-if="chosenChart">{{$t('embedMaker.embedCode')}}</h3>
        <b-form-textarea @click="copy" v-if="chosenChart" v-b-tooltip.bottom.hover :title="tooltipTitle" class="copy-input" id="textarea-plaintext" readonly :value="embedString"></b-form-textarea>
      </div>
    </div>
    <hr>
    <div v-if="chosenChart" class="d-flex flex-column">
      <h3 class="p-3">{{$t('embedMaker.preview')}}</h3>
      <iframe
        :src="embedUrl"
        frameborder="0"
        :width="chosenChart ? chosenChart.dimensions[0] : 1140"
        :height="chosenChart ? chosenChart.dimensions[1] : 680"
      ></iframe>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      chosenChartValue: null,
      height: 750,
      width: 1100,
      tooltipTitle: this.$t('embedMaker.copy'),
      baseEmbedUrl: location.origin + "/embed.html#/" + this.$i18n.i18next.language,
      charts: {
        "empty": {
          value: null,
          text: this.$t("embedMaker.chooseChart")
        },
        "MetricsComparison": {
          value: "MetricsComparison",
          text: this.$t('charts.metricsComparison.title', { context: process.env.VUE_APP_LOCALE_CONTEXT } ), // TODO: add this context by default to all calls
          dimensions: [1140, 780]
        },
        "EuropeMap": {
          value: "EuropeMap",
          text: this.$t('charts.europe.title'),
          dimensions: [1140, 780]
        },
        "Cases": {
          value: "Cases",
          text: this.$t('charts.cases.title'),
          dimensions: [1140, 630]
        },
        "Patients": {
          value: "Patients",
          text: this.$t('charts.patients.title'),
          dimensions: [1140, 720]
        },
        "Ratios": {
          value: "Ratios",
          text: this.$t('charts.ratios.title'),
          dimensions: [1140, 720]
        },
        "HCenters": {
          value: "HCenters",
          text: this.$t('charts.hCenters.title'),
          dimensions: [1140, 720]
        },
        "Tests": {
          value: "Tests",
          text: this.$t('charts.tests.title'),
          dimensions: [1140, 720]
        },
        "Infections": {
          value: "Infections",
          text: this.$t('charts.infections.title'),
          dimensions: [1140, 720]
        },
        "Spread": {
          value: "Spread",
          text: this.$t('charts.spread.title'),
          dimensions: [1140, 630]
        },
        "Regions": {
          value: "Regions",
          text: this.$t('charts.regions.title'),
          dimensions: [1140, 720]
        },
        "Map": {
          value: "Map",
          text: this.$t('charts.map.title'),
          dimensions: [1140, 820]
        },
        "Municipalities": {
          value: "Municipalities",
          text: this.$t('charts.municipalities.title'),
          dimensions: [1140, 1150]
        },
        "AgeGroups": {
          value: "AgeGroups",
          text: this.$t('charts.ageGroups.title'),
          dimensions: [1140, 720]
        },
        "AgeGroupsTimeline": {
          value: "AgeGroupsTimeline",
          text: this.$t('charts.ageGroupsTimeline.title'),
          dimensions: [1140, 720]
        },
//        "Hospitals": {
//          value: "Hospitals",
//          text: 'Kapacitete bolnišnic',
//          dimensions: [1140, 1300]
//        },
        "Countries": {
          value: "Countries",
          text: this.$t('charts.countries.title'),
          dimensions: [1140, 740]
        },
      }
    };
  },
  computed: {
    embedUrl() {
      return `${this.baseEmbedUrl}/chart/${this.chosenChartValue}`;
    },
    chosenChart(){
      return this.charts[this.chosenChartValue]
    },
    embedString(){
      return `<iframe src="${this.embedUrl}" frameborder="0" width="${this.chosenChart.dimensions[0]}" height="${this.chosenChart.dimensions[1]}"></iframe>`
    }
  },
  methods: {
    copy($event){
      const element = $event.target
      element.select();
      element.setSelectionRange(0, 99999); /*For mobile devices*/
      /* Copy the text inside the text field */
      document.execCommand("copy");
      this.tooltipTitle = this.$t('embedMaker.copied')
      setTimeout(() => {
        this.tooltipTitle = this.$t('embedMaker.copy')
      }, 2000)
    }
  }
};
</script>

<style lang="scss">
.embed-controls{
  &>*{
    width: auto;
    flex: 1
  }
}
.copy-input{
  min-height: 90px;
  cursor: pointer;
}

</style>
