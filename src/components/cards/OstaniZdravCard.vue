<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title d-flex justify-content-between">
      <span>{{ title }}</span>
    </div>
    <div class="card-number">
      <span>{{ lastValue }}</span>
      <div class="card-percentage-diff no-change">
        {{ percentageDiff | prefixDiff }}%
      </div>
    </div>
    <div class="card-diff">
      <span class="card-note">Danes že: {{ current }}</span>
    </div>
    <div class="data-time">
      {{ $t('infocard.lastUpdated', { date: new Date(lastValueDate) }) }}
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
    ...mapGetters('ostaniZdrav', ['data']),
    ...mapState('ostaniZdrav', ['loaded']),
    currentDate() {
      return this.data[this.data.length - 1].date
    },
    lastValueDate() {
      return this.data[this.data.length - 2].date
    },
    current() {
      return this.data[this.data.length - 1].users_published
    },
    lastValue() {
      return this.data[this.data.length - 2].users_published
    },
    dayBeforeLastValue() {
      return this.data[this.data.length - 3].users_published
    },
    twoDaysBeforeLastValue() {
      return this.data[this.data.length - 4].users_published
    },
    diff() {
      return this.lastValue - this.dayBeforeLastValue
    },
    dayBeforeLastDiff() {
      return this.dayBeforeLastValue - this.twoDaysBeforeLastValue
    },
    percentageDiff() {
      return this.dayBeforeLastValue === 0
        ? 0
        : Math.round(this.diff / this.dayBeforeLastValue * 1000) / 10
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
