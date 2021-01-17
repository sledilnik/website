<template>
  <b-container class="mt-3">
    <b-row>
      <div class="d-flex flex-wrap w-100 justify-content-center">
        <Card
          :title="title"
          :field="field"
          :fields="fields"
          :hospital="hospital"
        />
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
      fields: null,
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
        title: 'V intenzivni terapiji',
        field: 'icu',
      },
      facility: {
        title: '',
        fields: [
          {
            title: 'Hospitalizirani',
            category: 'inHospital',
          },
          {
            title: 'V intenzivni terapiji',
            category: 'icu',
          },
          // {
          //   title: 'V kritičnem stanju',
          //   category: 'critical',
          // },
          // {
          //   title: 'Umrli',
          //   category: 'deceased',
          // },
        ],
      },
    }

    let data = cards[this.$route.params.type]

    this.title = data.title
    this.setHospital(this.$route.params.hospital)

    this.field = data.field
    this.fields = data.fields

    this.hospital = this.$route.params.hospital
  },
  methods: {
    setHospital(hospital) {
      if (this.title !== '') {
        this.title = this.title.concat(' - ')
      }
      switch (hospital) {
        case 'bto':
          this.title = this.title.concat('B Topolšica')
          break
        case 'bse':
          this.title = this.title.concat('B Sežana')
          break
        case 'sbce':
          this.title = this.title.concat('SB Celje')
          break
        case 'sbms':
          this.title = this.title.concat('SB Murska Sobota')
          break
        case 'sbnm':
          this.title = this.title.concat('SB Novo mesto')
          break
        case 'sbsg':
          this.title = this.title.concat('SB Slovenj Gradec')
          break
        case 'sbje':
          this.title = this.title.concat('SB Jesenice')
          break
        case 'sbpt':
          this.title = this.title.concat('SB Ptuj')
          break
        case 'sbbr':
          this.title = this.title.concat('SB Brežice')
          break
        case 'sbng':
          this.title = this.title.concat('SB Nova Gorica')
          break
        case 'sbiz':
          this.title = this.title.concat('SB Izola')
          break
        case 'sbtr':
          this.title = this.title.concat('SB Trbovlje')
          break
        case 'ukclj':
          this.title = this.title.concat('UKC Ljubljana')
          break
        case 'ukcmb':
          this.title = this.title.concat('UKC Maribor')
          break
        case 'ukg':
          this.title = this.title.concat('UK Golnik')
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
