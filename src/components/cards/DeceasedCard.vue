<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title">{{ title }}</div>
    <div class="card-number">
      <span>{{ runningSum(0, 7) }}</span>
      <div class="card-percentage-diff" :class="diffClass">
        {{ diff | prefixDiff }}%
      </div>
    </div>
    <div class="card-diff">
      <div class="card-note">{{ $t('infocard.total')}} {{ totalDeceased }}</div>
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
    field: String
  },
  computed: {
    ...mapGetters('patients', ['data', 'runningSum']),
    ...mapState('patients', ['exportTime', 'loaded']),
    totalDeceased() {
      return this.data[this.data.length - 1].total.deceased.toDate
    },
    diff() {
      const thisWeek = this.runningSum(0, 7)
      const previousWeek = this.runningSum(7, 14)
      const delta = thisWeek - previousWeek

      const percentageDiff =
        previousWeek === 0
          ? Math.round((delta - previousWeek) * 1000) / 10
          : Math.round((delta / previousWeek) * 1000) / 10

      return percentageDiff
    },
    diffClass() {
      return this.diff === 0 ? 'no-change' : this.diff > 0 ? 'bad' : 'good'
    },
  },
}
</script>

<style scoped lang="scss"></style>
