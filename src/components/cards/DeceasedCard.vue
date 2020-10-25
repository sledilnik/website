<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title">{{ title }}</div>
    <div class="card-number">
      <span>{{ totalDeceased }} / {{ runningSum(7) }}</span>
    </div>
    <div class="card-diff">
      <div class="card-note">total / in the last 7 days</div>
    </div>
    <div class="data-time">
      {{
        $t('infocard.lastUpdated', {
          date: new Date(exportTime),
        })
      }}
    </div>
  </div>
  <div class="hp-card" v-else>
    <div class="card-title">{{ title }}</div>
    <font-awesome-icon icon="spinner" spin />
  </div>
</template>
<script>
import { mapGetters, mapState } from 'vuex'

export default {
  props: {
    title: String,
  },
  computed: {
    ...mapGetters('patients', ['data', 'runningSum']),
    ...mapState('patients', ['exportTime', 'loaded']),
    totalDeceased() {
      return this.data[this.data.length - 1].total.deceased.toDate
    },
  },
}
</script>

<style scoped lang="scss"></style>
