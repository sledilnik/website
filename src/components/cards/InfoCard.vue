<template>
  <div class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <div class="card-title d-flex justify-content-between">
        <span>{{ title }}</span>
        <a
          v-if="showPhaseIndicator"
          href="/faq#chart-infocard-phase"
          target="_blank"
          class="card-phase-indicator"
          v-b-tooltip.html :title="phaseTitle"
        >
          <div class="trend-icon phase" :class="incidenceClass">
            <span>{{ getPhase }}</span>
          </div>
        </a>
      </div>
      <div class="card-number">
        <span v-if="showIncidence">{{
          Math.round(renderValues.lastDay.value / incidence)
        }}</span>
        <span v-else>{{ renderValues.lastDay.value }}</span>
        <div class="card-percentage-diff" :class="diffClass">
          {{ renderValues.lastDay.percentDiff | prefixDiff }}%
        </div>
      </div>
      <div :id="name" class="card-diff">
        <div v-if="showIncidence">
          <span class="card-note">{{ $t('infocard.per100k') }} </span>
        </div>
        <div v-if="showAbsolute">
          <div class="trend-icon" :class="[diffClass, iconClass]"></div>
          <span :class="diffClass"
            >{{ Math.abs(renderValues.lastDay.diff) }}
          </span>
        </div>
        <div v-if="showIn" class="card-diff-item">
          <div class="trend-icon in bad up"></div>
          <span v-if="field === 'cases.active'" class="in bad">{{
            renderActiveValues(fieldNewCases).lastDay.value
          }}</span>
          <span v-else class="in bad">{{ renderTotalValues(totalIn) }}</span>
        </div>
        <div v-if="showOut" class="card-diff-item">
          <div class="trend-icon out good down"></div>
          <span v-if="field === 'cases.active'" class="out good">{{
            renderActiveValues(fieldNewCases).lastDay.value -
              renderActiveValues(field).lastDay.diff -
              renderActiveValues(fieldDeceased).lastDay.value
          }}</span>
          <span v-else class="out good">{{ renderTotalValues(totalOut) }}</span>
        </div>
        <div v-if="showDeceased" class="card-diff-item">
          <div class="trend-icon deceased"></div>
          <span v-if="field === 'cases.active'" class="deceased">{{
            renderActiveValues(fieldDeceased).lastDay.value
          }}</span>
          <span v-else class="deceased"
            >{{ renderTotalValues(totalDeceased) }}
          </span>
        </div>
      </div>
      <!-- <trend
        :data="lastData(0, 14, field)"
        :gradient="['#ffbe88', '#ffbe88', '#ffbe88']"
        :stroke-width="4"
        auto-draw
        smooth
        class="sparkline"
      >
      </trend> -->
      <div class="data-time">
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
    totalIn: String,
    totalOut: String,
    totalDeceased: String,
    goodTrend: {
      type: String,
      default: 'down',
    },
    name: String,
    seriesType: {
      type: String,
      default: 'cum',
    },
  },
  computed: {
    ...mapGetters('stats', ['lastChange']),
    ...mapGetters('patients', { patients: 'data' }, ['lastData']),
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
      if (this.renderValues.lastDay.diff === 0) {
        return 'no-change'
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodTrend === 'down' ? 'bad' : 'good'
      } else {
        return this.goodTrend === 'down' ? 'good' : 'bad'
      }
    },
    incidenceClass() {
      const value = this.renderValues.lastDay.value
      const incidence = Math.round(
        this.renderValues.lastDay.value / this.incidence
      )
      if (this.name === 'incidence') {
        if (incidence >= 40 && incidence < 140) return 'orange'
        if (incidence >= 140) return 'red'
      }
      if (this.field === 'statePerTreatment.inHospital') {
        if (value >= 60 && value < 250) return 'orange'
        if (value >= 250) return 'red'
      }
      if (this.field === 'statePerTreatment.inICU') {
        if (value >= 15 && value < 50) return 'orange'
        if (value >= 50) return 'red'
      }
      return 'unknown'
    },
    cardTitle() {
      if (this.name === 'incidence')
        return this.title + ' ' + this.$t('infocard.per100k')
      return this.title
    },
    phaseTitle() {
      const value = this.renderValues.lastDay.value
      const incidence = Math.round(
        this.renderValues.lastDay.value / this.incidence
      )
      let phaseNextNumber = 1
      let phaseNextPackage = this.$t('infocard.orangePhaseGenitive')
      let phaseNextCriteria = 140
      let string1 = ''
      if (this.name === 'incidence') {
        if (incidence >= 40 && incidence < 80) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package1'
          )}</strong>`
          phaseNextNumber = 2
          phaseNextCriteria = 80
        }
        if (incidence >= 80 && incidence < 120) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package2'
          )}</strong>`
          phaseNextNumber = 3
          phaseNextCriteria = 120
        }
        if (incidence >= 120 && incidence < 140) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
          phaseNextNumber = 1
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 140
        }
        if (incidence >= 140 && incidence < 170) {
          string1 = `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package1'
          )}</strong>`
          phaseNextNumber = 2
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 170
        }
        if (incidence >= 170) {
          return `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
        }
      }
      if (this.field === 'statePerTreatment.inHospital') {
        if (value >= 60 && value < 100) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package1'
          )}</strong>`
          phaseNextNumber = 2
          phaseNextCriteria = 100
        }
        if (value >= 100 && value < 180) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package2'
          )}</strong>`
          phaseNextNumber = 3
          phaseNextCriteria = 180
        }
        if (value >= 180 && value < 250) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
          phaseNextNumber = 1
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 250
        }
        if (value >= 250 && value < 300) {
          string1 = `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package1'
          )}</strong>`
          phaseNextNumber = 2
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 300
        }
        if (value >= 300 && value < 360) {
          string1 = `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package2'
          )}</strong>`
          phaseNextNumber = 3
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 360
        }
        if (value >= 360) {
          return `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
        }
      }
      if (this.field === 'statePerTreatment.inICU') {
        if (value >= 15 && value < 20) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package1'
          )}</strong>`
          phaseNextNumber = 2
          phaseNextCriteria = 20
        }
        if (value >= 20 && value < 30) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package2'
          )}</strong>`
          phaseNextNumber = 3
          phaseNextCriteria = 30
        }
        if (value >= 30 && value < 50) {
          string1 = `<strong>${this.$t('infocard.orangePhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
          phaseNextNumber = 1
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 50
        }
        if (value >= 50 && value < 60) {
          string1 = `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package2'
          )}</strong>`
          phaseNextNumber = 3
          phaseNextPackage = this.$t('infocard.redPhaseGenitive')
          phaseNextCriteria = 60
        }
        if (value >= 60) {
          return `<strong>${this.$t('infocard.redPhase')}, ${this.$t(
            'infocard.package3'
          )}</strong>`
        }
      }
      let string2 = this.$t('infocard.nextCriteria', { phaseNextNumber, phaseNextPackage, phaseNextCriteria })
      return string1 + string2
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
          this.renderTotalValues(this.totalDeceased) !== 0)
      )
    },
    showIn() {
      if (this.showIncidence) return false
      if (this.field === 'cases.active') {
        return this.renderActiveValues(this.fieldNewCases).lastDay.value > 0
      }
      return this.totalIn && this.renderTotalValues(this.totalIn) > 0
    },
    showOut() {
      if (this.showIncidence) return false
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
      if (this.showIncidence) return false
      if (this.field === 'cases.active') {
        return this.renderActiveValues(this.fieldDeceased).lastDay.value > 0
      }
      return (
        this.totalDeceased && this.renderTotalValues(this.totalDeceased) > 0
      )
    },
    showIncidence() {
      if (this.name === 'incidence') {
        return true
      }
      return false
    },
    getPhase() {
      const value = this.renderValues.lastDay.value
      const incidence = Math.round(
        this.renderValues.lastDay.value / this.incidence
      )
      if (this.name === 'incidence') {
        if (incidence >= 40 && incidence < 80) return 1
        if (incidence >= 80 && incidence < 120) return 2
        if (incidence >= 120 && incidence < 140) return 3
        if (incidence >= 140 && incidence < 170) return 1
        if (incidence >= 170) return 3
      }
      if (this.field === 'statePerTreatment.inHospital') {
        if (value >= 60 && value < 100) return 1
        if (value >= 100 && value < 180) return 2
        if (value >= 180 && value < 250) return 3
        if (value >= 250 && value < 300) return 1
        if (value >= 300 && value < 360) return 2
        if (value >= 360) return 3
      }
      if (this.field === 'statePerTreatment.inICU') {
        if (value >= 15 && value < 20) return 1
        if (value >= 20 && value < 30) return 2
        if (value >= 30 && value < 50) return 3
        if (value >= 50 && value < 60) return 2
        if (value >= 60) return 3
      }
      return 0
    },
    showPhaseIndicator() {
      if (this.name === 'incidence') {
        return true
      }
      if (this.field === 'statePerTreatment.inHospital') {
        return true
      }
      if (this.field === 'statePerTreatment.inICU') {
        return true
      }
      return false
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

  &.phase {
    border-radius: 50px;
    width: 17px;
    height: 17px;
    color: white;
    vertical-align: text-top;
    cursor: pointer;

    span {
      position: relative;
      left: 5px;
      top: -1px;
      font-size: 13px;
      font-weight: bold;
    }
  }

  &.orange {
    background-color: orange;
  }

  &.red {
    background-color: #bf5747;
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
}
</style>
