<template>
  <div class="hp-card" v-if="loaded">
    <div class="card-title d-flex justify-content-between">
      <span>{{ title }}</span>
    </div>
    <div class="d-flex">
      <div>
        <div class="card-number">
          <span>{{ lastValue.published }}</span>
          <div class="card-percentage-diff" :class="diffClass">
            {{ diff | prefixDiff }}%
          </div>
        </div>
        <div class="card-diff">
          <div><span class="card-note">published</span></div>
        </div>
      </div>
      <div class="ml-0 ml-sm-5">
        <div class="card-number">
          <span>{{ lastValue.users_published }}</span>
          <div class="card-percentage-diff" :class="diffClassUsers">
            {{ diffUsers | prefixDiff }}%
          </div>
        </div>
        <div class="card-diff">
          <div><span class="card-note">users</span></div>
        </div>
      </div>
    </div>
    <div class="data-time">
      {{
        $t('infocard.lastUpdated', {
          date: new Date(lastValue.date),
        })
      }}
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
    lastValue() {
      const lastValue = this.data[this.data.length - 1]
      return lastValue
    },
    diff() {
      const lastValue = this.data[this.data.length - 1]
      const oneBeforeLastValue = this.data[this.data.length - 2]
      return lastValue.published - oneBeforeLastValue.published
    },
    diffUsers() {
      const lastValue = this.data[this.data.length - 1]
      const oneBeforeLastValue = this.data[this.data.length - 2]
      return lastValue.users_published - oneBeforeLastValue.users_published
    },
    diffClass() {
      if (this.diff === 0) {
        return 'no-change'
      } else {
        return this.diff > 0 ? 'good' : 'bad'
      }
    },
    diffClassUsers() {
      if (this.diffUsers === 0) {
        return 'no-change'
      } else {
        return this.diffUsers > 0 ? 'good' : 'bad'
      }
    },
  },
}
</script>

<style></style>
