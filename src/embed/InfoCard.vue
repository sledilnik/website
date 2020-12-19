<template>
  <b-container class="mt-3">
    <b-row>
      <div class="d-flex flex-wrap w-100 justify-content-center">
        <Info-card
          :cardName="card.cardName"
          :cardData="summary[card.cardName]"
          :loading="!card || !summary || !summary[card.cardName] || !summary[card.cardName].value"
        />
      </div>
    </b-row>
  </b-container>
</template>

<script>
import InfoCard from 'components/cards/InfoCard'
import { mapState } from 'vuex'
const cards = {
  confirmedCases: {
    cardName: 'casesToDateSummary',
    title: 'Potrjeni primeri',
    field: 'cases.confirmed.todate',
  },
  active: {
    cardName: 'casesActive',
    title: 'Aktivni primeri',
    field: 'cases.active',
    fieldNewCases: 'cases.confirmedToday',
    fieldDeceased: 'statePerTreatment.deceased',
  },
  hospitalized: {
    cardName: 'hospitalizedCurrent',
    title: 'Hospitalizirani',
    field: 'statePerTreatment.inHospital',
    totalIn: 'total.inHospital.in',
    totalOut: 'total.inHospital.out',
    totalDeceased: 'total.deceased.hospital.today',
  },
  icu: {
    cardName: 'icuCurrent',
    title: 'V intenzivni enoti',
    field: 'statePerTreatment.inICU',
    totalIn: 'total.icu.in',
    totalOut: 'total.icu.out',
    totalDeceased: 'total.deceased.hospital.icu.today',
  },
  deceased: {
    cardName: 'deceasedToDate',
    title: 'Umrli',
    field: 'statePerTreatment.deceasedToDate',
  },
}

export default {
  name: 'InfoCardEmbed',
  components: {
    InfoCard,
  },
  data() {
    return {
      card: null,
    }
  },
  computed: {
    ...mapState("stats", {
      exportTime: "exportTime",
      summary: "summary",
    }),
  },
  created() {
    this.$store.dispatch('stats/fetchSummary')
  },
  mounted(){
    const cardType = this.$route.params.type
    this.card = cards[cardType]
  }
}
</script>
