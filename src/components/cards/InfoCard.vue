<template>
  <b-card :title="title" :sub-title="subtitle" tag="article" >
    <b-card-text>{{ value }}</b-card-text>
  </b-card>
</template>
<script>
import moment from 'moment'

export default {

  props: ["title", "text", "value", "valueDate"],
  computed: {
    subtitle() {
      // console.log("moment", moment(new Date()))
      let dateFormatted = moment(this.valueDate).calendar()
      return `Podatki do ${dateFormatted}`
    }
  }
};
</script>

<style>
</style>