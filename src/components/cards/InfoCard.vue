<template>
  <div :title="title" class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <span class="card-title">{{ title }}</span>
      <span class="card-number">
        {{ renderValues.lastDay.value }}
        <span class="card-percentage-diff" :class="diffClass"
          >{{ renderValues.lastDay.percentDiff | prefixDiff }}%
        </span>
      </span>
      <div :id="elementId" class="card-diff">
        <span v-if="showAbsolute">
          <div class="trend-icon" :class="[diffClass, iconClass]"></div>
          <span :class="diffClass">
            {{ Math.abs(renderValues.lastDay.diff) }}
          </span>
        </span>
        <span v-if="showIn">
          <div
            class="trend-icon in"
            :class="[
              diffTotalClass(totalIn, 'down'),
              iconTotalClass(totalIn, 'down'),
            ]"
          ></div>
          <span class="in" :class="diffTotalClass(totalIn, 'down')">
            {{ renderTotalValues(totalIn) }}
          </span>
        </span>
        <span v-if="showOut">
          <div
            class="trend-icon out"
            :class="[
              diffTotalClass(totalOut, 'up'),
              iconTotalClass(totalOut, 'up'),
            ]"
          ></div>
          <span class="out" :class="diffTotalClass(totalOut, 'up')">
            {{ renderTotalValues(totalOut) }}
          </span>
        </span>
        <span v-if="showDeceased">
          <div
            class="trend-icon deceased"
            :class="[
              diffTotalClass(totalDeceased),
              iconTotalClass(totalDeceased),
            ]"
          ></div>
          <span class="deceased" :class="diffTotalClass(totalDeceased)">
            {{ renderTotalValues(totalDeceased) }}
          </span>
        </span>
      </div>
      <div class="data-time">
        {{
          $t('infocard.lastUpdated', {
            date: new Date(renderValues.lastDay.displayDate),
          })
        }}
      </div>
    </div>
    <div class="hp-card" v-else>
      <span class="card-title">{{ title }}</span>
      <font-awesome-icon icon="spinner" spin />
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from 'vuex'

export default {
  props: {
    title: String,
    field: String,
    totalIn: String,
    totalOut: String,
    totalDeceased: String,
    goodTrend: {
      type: String,
      default: 'down',
    },
    seriesType: {
      type: String,
      default: 'cum',
    },
  },
  data() {
    return {
      show: false,
    }
  },
  computed: {
    ...mapGetters('stats', ['lastChange']),
    ...mapGetters('patients', { patients: 'data' }),
    ...mapState('stats', ['exportTime', 'loaded']),
    diffClass() {
      if (this.field === 'statePerTreatment.deceasedToDate') {
        return 'deceased'
      }
      if (this.field === 'statePerTreatment.inHospital' || this.field === 'statePerTreatment.inICU') {
        if (this.renderTotalValues(this.totalDeceased) > 0)
          return 'deceased'
      }
      if (this.renderValues.lastDay.diff === 0) {
        return 'no-change'
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodTrend === 'down' ? 'bad' : 'good'
      } else {
        return this.goodTrend === 'down' ? 'good' : 'bad'
      }
    },
    iconClass() {
      let className = ''
      if (this.field === 'statePerTreatment.deceasedToDate') {
        className += 'deceased'
      }
      if (this.renderValues.lastDay.diff === 0) {
        className += ' none'
        return className
      } else if (this.renderValues.lastDay.diff > 0) {
        className += 'up'
      } else {
        className += 'down'
      }
      return className
    },
    renderValues() {
      let date
      Object.keys(this.$route.query).length > 0
        ? (date = this.$route.query.showDate)
        : (date = null)
      const x = this.lastChange(this.field, this.seriesType == 'cum', date)
      if (x) {
        if (this.seriesType == 'cum') {
          x.lastDay.displayDate = x.lastDay.firstDate || x.lastDay.date
          x.dayBefore.displayDate = x.dayBefore.firstDate || x.dayBefore.date
        } else {
          x.lastDay.displayDate = x.lastDay.date
          x.dayBefore.displayDate = x.dayBefore.date
        }
      }
      return x
    },
    elementId() {
      return this.field
    },
    showAbsolute() {
      if (this.field === 'statePerTreatment.inHospital' || this.field === 'statePerTreatment.inICU') {
        if (this.renderTotalValues(this.totalDeceased) > 0) {
          return false
        }
      }
      return (
        (!this.totalIn && !this.totalOut && !this.totalDeceased) ||
        (this.renderTotalValues(this.totalIn) === 0 &&
         this.renderTotalValues(this.totalOut) === 0)
      )
    },
    showIn() {
      return (
        this.totalIn && this.renderTotalValues(this.totalIn) > 0
      )
    },
    showOut() {
      return (
        this.totalOut && this.renderTotalValues(this.totalOut) > 0
      )
    },
    showDeceased() {
      return (
        this.totalDeceased && this.renderTotalValues(this.totalDeceased) > 0
      )
    },
  },
  methods: {
    renderTotalValues(value) {
      const lastDay = this.patients.filter((x) => {
        return (
          x.day === this.renderValues.lastDay.date.getDate() &&
          x.month === this.renderValues.lastDay.date.getMonth() + 1 &&
          x.year === this.renderValues.lastDay.date.getFullYear()
        )
      })
      return _.get(lastDay[0], value)
    },
    diffTotalClass(value, goodTrend) {
      if (this.totalDeceased > 0) {
        return
      }
      if (this.renderTotalValues(value) === 0) {
        return 'no-change'
      } else {
        return this.goodTrend === goodTrend ? 'bad' : 'good'
      }
    },
    iconTotalClass(value, goodTrend) {
      if (this.field === 'statePerTreatment.deceasedToDate') {
        return 'deceased'
      } else if (this.renderTotalValues(value) === 0) {
        return 'none'
      } else {
        return this.goodTrend === goodTrend ? 'up' : 'down'
      }
    },
  },
  mounted() {
    this.show = true
  },
}
</script>

