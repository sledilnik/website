<template>
  <div :title="title" class="hp-card-holder" v-if="show">
    <div class="hp-card">
      <span class="card-title">{{title}}</span>
      <span class="card-number">{{ renderValues.lastDay.value }}</span>
      <div
        :id="elementId"
        class="card-diff"
        :class="{
        'bad': renderValues.lastDay.diff != 0,
        'good': renderValues.lastDay.diff == 0,
      }"
      >
        <span>{{ renderValues.lastDay.diff | prefixDiff }} ({{ renderValues.lastDay.percentDiff | prefixDiff }}%)</span>
        <b-tooltip
          :target="elementId"
          triggers="hover"
        >Prejšnji dan: {{ renderValues.dayBefore.value }} [{{ renderValues.dayBefore.diff | prefixDiff }}]</b-tooltip>
      </div>
      <div class="data-time">{{ formattedDate }}</div>
    </div>
  </div>
</template>
<script>
import Vue from "vue";
import moment from "moment";
import { mapGetters } from "vuex";

Vue.filter("prefixDiff", function(value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});

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
    ...mapGetters(["getLastValue", "getValueOn"]),
    lastDay() {
      return this.getLastValue(this.field);
    },
    dayBefore() {
      let dayBefore = moment(this.lastDay.date)
        .subtract(1, "days")
        .toDate();
      return this.getValueOn(this.field, dayBefore);
    },
    day2Before() {
      let day2Before = moment(this.lastDay.date)
        .subtract(2, "days")
        .toDate();
      return this.getValueOn(this.field, day2Before);
    },
    renderValues() {
      let diff = this.lastDay.value - this.dayBefore.value;
      let dayBeforeDiff = this.dayBefore.value - this.day2Before.value;
      return {
        diffdiff: diff - dayBeforeDiff,
        lastDay: {
          ...this.lastDay,
          diff,
          percentDiff: Math.round((diff / this.dayBefore.value) * 1000) / 10
        },
        dayBefore: {
          ...this.dayBefore,
          diff: dayBeforeDiff,
          percentDiff:
            Math.round((dayBeforeDiff / this.day2Before.value) * 1000) / 10
        }
      };
    },
    elementId() {
      return this.field;
    },
    textClass() {
      if (this.goodDirection == "up") {
        if (this.diffdiff < 0) {
          return "text-danger";
        }
        if (this.diffdiff > 0) {
          return "text-success";
        }
        return "text-info";
      } else {
        if (this.diffdiff > 0) {
          return "text-danger";
        }
        if (this.diffdiff < 0) {
          return "text-success";
        }
        return "text-info";
      }
    },
    formattedDate() {
      let dateFormatted = moment(this.lastDay.date).format("DD. MM. YYYY");
      return `Osveženo ${dateFormatted}`;
    }
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
}

.data-time {
  font-size: 12px;
  color: #a0a0a0;
}
</style>