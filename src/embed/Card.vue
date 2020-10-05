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
    this.field = data.field
    this.hospital = this.$route.params.hospital
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="sass"></style>
