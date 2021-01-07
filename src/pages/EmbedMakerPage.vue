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
          text: this.$t('charts.metricsComparison.title', { context: process.env.VUE_APP_LOCALE_CONTEXT } ),
          dimensions: [1140, 720]
        },
        "DailyComparison": {
          value: "DailyComparison",
          text: this.$t('charts.dailyComparison.title', { context: process.env.VUE_APP_LOCALE_CONTEXT } ),
          dimensions: [1140, 720]
        },
        "EuropeMap": {
          value: "EuropeMap",
          text: this.$t('charts.europe.title'),
          dimensions: [1140, 780]
        },
        "WorldMap": {
          value: "WorldMap",
          text: this.$t('charts.world.title'),
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
        "CarePatients": {
          value: "Patients",
          text: this.$t('charts.carePatients.title'),
          dimensions: [1140, 720]
        },
//        "Ratios": {
//          value: "Ratios",
//          text: this.$t('charts.ratios.title'),
//          dimensions: [1140, 720]
//        },
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
        "Deceased": {
          value: "Deceased",
          text: this.$t('charts.deceased.title'),
          dimensions: [1140, 720]
        },
        "Sources": {
          value: "Sources",
          text: this.$t('charts.sources.title'),
          dimensions: [1140, 720]
        },
        "Regions": {
          value: "Regions",
          text: this.$t('charts.regions.title'),
          dimensions: [1140, 720]
        },
        "Regions100k": {
          value: "Regions100k",
          text: this.$t('charts.regions100k.title'),
          dimensions: [1140, 720]
        },
        "Map": {
          value: "Map",
          text: this.$t('charts.map.title'),
          dimensions: [1140, 820]
        },
        "RegionMap": {
          value: "RegionMap",
          text: this.$t('charts.rmap.title'),
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
        "WeeklyDemographics": {
          value: "WeeklyDemographics",
          text: this.$t('charts.weeklyDemographics.title'),
          dimensions: [1140, 720]
        },
        "PhaseDiagram": {
          value: "PhaseDiagram",
          text: this.$t('charts.phaseDiagram.title'),
          dimensions: [1140, 720]
        },
        "ExcessDeaths": {
          value: "ExcessDeaths",
          text: this.$t('charts.excessDeaths.title'),
          dimensions: [1140, 720]
        },
        "MetricsCorrelation": {
          value: "MetricsCorrelation",
          text: this.$t('charts.metricsCorrelation.title'),
          dimensions: [1140, 720]
        },
        "CountriesCasesPer100k": {
          value: "CountriesCasesPer100k",
          text: this.$t('charts.countriesNewCasesPer100k.title'),
          dimensions: [1140, 720]
        },
        "CountriesActiveCasesPer100k": {
          value: "CountriesActiveCasesPer100k",
          text: this.$t('charts.countriesActiveCasesPer100k.title'),
          dimensions: [1140, 720]
        },
        "CountriesNewDeathsPer100k": {
          value: "CountriesNewDeathsPer100k",
          text: this.$t('charts.countriesNewDeathsPer100k.title'),
          dimensions: [1140, 720]
        },
        "CountriesTotalDeathsPer100k": {
          value: "CountriesTotalDeathsPer100k",
          text: this.$t('charts.countriesTotalDeathsPer100k.title'),
          dimensions: [1140, 720]
        },
//        "Hospitals": {
//          value: "Hospitals",
//          text: 'Kapacitete bolnišnic',
//          dimensions: [1140, 1300]
//        },
        "OstaniZdravPublished": {
          value: "OstaniZdravPublished",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.published.title')}`,
          dimensions: [1140, 720]
        },
        "OstaniZdravUserCount": {
          value: "OstaniZdravUserCount",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.usercount.title')}`,
          dimensions: [1140, 720]
        },
        "OstaniZdravUserPublishedByCount": {
          value: "OstaniZdravUserPublishedByCount",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.userPublishedByCount.title')}`,
          dimensions: [1140, 720]
        },
        "OstaniZdravPublishedByRisk": {
          value: "OstaniZdravPublishedByRisk",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.publishedByRisk.title')}`,
          dimensions: [1140, 720]
        },
        "OstaniZdravValid": {
          value: "OstaniZdravValid",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.valid.title')}`,
          dimensions: [1140, 720]
        },
        "OstaniZdravValidByRisk": {
          value: "OstaniZdravValidByRisk",
          text: `${this.$t("charts.ostanizdrav.title")} - ${this.$t('charts.ostanizdrav.validByRisk.title')}`,
          dimensions: [1140, 720]
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
