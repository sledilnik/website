<template>
  <b-card :title="title" class="card-info col-12 col-md-3 col-lg-2 mx-3 mb-3 px-0" v-if="show">
    <b-card-text :id="elementId" class="info-card-value">{{ lastDay.value }}</b-card-text>
    <b-card-text
      :class="textClass"
    >{{ lastDay.diff | prefixDiff }} ({{ lastDay.percentDiff | prefixDiff }}%)</b-card-text>
    <b-card-text class="data-time">{{ formattedDate }}</b-card-text>
    <b-tooltip
      :target="elementId"
      triggers="hover"
    >Prejšnji dan: {{ dayBefore.value }} ({{ dayBefore.diff | prefixDiff }})</b-tooltip>
  </b-card>
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
      let diff = this.lastDay.diff;
      if (this.goodDirection == "up") {
        if (diff < 0) {
          return "text-danger";
        }
        if (diff > 0) {
          return "text-success";
        }
        return "text-info";
      } else {
        if (diff > 0) {
          return "text-danger";
        }
        if (diff < 0) {
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
.card.card-info {
  border-radius: 0px;
  border: none;
  color: #000000;

  .card-title {
    font-size: 14px;
    font-weight: 600;

    // text-align: center;
    font-size: $font-size-base;
    // text-transform: uppercase;
  }

  .card-text {
    margin-bottom: 0.5rem;
  }

  .card-body {
    font-size: $font-size-base * 0.9;
    padding: 1.9rem;

    .info-card-value {
      font-size: 32px;
      font-weight: 600;
    }
  }

  .data-time {
    color: $text-muted;
    font-size: 0.75rem;
  }
}
</style>