<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title">{{ title }}</div>
    <div v-if="!fields">
      <PatientsCardData :category="field" :patients="patients" :hospital="hospital" />
    </div>
    <div class="d-flex flex-wrap" v-else>
      <PatientsCardData
        v-for="field in fields"
        :key="field.category"
        :field="field"
        :patients="patients"
        :hospital="hospital"
      />
    </div>
    <div class="footer d-flex justify-content-between">
      <div class="data-time">
        {{
          $t('infocard.lastUpdated', {
            date: new Date(exportTime),
          })
        }}
      </div>
      <a
        class="brand-link"
        target="_blank"
        href="https://covid-19.sledilnik.org/"
        >COVID-19 Sledilnik</a
      >
    </div>
  </div>
  <div class="hp-card" v-else>
    <div class="card-title">{{ title }}</div>
    <font-awesome-icon icon="spinner" spin />
  </div>
</template>
<script>
import PatientsCardData from 'components/cards/PatientsCardData'
import { mapGetters, mapState } from 'vuex'

export default {
  components: {
    PatientsCardData,
  },
  props: {
    title: String,
    field: String,
    fields: Array,
    hospital: String,
  },
  computed: {
    ...mapGetters('patients', { patients: 'data' }),
    ...mapState('patients', ['exportTime', 'loaded']),
  },
}
</script>

<style scoped lang="scss">
.footer {	
  margin-top: auto;	
}
</style>
