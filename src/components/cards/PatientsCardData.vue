<template>
  <div class="card-content d-flex flex-column align-items-center">
    <div v-if="typeof field !== 'undefined'" class="card-title">
      {{ field.title }}
    </div>
    <div class="card-number">
      <span>{{ renderValue(today) }}</span>
      <div class="card-percentage-diff" :class="diffClass">
        {{ diff(today, yesterday) | prefixDiff }}%
      </div>
    </div>
    <div class="card-diff">
      <div v-if="renderIn(today) > 0" class="card-diff-item">
        <div class="trend-icon bad up"></div>
        <span class="in bad">{{ renderIn(today) }}</span>
      </div>
      <div v-if="renderOut(today) > 0" class="card-diff-item">
        <div class="trend-icon good down"></div>
        <span class="out good">{{ renderOut(today) }}</span>
      </div>
      <div v-if="renderDeceased(today) > 0 && percentageDiff !== 0" class="card-diff-item">
        <div class="trend-icon deceased"></div>
        <span class="deceased">{{ renderDeceased(today) }}</span>
      </div>
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from 'vuex'

export default {
  props: {
    category: String,
    field: Object,
    hospital: String,
    patients: Array,
  },
  data() {
    return {
      percentageDiff: 0,
    }
  },
  computed: {
    today() {
      const item = this.patients[this.patients.length - 1]
      return item
    },
    yesterday() {
      const item = this.patients[this.patients.length - 2]
      return item
    },
    diffClass() {
      return this.percentageDiff === 0
        ? 'no-change'
        : this.percentageDiff > 0
        ? 'bad'
        : 'good'
    },
  },
  methods: {
    renderValue(item) {
      const field = this.category ? this.category : this.field.category
      const value = item.facilities[this.hospital][field].today
      if (!value) return 0
      return value
    },
    diff(today, yesterday) {
      const field = this.category ? this.category : this.field.category
      const todayValue = this.renderValue(today, field)
      const yesterdayValue = this.renderValue(yesterday, field)
      const delta = todayValue - yesterdayValue

      const percentageDiff =
        yesterdayValue === 0
          ? Math.round((delta - yesterdayValue) * 1000) / 10
          : Math.round((delta / yesterdayValue) * 1000) / 10

      this.percentageDiff = percentageDiff
      return percentageDiff
    },
    renderIn(item) {
      const field = this.category ? this.category : this.field.category
      const value = item.facilities[this.hospital][field].in
      if (!value) return 0
      return value
    },
    renderOut(item) {
      const field = this.category ? this.category : this.field.category
      const value = item.facilities[this.hospital][field].out
      if (!value) return 0
      return value
    },
    renderDeceased(item) {
      const value = item.facilities[this.hospital].deceased.today
      if (!value) return 0
      return value
    },
  },
}
</script>

<style scoped lang="scss">
// .card-content {
//   flex: 1 1 0;
// }
.card-content:not(:last-child) {
  margin-right: 30px;
}
</style>
