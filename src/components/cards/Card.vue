<template>
  <div :title="title" class="hp-card" v-if="loaded">
    <div class="card-embed">
      <div class="card-title">{{ title }}</div>
      <a
        class="brand-link"
        target="_blank"
        href="https://covid-19.sledilnik.org/"
        >COVID-19 Sledilnik</a
      >
    </div>
    <div class="card-number">
      <span>{{ renderValue(lastData) }}</span>
      <div class="card-percentage-diff"></div>
    </div>
    <div class="card-diff">
      <div v-if="renderIn(lastData) > 0" class="card-diff-item">
        <div class="trend-icon bad up"></div>
        <span class="in bad">{{ renderIn(lastData) }} </span>
      </div>
      <div v-if="renderOut(lastData) > 0" class="card-diff-item">
        <div class="trend-icon good down"></div>
        <span class="out good">{{ renderOut(lastData) }} </span>
      </div>
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
    field: String,
    hospital: String,
  },
  computed: {
    ...mapGetters('patients', { patients: 'data' }),
    ...mapState('patients', ['exportTime', 'loaded']),
    lastData() {
      const item = this.patients[this.patients.length - 1]
      return item
    },
  },
  methods: {
    renderValue(item) {
      const value = item.facilities[this.hospital][this.field].today
      if (!value) return 0
      return value
    },
    renderIn(item) {
      const value = item.facilities[this.hospital][this.field].in
      if (!value) return 0
      return value
    },
    renderOut(item) {
      const value = item.facilities[this.hospital][this.field].out
      if (!value) return 0
      return value
    },
  },
}
</script>

<style scoped lang="scss">
.hp-card {
  min-width: 230px;
}

.card-embed {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-title {
  margin-bottom: 0;
}
</style>
