<template>
  <div class="hp-card" v-if="loaded">
    <div class="d-flex justify-content-between">
      <div class="card-title">{{ title }}</div>
    </div>
    <div class="card-number">
      <span>{{ runningSum(0, 7, field) }}</span>
      <div class="card-percentage-diff" :class="diffClass">
        {{ diff | prefixDiff }}%
      </div>
    </div>
    <div class="card-diff">
      <!-- <div class="card-diff-item">
        <div class="trend-icon deceased"></div>
        <span class="deceased">{{ total }} </span>
      </div> -->
    </div>
    <div class="d-flex flex-column flex-sm-row justify-content-between">
      <div class="card-data order-2 order-sm-1">
        <div class="data-time">
          {{
            $t('infocard.lastUpdated', {
              date: new Date(exportTime),
            })
          }}
        </div>
      </div>
      <trend
        :data="lastData(0, 14, field)"
        :gradient="['#ffbe88', '#ffbe88', '#ffbe88']"
        :stroke-width="4"
        auto-draw
        smooth
        class="sparkline order-1 order-sm-2 mb-3 mb-sm-0"
      >
      </trend>
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
    field: String,
  },
  computed: {
    ...mapGetters('patients', ['data', 'runningSum', 'lastData']),
    ...mapState('patients', ['exportTime', 'loaded']),
    total() {
      return this.data[this.data.length - 1].total.deceased.toDate
    },
    diff() {
      const thisWeek = this.runningSum(0, 7, this.field)
      const previousWeek = this.runningSum(7, 14, this.field)
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

<style scoped lang="scss">
.card-data {
  flex: 0 0 66%;
}
.sparkline {
  height: 100%;
}
</style>
