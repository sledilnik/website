<template>
  <b-container class="mt-3">
    <b-row>
      <div class="d-flex flex-wrap w-100 justify-content-center">
        <Card :title="title" :field="field" :hospital="hospital" />
      </div>
    </b-row>
  </b-container>
</template>

<script>
import Card from 'components/cards/Card'

export default {
  name: 'CardEmbed',
  components: {
    Card,
  },
  data() {
    return {
      title: '',
      field: null,
      hospital: null,
    }
  },
  created() {
    const cards = {
      hospitalized: {
        title: 'Hospitalizirani',
        field: 'inHospital',
      },
      icu: {
        title: 'V intenzivni enoti',
        field: 'icu',
      },
    }

    let data = cards[this.$route.params.type]

    this.title = data.title
    this.setHospital(this.$route.params.hospital)

    this.field = data.field
    this.hospital = this.$route.params.hospital
  },
  methods: {
    setHospital(hospital) {
      switch (hospital) {
        case 'sbce':
          this.title = this.title.concat(' v SB Celje')
          break
        case 'sbje':
          this.title = this.title.concat(' v SB Jesenice')
          break
        case 'sbms':
          this.title = this.title.concat(' v SB Murska Sobota')
          break
        case 'sbnm':
          this.title = this.title.concat(' v SB Novo mesto')
          break
        case 'ukclj':
          this.title = this.title.concat(' v UKC Ljubljana')
          break
        case 'ukcmb':
          this.title = this.title.concat(' v UKC Maribor')
          break
        case 'ukg':
          this.title = this.title.concat(' v UKC Golnik')
          break
        default:
          this.title = 'Neznana bolnišnica'
          break
      }
    },
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass"></style>
