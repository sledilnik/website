<template>
  <div :title="title" class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <span class="card-title">{{ title }}</span>
      <span class="card-number">{{ renderValues.lastDay.value }}</span>
      <div :id="elementId" class="card-diff">
        <div v-if="showAbsolute" class="trend-icon" :class="[diffClass, iconClass]"></div>
        <span v-if="showAbsolute" :class="diffClass">{{ Math.abs(renderValues.lastDay.diff) }}</span>
        <div v-if="showIn" class="trend-icon" :class="[diffInOutClass(totalIn), iconInOutClass(totalIn)]"></div>
        <span v-if="showIn" class="in" :class="diffInOutClass(totalIn)">{{ renderInOutValues(totalIn) }}</span>
        <div v-if="showOut" class="trend-icon" :class="[diffInOutClass(totalOut), iconInOutClass(totalOut)]"></div>
        <span v-if="showOut" class="out" :class="diffInOutClass(totalOut)">{{ renderInOutValues(totalOut) }}</span>
        <span class="card-percentage-diff" :class="diffClass">({{ renderValues.lastDay.percentDiff | prefixDiff }}%)</span>

        <!-- TODO: x.dayBefore values aren't calculated correctly, check stats.store.js why not -->
        <!-- <b-tooltip :target="elementId" triggers="hover">
          {{ $t("infocard.accordingTo", { date: new Date(renderValues.dayBefore.date) }) }}: {{ renderValues.dayBefore.value }}
          <span v-if="renderValues.dayBefore.diff">
            [{{ renderValues.dayBefore.diff | prefixDiff }}]
          </span>
        </b-tooltip> -->
      </div>
      <div class="data-time">{{ $t("infocard.lastUpdated", { date: new Date(renderValues.lastDay.displayDate) }) }}</div>
    </div>
    <div class="hp-card" v-else>
      <span class="card-title">{{ title }}</span>
      <font-awesome-icon icon="spinner" spin />
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from 'vuex';

export default {
  props: {
    title: String,
    field: String,
    totalIn: String,
    totalOut: String,
    goodTrend: {
      type: String,
      default: 'down',
    },
    seriesType: {
      type: String,
      default: 'cum'
    }
  },
  data() {
    return {
      show: false,
    };
  },
  computed: {
    ...mapGetters('stats', ['lastChange']),
    ...mapGetters('patients', { patients: 'data' }),
    ...mapState('stats', ['exportTime', 'loaded']),
    diffClass() {
      if (this.renderValues.lastDay.diff === 0) {
        return 'no-change';
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodTrend === 'down' ? 'bad' : 'good';
      } else {
        return this.goodTrend === 'down' ? 'good' : 'bad';
      }
    },
    iconClass() {
      if (this.field === 'statePerTreatment.deceasedToDate') {
        return "deceased";
      } else if (this.renderValues.lastDay.diff == 0) {
        return 'none';
      } else if (this.renderValues.lastDay.diff > 0) {
        return "up";
      } else {
        return "down";
      }
    },
    renderValues() {
      const x = this.lastChange(this.field, this.seriesType == 'cum');
      if (x) {
        // TODO: x.dayBefore values aren't calculated correctly, check stats.store.js why not
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
      return this.field;
    },
    showAbsolute() {
      return !this.totalIn && !this.totalOut || (this.renderInOutValues(this.totalIn) === this.renderInOutValues(this.totalOut))
    },
    showIn() {
      return this.totalIn && (this.renderInOutValues(this.totalIn) !== this.renderInOutValues(this.totalOut))
    },
    showOut() {
      return this.totalOut && (this.renderInOutValues(this.totalIn) !== this.renderInOutValues(this.totalOut))
    }
  },
  methods: {
    renderInOutValues(inOut) {
      const lastDay = this.patients.filter(x => {
        return x.day === this.renderValues.lastDay.date.getDate() &&
               x.month === this.renderValues.lastDay.date.getMonth() + 1 &&
               x.year === this.renderValues.lastDay.date.getFullYear()
      })
      return _.get(lastDay[0], inOut)
    },
    diffInOutClass(inOut) {
      if (this.renderInOutValues(inOut) === 0) {
        return 'no-change';
      } else if (this.renderInOutValues(inOut) > 0) {
        return this.goodTrend === 'down' ? 'bad' : 'good';
      } else {
        return this.goodTrend === 'down' ? 'good' : 'bad';
      }
    },
    iconInOutClass(inOut) {
      if (this.renderInOutValues(inOut) === 0) {
        return 'none';
      } else if (this.renderInOutValues(inOut) > 0) {
        return 'up';
      } else {
        return 'down';
      }
    },
  },
  mounted() {
    this.show = true;
  },
};
</script>

<style scoped lang="scss">
.hp-card-holder {
  flex: 0 0 100%;
  padding: 0 15px 15px;

  @media only screen and (min-width: 480px) {
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
  min-height: 195px;
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
  margin-left: 8px;
  font-size: 14px;
  font-weight: normal;
}

.card-diff {
  font-size: 14px;

  .trend-icon {
    display:inline-block;
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
      -webkit-mask: url(../../assets/svg/close-circle-deceased.svg) no-repeat center;
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
  .out {
    margin-left: 10px;
  }
}

.bad {
  color: #bf5747;
}

.good {
  color: #20b16d;
}

.no-change {
  color: #a0a0a0;
}

.data-time {
  margin-top: 0.7rem;
  // text-align: center;
  font-size: 12px;
  color: #a0a0a0;
}
</style>