<style scoped lang="scss">
.hp-card-holder {
  flex: 0 0 100%;
  padding: 0 15px 15px;

  @media only screen and (min-width: 400px) {
    flex: 0 0 calc(100% / 2);
    padding: 0 15px 30px;
  }

  @media only screen and (min-width: 768px) {
    flex: 0 0 calc(100% / 3);
  }

  @media only screen and (min-width: 1135px) {
    flex: 0 0 20%;
  }
}

.hp-card {
  display: flex;
  flex-direction: column;
  min-height: 195px;
  height: 100%;
  padding: 18px;
  background: #fff;
  box-shadow: $element-box-shadow;

  @include media-breakpoint-down(sm) {
    min-height: 167px;
  }

  @media only screen and (min-width: 480px) {
    padding: 26px;
  }

  @media only screen and (min-width: 768px) {
    padding: 32px;
  }
}

.card-title {
  display: block;
  font-size: 14px;
  font-weight: 700;
}

.card-number {
  display: block;
  font-size: 32px;
  font-weight: 700;
}

.card-percentage-diff {
  display: inline-block;
  font-size: 14px;
  font-weight: normal;
}

.card-diff {
  font-size: 14px;
  margin-bottom: 0.7rem;

  > *:not(:last-child) {
    margin-right: 8px;
  }

  .trend-icon {
    display: inline-block;
    width: 24px;
    height: 24px;
    object-fit: contain;
    vertical-align: bottom;

    &.bad {
      background-color: #bf5747;
    }

    &.good {
      background-color: #20b16d;
    }

    &.up {
      -webkit-mask: url(../../assets/svg/close-circle-up.svg) no-repeat center;
      mask: url(../../assets/svg/close-circle-up.svg) no-repeat center;
    }

    &.down {
      -webkit-mask: url(../../assets/svg/close-circle-down.svg) no-repeat center;
      mask: url(../../assets/svg/close-circle-down.svg) no-repeat center;
    }

    &.deceased {
      -webkit-mask: url(../../assets/svg/close-circle-deceased.svg) no-repeat
        center;
      mask: url(../../assets/svg/close-circle-deceased.svg) no-repeat center;
      background-color: #404040;
    }

    &.none {
      display: none;
    }

    &.no-change {
      background-color: #a0a0a0;
    }
  }
}

.bad {
  color: #bf5747;
}

.good {
  color: #20b16d;
}

.no-change,
.deceased {
  color: #a0a0a0;
}

.data-time {
  margin-top: auto;
  font-size: 12px;
  color: #a0a0a0;
}
</style>
