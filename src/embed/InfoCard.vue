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
  },
  active: {
    cardName: 'casesActive',
    title: 'Aktivni primeri',
  },
  hospitalized: {
    cardName: 'hospitalizedCurrent',
    title: 'Hospitalizirani',
  },
  icu: {
    cardName: 'icuCurrent',
    title: 'V intenzivni enoti',
  },
  deceased: {
    cardName: 'deceasedToDate',
    title: 'Umrli',
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
