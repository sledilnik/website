<template>
  <b-container class="mt-3">
    <b-row>
      <div class="d-flex flex-wrap w-100 justify-content-center">
        <Info-card :title="title" :field="field" :good-trend="goodDirection" />
      </div>
    </b-row>
  </b-container>
</template>

<script>
import InfoCard from "components/cards/InfoCard";

export default {
  name: "StatsPage",
  components: {
    InfoCard
  },
  // TODO: could this code be deleted - because it's not used anywhere
  data() {
    return {
      title: null,
      field: null,
      goodDirection: null,
    }
  },
  created() {
    const cards = {
      'confirmedCases': {
        title: 'Potrjeno okuženi',
        field: 'cases.confirmed.todate',
        goodDirection: 'down',
      },
      'active': {
        title: 'Aktivni primeri',
        field: 'cases.active',
        goodDirection: 'up',
      },
      'hospitalized': {
        title: 'Hospitalizirani',
        field: 'state.in_hospital',
        goodDirection: 'down',
      },
      'icu': {
        title: 'V intenzivni enoti',
        field: 'state.icu',
        goodDirection: 'down',
      },
      'deceased': {
        title: 'Umrli',
        field: 'state.deceased.todate',
        goodDirection: 'down',
      },
    }

    let data = cards[this.$route.params.type]

    this.title = data.title
    this.field = data.field
    this.goodDirection = data.goodDirection
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass">
</style>
