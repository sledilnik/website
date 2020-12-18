<template>
  <div class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <div class="card-title">{{ title }}</div>
      <div class="card-number">
        <span v-if="showRunningSum">{{ renderRunningSum | number }}</span>
        <span v-else>{{ renderValues.lastDay.value | number }}</span>
        <div v-if="showRunningSum" class="card-percentage-diff" :class="diffSumClass">
          {{ renderRunningSumDiff | percent }}
        </div>
        <div v-else class="card-percentage-diff" :class="diffClass">
          {{ renderValues.lastDay.percentDiff | percent }}
        </div>
      </div>
      <div :id="name" class="card-diff">
        <div v-if="showRunningSum">
          <span class="card-note">{{ $t('infocard.newCases7dInfo') }}</span>
        </div>
        <div v-if="showAbsolute">
          <div class="trend-icon" :class="[diffClass, iconClass]"></div>
          <span :class="diffClass"
            >{{ Math.abs(renderValues.lastDay.diff) | number }}
          </span>
        </div>
        <div v-if="showIn" class="card-diff-item">
          <div class="trend-icon in bad up"></div>
          <span v-if="field === 'cases.active'" class="in bad">{{
            renderActiveValues(fieldNewCases).lastDay.value | number
          }}</span>
          <span v-else class="in bad">{{ renderTotalValues(totalIn) }}</span>
        </div>
        <div v-if="showOut && field !== 'cases.active'" class="card-diff-item">
          <div class="trend-icon out good down"></div>
          <span class="out good">{{ renderTotalValues(totalOut) | number }}</span>
        </div>
        <div v-if="showOut && field === 'cases.active'" class="card-diff-item">
          <div class="trend-icon deceased down"></div>
          <span class="out deceased">{{ renderActiveValues(fieldRecovered).lastDay.diff | number }}</span>
        </div>
        <div v-if="showDeceased" class="card-diff-item">
          <div class="trend-icon deceased"></div>
          <span class="deceased"
            >{{ renderTotalValues(totalDeceased) | number }}
          </span>
        </div>
      </div>
      <div class="data-time" :class="(new Date()-1000*3600*60)>renderValues.lastDay.displayDate? 'outdated':''">
        {{
          $t('infocard.lastUpdated', {
            date: new Date(renderValues.lastDay.displayDate),
          })
        }}
      </div>
    </div>
    <div class="hp-card" v-else>
      <div class="card-title">{{ title }}</div>
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
    fieldNewCases: String,
    fieldDeceased: String,
    fieldRecovered: String,
    totalIn: String,
    totalOut: String,
    totalDeceased: String,
    goodTrend: {
      type: String,
      default: 'down',
    },
    name: String,
    runningSumField: String,
    seriesType: {
      type: String,
      default: 'cum',
    },
  },
  computed: {
    ...mapGetters('stats', ['lastChange', 'runningSum']),
    ...mapGetters('patients', { patients: 'data', runningSumPatients: 'runningSumPatients' }),
    ...mapState('stats', ['exportTime', 'loaded']),
    incidence() {
      switch (localStorage.getItem('contextCountry')) {
        case 'SVN': {
          return 20.95861
          break
        }
        case 'MKD': {
          return 20.83374
          break
        }
        default:
          return 0
          break
      }
    },
    diffClass() {
      if (this.field === 'statePerTreatment.deceasedToDate') {
        return 'deceased'
      }
      if (this.field === 'cases.active') {
        if (this.renderActiveValues(this.fieldDeceased).lastDay.value > 0) {
          return 'deceased'
        }
      }
      if (this.renderValues.lastDay.diff === 0) {
        return 'no-change'
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodTrend === 'down' ? 'bad' : 'good'
      } else {
        return this.goodTrend === 'down' ? 'good' : 'bad'
      }
    },
    diffSumClass() {
      if (this.renderRunningSumDiff === 0) {
        return 'no-change'
      } else if (this.renderRunningSumDiff > 0) {
        return 'bad'
      } else {
        return 'good'
      }
    },
    cardTitle() {
      if (this.name === 'incidence')
        return this.title + ' ' + this.$t('infocard.per100k')
      return this.title
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
        className += ' up'
      } else {
        className += ' down'
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
    showAbsolute() {
      if (this.field === 'cases.active') {
        // return (
        //   this.renderActiveValues(this.field).lastDay.diff === 0 &&
        //   this.renderActiveValues(this.fieldNewCases).lastDay.value === 0 &&
        //   this.renderActiveValues(this.fieldDeceased).lastDay.value === 0
        // )
        return false
      }
      if (
        this.field === 'statePerTreatment.inHospital' ||
        this.field === 'statePerTreatment.inICU'
      ) {
        return (
          this.renderTotalValues(this.totalIn) === 0 &&
          this.renderTotalValues(this.totalOut) === 0 &&
          this.renderTotalValues(this.totalDeceased) === 0
        )
      }
      return (
        (!this.totalIn && !this.totalOut && !this.totalDeceased) ||
        (this.renderTotalValues(this.totalIn) === 0 &&
          this.renderTotalValues(this.totalOut) === 0 &&
          this.renderTotalValues(this.totalDeceased) !== 0)
      )
    },
    showIn() {
      if (this.showRunningSum) return false
      if (this.field === 'cases.active') {
        return this.renderActiveValues(this.fieldNewCases).lastDay.value > 0
      }
      return this.totalIn && this.renderTotalValues(this.totalIn) > 0
    },
    showOut() {
      if (this.showRunningSum) return false
      if (this.field === 'cases.active') {
        return (
          this.renderActiveValues(this.fieldNewCases).lastDay.value -
            this.renderActiveValues(this.field).lastDay.diff +
            this.renderActiveValues(this.fieldDeceased).lastDay.value >
          0
        )
      }
      return this.totalOut && this.renderTotalValues(this.totalOut) > 0
    },
    showDeceased() {
      if (this.showRunningSum) return false
      if (this.field === 'cases.active') {
        return false
      }
      return (
        this.totalDeceased && this.renderTotalValues(this.totalDeceased) > 0
      )
    },
    showRunningSum() {
      if (this.name === 'newCases7d') {
        return true
      }
      return false
    },
    renderRunningSum() {
      return this.runningSum(0, 7, this.runningSumField).toFixed(1)
    },
    renderRunningSumDiff() {
      const today = this.runningSum(0, 7, this.runningSumField)
      const yesterday = this.runningSum(1, 8, this.runningSumField)
      const diff = today - yesterday

      return Math.round((diff / yesterday) * 1000) / 10
    }
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
    renderActiveValues(value) {
      if (this.field === 'cases.active') {
        let date
        Object.keys(this.$route.query).length > 0
          ? (date = this.$route.query.showDate)
          : (date = null)
        const x = this.lastChange(value, this.seriesType == 'cum', date)
        return x
      }
      return null
    },
  },
}
</script>

