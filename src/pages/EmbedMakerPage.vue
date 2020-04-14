<template>
  <div class="container">
    <div class="d-flex embed-controls">
      <div class="p-3">
        <h2>Izberite graf</h2>
        <b-form-select v-model="chosenChartValue" :options="charts"></b-form-select>
      </div>
      <div class="p-3">
        <h2 v-if="chosenChart">Koda za vdelavo:</h2>
        <b-form-textarea @click="copy" v-if="chosenChart" v-b-tooltip.bottom.hover :title="tooltipTitle" class="copy-input" id="textarea-plaintext" readonly :value="embedString"></b-form-textarea>
      </div>
    </div>
    <hr>
    <div v-if="chosenChart" class="d-flex flex-column">
      <h2 class="p-3">Predogled:</h2>
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
      tooltipTitle: "Kopiraj",
      baseEmbedUrl: "https://covid-19.sledilnik.org/embed.html#",
      charts: {
        "empty": {
          value: null, 
          text: 'Izberite graf'
        },
        "MetricsComparison": {
          value: "MetricsComparison",
          text: 'Širjenje COVID-19 v Sloveniji',
          dimensions: [1140, 780]
        },
        "Patients": {
          value: "Patients",
          text: 'Obravnava hospitaliziranih',
          dimensions: [1140, 680]
        },
        "Spread": {
          value: "Spread",
          text: 'Prirast potrjeno okuženih',
          dimensions: [1140, 630]
        },
        "Regions": {
          value: "Regions",
          text: 'Potrjeno okuženi po regijah',
          dimensions: [1140, 720]
        },
        "Municipalities": {
          value: "Municipalities",
          text: 'Potrjeno okuženi po občinah',
          dimensions: [1140, 1150]
        },
        "AgeGroups": {
          value: "AgeGroups",
          text: 'Potrjeno okuženi po starostnih skupinah',
          dimensions: [1140, 650]
        },
        "Hospitals": {
          value: "Hospitals",
          text: 'Kapacitete bolnišnic',
          dimensions: [1140, 1300]
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
      this.tooltipTitle = "Skopirano!"
      setTimeout(() => {
        this.tooltipTitle = "Kopiraj"
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