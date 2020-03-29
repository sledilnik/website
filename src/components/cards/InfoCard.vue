<template>
  <div :title="title" class="hp-card-holder" v-if="show">
    <div class="hp-card">
      <div
        :id="elementId"
        class="text-center"
        :class="{
        'text-info': renderValues.lastDay.diff != 0,
        'text-secondary': renderValues.lastDay.diff == 0,
      }"
      >
        <span>{{ renderValues.lastDay.value }}</span>
        <span>[{{ renderValues.lastDay.diff | prefixDiff }} | {{ renderValues.lastDay.percentDiff | prefixDiff }}%]</span>
        <b-tooltip
          :target="elementId"
          triggers="hover"
        >Prejšnji dan: {{ renderValues.dayBefore.value }} [{{ renderValues.dayBefore.diff | prefixDiff }}]</b-tooltip>
      </div>
    </div>
    <div class="data-time text-center">{{ formattedDate }}</div>
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
  padding: 0 15px 30px;
  flex: 0 0 20%;
}

.card-info {
  color: $text-muted;

  .card-title {
    text-align: center;
    font-size: $font-size-base;
    // text-transform: uppercase;
  }
  .card-body {
    font-size: $font-size-base * 0.9;
    padding: 0.5rem;
  }

  .data-time {
    font-size: $font-size-sm * 0.7;
  }
}
</style>