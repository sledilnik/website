<template>
  <div :title="title" class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <span class="card-title">{{ title }}</span>
      <div class="card-wrapper">
        <span class="card-number">
          {{ renderValues.lastDay.value }}
          <span class="card-percentage-diff" :class="diffClass"
            >{{ renderValues.lastDay.percentDiff | prefixDiff }}%
          </span>
        </span>
        <div :id="field" class="card-diff">
          <span v-if="showAbsolute">
            <div class="trend-icon" :class="[diffClass, iconClass]"></div>
            <span :class="diffClass"
              >{{ Math.abs(renderValues.lastDay.diff) }}
            </span>
          </span>
          <span v-if="showIn">
            <div class="trend-icon in bad up"></div>
            <span v-if="field === 'cases.active'" class="in bad"
              >{{ renderActiveValues(fieldNewCases).lastDay.value }}</span>
            <span v-else class="in bad"
              >{{ renderTotalValues(totalIn) }}</span>
          </span>
          <span v-if="showOut">
            <div class="trend-icon out good down"></div>
            <span v-if="field === 'cases.active'" class="out good"
              >{{
                renderActiveValues(fieldNewCases).lastDay.value -
                  renderActiveValues(field).lastDay.diff +
                  renderActiveValues(fieldDeceased).lastDay.value
              }}</span>
            <span v-else class="out good"
              >{{ renderTotalValues(totalOut) }}</span>
          </span>
          <span v-if="showDeceased">
            <div class="trend-icon deceased"></div>
            <span v-if="field === 'cases.active'" class="deceased"
              >{{ renderActiveValues(fieldDeceased).lastDay.value }}</span>
            <span v-else class="deceased"
              >{{ renderTotalValues(totalDeceased) }}
            </span>
          </span>
        </div>
        <div class="data-time"
          >{{
            $t('infocard.lastUpdated', {
              date: new Date(renderValues.lastDay.displayDate),
            })
          }}</div>
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
    fieldNewCases: String,
    fieldDeceased: String,
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
  computed: {
    ...mapGetters('stats', ['lastChange']),
    ...mapGetters('patients', { patients: 'data' }),
    ...mapState('stats', ['exportTime', 'loaded']),
    diffClass() {
      if (
        this.field === 'statePerTreatment.deceasedToDate' ||
        (this.renderActiveValues(this.fieldDeceased) &&
          this.renderActiveValues(this.fieldDeceased).lastDay.value > 0)
      ) {
        return 'deceased'
      }
      if (
        this.field === 'statePerTreatment.inHospital' ||
        this.field === 'statePerTreatment.inICU'
      ) {
        if (this.renderTotalValues(this.totalDeceased) > 0) return 'deceased'
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
        return (
          this.renderActiveValues(this.field).lastDay.diff === 0 &&
          this.renderActiveValues(this.fieldNewCases).lastDay.value === 0 &&
          this.renderActiveValues(this.fieldDeceased).lastDay.value === 0
        )
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
          this.renderTotalValues(this.totalDeceased) !== 0 )
      )
    },
    showIn() {
      if (this.field === 'cases.active') {
        return this.renderActiveValues(this.fieldNewCases).lastDay.value > 0
      }
      return this.totalIn && this.renderTotalValues(this.totalIn) > 0
    },
    showOut() {
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
      if (this.field === 'cases.active') {
        return this.renderActiveValues(this.fieldDeceased).lastDay.value > 0
      }
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

.card-wrapper {
  margin-top: auto;
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
  font-size: 12px;
  color: #a0a0a0;
}
</style>
