<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title d-flex justify-content-between">
      <span>{{ title }}</span>
    </div>
    <div class="card-number">
      <span>{{ lastValue }}</span>
      <div class="card-percentage-diff" :class="diffClass">
        {{ percentageDiff | prefixDiff }}%
      </div>
    </div>
    <div class="card-diff">
      <div class="trend-icon" :class="[diffClass, iconClass]"></div>
      <span :class="diffClass">{{ diff }}</span>
    </div>
    <div class="data-time">
      {{ $t('infocard.lastUpdated', { date: new Date(date) }) }}
    </div>
  </div>
</template>

<script>
import { mapGetters, mapState } from 'vuex'

export default {
  props: {
    title: String,
  },
  computed: {
    ...mapGetters('ostaniZdrav', ['data']),
    ...mapState('ostaniZdrav', ['loaded']),
    date() {
      return this.data[this.data.length - 2].date
    },
    lastValue() {
      const lastValue = this.data[this.data.length - 2]
      return lastValue.users_published
    },
    dayBeforeValue() {
      const dayBeforeValue = this.data[this.data.length - 3]
      return dayBeforeValue.users_published
    },
    diff() {
      const lastValue = this.data[this.data.length - 2].users_published
      const dayBeforeLastValue = this.data[this.data.length - 3].users_published
      return lastValue - dayBeforeLastValue
    },
    dayBeforeDiff() {
      const lastValue = this.data[this.data.length - 3].users_published
      const dayBeforeLastValue = this.data[this.data.length - 4].users_published
      return lastValue - dayBeforeLastValue
    },
    percentageDiff() {
      console.log(this.diff, this.dayBeforeValue)
      return Math.round((this.diff / this.dayBeforeValue) * 1000) / 10
    },
    iconClass() {
      let className = ''
      if (this.diff === 0) {
        className += ' none'
        return className
      } else if (this.diff > 0) {
        className += ' up'
      } else {
        className += ' down'
      }
      return className
    },
    diffClass() {
      if (this.diff === 0) {
        return 'no-change'
      } else {
        return this.diff > 0 ? 'good' : 'bad'
      }
    },
  },
}
</script>

<style></style>
