<template>
  <div :title="title" class="hp-card-holder">
    <div class="hp-card" v-if="loaded">
      <span class="card-title">{{ title }}</span>
      <span class="card-number">{{ renderValues.lastDay.value }}</span>
      <div :id="elementId" class="card-diff" :class="diffClass">
        <span>{{ renderValues.lastDay.diff | prefixDiff }} ({{ renderValues.lastDay.percentDiff | prefixDiff }}%)</span>
        <b-tooltip :target="elementId" triggers="hover"
          >Glede na {{ renderValues.dayBefore.date | formatDate('d. MMMM') }}: {{ renderValues.dayBefore.value }} <span v-if="renderValues.dayBefore.diff">[{{
            renderValues.dayBefore.diff | prefixDiff
          }}]</span></b-tooltip
        >
      </div>
      <div class="data-time">{{ renderValues.lastDay.displayDate | formatDate('d. MMMM') }}</div>
    </div>
    <div class="hp-card" v-else>
      <span class="card-title">{{ title }}</span>
      <font-awesome-icon icon="spinner" spin />
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from 'vuex';
// import { add } from 'date-fns';

export default {
  props: {
    title: String,
    field: String,
    goodDirection: {
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
    ...mapState('stats', ['exportTime', 'loaded']),
    diffClass() {
      if (this.renderValues.lastDay.diff == 0) {
        return 'no-change';
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodDirection === 'down' ? 'bad' : 'good';
      } else {
        return this.goodDirection === 'down' ? 'good' : 'bad';
      }
    },
    renderValues() {
      const x = this.lastChange(this.field);
      if (x) {
        if (this.seriesType == 'cum') {
          x.lastDay.displayDate = x.lastDay.firstDate || x.lastDay.date
        } else {
          x.lastDay.displayDate = x.lastDay.date
        }
      }
      return x
    },
    elementId() {
      return this.field;
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

  @media only screen and (min-width: 1100px) {
    flex: 0 0 20%;
  }
}

.hp-card {
  min-height: 195px;
  padding: 18px;
  background: #fff;
  box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22);

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

.card-diff {
  font-size: 14px;

  &.bad {
    color: #bf5747;
  }

  &.good {
    color: #20b16d;
  }

  &.no-change {
    color: #a0a0a0;
  }
}

.data-time {
  margin-top: 0.7rem;
  // text-align: center;
  font-size: 12px;
  color: #a0a0a0;
}
</style>
