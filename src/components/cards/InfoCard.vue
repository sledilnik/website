<template>
  <div :title="title" class="hp-card-holder" v-if="show">
    <div class="hp-card">
      <span class="card-title">{{title}}</span>
      <span class="card-number">{{ renderValues.lastDay.value }}</span>
      <div
        :id="elementId"
        class="card-diff"
        :class="diffClass"
      >
        <span>{{ renderValues.lastDay.diff | prefixDiff }} ({{ renderValues.lastDay.percentDiff | prefixDiff }}%)</span>
        <b-tooltip
          :target="elementId"
          triggers="hover"
        >Prejšnji dan: {{ renderValues.dayBefore.value }} [{{ renderValues.dayBefore.diff | prefixDiff }}]</b-tooltip>
      </div>
      <div class="data-time">Osveženo {{ renderValues.lastDay.date | formatDate('dd. MM. yyyy') }}</div>
      <!-- <div class="data-time">Na dan {{ renderValues.lastDay.date | formatDate('dd. MM. yyyy') }}</div> -->
      <!-- <div class="data-time">Osveženo {{ exportTime | formatDate('dd. MM. yyyy HH:mm') }}</div> -->
    </div>
  </div>
</template>
<script>
import { mapGetters, mapState } from "vuex";
import { add } from "date-fns";

export default {
  props: {
    title: String,
    field: String,
    goodDirection: {
      type: String,
      default: "down"
    }
  },
  data() {
    return {
      show: false
    };
  },
  computed: {
    ...mapGetters("stats", ["getLastValue", "getValueOn"]),
    ...mapState("stats", ["exportTime"]),
    lastDay() {
      return this.getLastValue(this.field);
    },
    diffClass() {
      if (this.renderValues.lastDay.diff == 0) {
        return 'no-change'
      } else if (this.renderValues.lastDay.diff > 0) {
        return this.goodDirection === "down" ? "bad" : "good"
      } else {
        return this.goodDirection === "down" ? "good" : "bad"
      }
    },
    dayBefore() {
      if (this.lastDay.date) {
        let date = new Date(this.lastDay.date.getTime());
        let dayBefore = add(date.setHours(0, 0, 0, 0), { days: -1 });
        return this.getValueOn(this.field, dayBefore);
      } else {
        return {};
      }
    },
    day2Before() {
      if (this.lastDay.date) {
        let date = new Date(this.lastDay.date.getTime());
        let day2Before = add(date.setHours(0, 0, 0, 0), { days: -2 });
        return this.getValueOn(this.field, day2Before);
      } else {
        return {};
      }
    },
    renderValues() {
      let diff = this.lastDay.value - this.dayBefore.value;
      let dayBeforeDiff = this.dayBefore.value - this.day2Before.value;
      return {
        diffdiff: diff - dayBeforeDiff,
        lastDay: {
          ...this.lastDay,
          diff,
          percentDiff:
            diff != 0
              ? Math.round((diff / this.dayBefore.value) * 1000) / 10
              : 0
        },
        dayBefore: {
          ...this.dayBefore,
          diff: dayBeforeDiff,
          percentDiff:
            dayBeforeDiff != 0
              ? Math.round((dayBeforeDiff / this.day2Before.value) * 1000) / 10
              : 0
        }
      };
    },
    elementId() {
      return this.field;
    },
  },
  mounted() {
    this.show = true;
  }
};
</script>

<style scoped lang="scss">
.hp-card-holder {
  flex: 0 0 100%;
  padding: 0 15px 30px;

  @media only screen and (min-width: 768px) {
    flex: 0 0 calc(100% / 3);
  }

  @media only screen and (min-width: 1200px) {
    flex: 0 0 20%;
  }
}

.hp-card {
  padding: 32px;
  background: #fff;
  box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3),
    0 11px 12px -12px rgba(0, 0, 0, 0.22);
  // transition: 0.35s ease-in-out;

  // &:hover {
  //   transform: translateY(-2px);
  //   box-shadow: 0 22px 38px -10px rgba(0, 0, 0, 0.3),
  //     0 18px 12px -10px rgba(0, 0, 0, 0.22);
  // }
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
  margin-bottom: 16px;

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
  font-size: 12px;
  color: #a0a0a0;
}
</style>