<style lang="scss">
.hp-card-holder {
  flex: 1;
}

.hp-card {
  display: flex;
  flex-direction: column;
  // display: grid;
  // grid-template-rows: auto auto 1fr auto; // TODO: fix for other languages (hr,de)
  min-height: 166px;
  height: 100%;
  padding: 16px;
  background: #fff;
  box-shadow: $element-box-shadow;
  border-radius: 6px;

  @media only screen and (min-width: 480px) {
    padding: 26px;
  }

  @media only screen and (min-width: 768px) {
    padding: 20px 32px;
  }
}

.card-title {
  font-size: 13px;
  font-weight: 700;
  margin-bottom: 0.5rem !important;

  span {
    margin-right: 5px;
  }
}

.card-number {
  font-size: 32px;
  font-weight: 700;
  white-space: nowrap;
}

.card-percentage-diff {
  display: inline-block;
  font-size: 14px;
  font-weight: normal;
  margin-left: 7px;
}

.card-diff {
  font-size: 14px;
  margin-bottom: 0.7rem;

  .card-diff-item {
    display: inline-block;
  }

  .card-diff-item:not(:last-child) {
    margin-right: 4px;
    
    @media only screen and (min-width: 992px) {
      margin-right: 8px;
    }
  }
}

.card-note {
  font-size: 12px;
}

.card-average {
  font-size: 12px;
  margin-bottom: 4px;
}

.trend-icon {
  display: inline-block;
  width: 22px;
  height: 22px;
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

  &.down.deceased {
    -webkit-mask: url(../../assets/svg/close-circle-down-deceased.svg) no-repeat center;
    mask: url(../../assets/svg/close-circle-down-deceased.svg) no-repeat center;
  }

  &.none {
    display: none;
  }

  &.no-change {
    background-color: #a0a0a0;
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
  font-size: 12px;
  color: #a0a0a0;
  margin-top: auto;

  &.outdated {
    color: #bf5747;
  }
}
</style>
