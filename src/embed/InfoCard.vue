<template>
  <b-container class="mt-3">
    <b-row>
      <div class="d-flex flex-wrap w-100 justify-content-center">
        <Info-card
          :title="title"
          :field="field"
          :field-new-cases="fieldNewCases"
          :field-deceased="fieldDeceased"
          :total-in="totalIn"
          :total-out="totalOut"
          :total-deceased="totalDeceased"
        />
      </div>
    </b-row>
  </b-container>
</template>

<script>
import InfoCard from 'components/cards/InfoCard'

export default {
  name: 'InfoCardEmbed',
  components: {
    InfoCard,
  },
  data() {
    return {
      title: null,
      field: null,
      fieldNewCases: null,
      fieldDeceased: null,
      totalIn: null,
      totalOut: null,
      totalDeceased: null,
      hospital: null,
    }
  },
  created() {
    const cards = {
      confirmedCases: {
        title: 'Potrjeni primeri',
        field: 'cases.confirmed.todate',
      },
      active: {
        title: 'Aktivni primeri',
        field: 'cases.active',
        fieldNewCases: 'cases.confirmedToday',
        fieldDeceased: 'statePerTreatment.deceased',
      },
      hospitalized: {
        title: 'Hospitalizirani',
        field: 'statePerTreatment.inHospital',
        totalIn: 'total.inHospital.in',
        totalOut: 'total.inHospital.out',
        totalDeceased: 'total.deceased.hospital.today',
      },
      icu: {
        title: 'V intenzivni enoti',
        field: 'statePerTreatment.inICU',
        totalIn: 'total.icu.in',
        totalOut: 'total.icu.out',
        totalDeceased: 'total.deceased.hospital.icu.today',
      },
      deceased: {
        title: 'Umrli',
        field: 'statePerTreatment.deceasedToDate',
      },
    }

    let data = cards[this.$route.params.type]

    this.title = data.title
    this.field = data.field
    this.goodDirection = data.goodDirection
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass"></style>
