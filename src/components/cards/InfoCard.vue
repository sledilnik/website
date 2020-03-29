<template>
  <b-card :title="title" class="card-info" v-if="show">
    <b-card-text
      :id="elementId"
      class="info-card-value"
    >
    {{ lastDay.value }}
    </b-card-text>
    <b-card-text
      :class="textClass">
      {{ lastDay.diff | prefixDiff }} ({{ lastDay.percentDiff | prefixDiff }}%)
    </b-card-text>    
    <b-card-text class="data-time">{{ formattedDate }}</b-card-text>
    <b-tooltip :target="elementId" triggers="hover">
      Prejšnji dan: {{ dayBefore.value }} ({{ dayBefore.diff | prefixDiff }})
    </b-tooltip>
  </b-card>
</template>
<script>
import Vue from "vue";
import moment from "moment";
import StatsData from "StatsData";

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
    },
  },
  data() {
    return {
      show: false,
      dayBefore: {},
      lastDay: {},
      date: null,
      value: null,
      percentDiff: null,
      diff: null,
      diffdiff: null,
    };
  },
  computed: {
    elementId() {
      return this.field
    },
    textClass() {
      let diff = this.lastDay.diff
      if (this.goodDirection == 'up') {
        if (diff < 0) {
          return 'text-danger'
        }
        if (diff > 0) {
          return 'text-success'
        }
        return 'text-info'
      } else {
        if (diff > 0) {
          return 'text-danger'
        }
        if (diff < 0) {
          return 'text-success'
        }
        return 'text-info'
      }
    },
    formattedDate() {
      let dateFormatted = moment(this.lastDay.date).format('DD. MM. YYYY');
      return `Osveženo ${dateFormatted}`;
    }
  },
  async mounted() {
    this.lastDay = await StatsData.getLastValue(this.field)
    

    let dayBefore = moment(this.lastDay.date)
      .subtract(1, "days")
      .toDate()
    let day2Before = moment(this.lastDay.date)
      .subtract(2, "days")
      .toDate()
    this.dayBefore = await StatsData.getValueOn(this.field, dayBefore)
    this.day2Before = await StatsData.getValueOn(this.field, day2Before)

    
    
    this.lastDay.diff = this.lastDay.value - this.dayBefore.value
    this.lastDay.percentDiff = Math.round((this.lastDay.diff / this.dayBefore.value) * 1000) / 10
    this.dayBefore.diff = this.dayBefore.value - this.day2Before.value
    this.dayBefore.percentDiff = Math.round((this.dayBefore.diff / this.day2Before.value) * 1000) / 10

    this.diffdiff = this.lastDay.diff - this.dayBefore.diff

    this.show = true
  }
};
</script>

<style scoped lang="scss">
@import "node_modules/bootstrap/scss/functions";
@import "node_modules/bootstrap/scss/variables";

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