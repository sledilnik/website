<template>
  <b-card :title="title" class="card-info" v-if="show">
    <b-card-text
      :id="elementId"
      class="text-center"
      :class="{
        'text-info': lastDay.diff != 0,
        'text-secondary': lastDay.diff == 0,
      }"
    >
      <font-awesome-icon icon="arrow-circle-up" v-if="lastDay.diff > 0" />
      <font-awesome-icon icon="arrow-circle-down" v-if="lastDay.diff < 0" />
      <font-awesome-icon icon="arrow-circle-right" v-if="lastDay.diff == 0" />&nbsp;
      <span>{{ lastDay.value }}</span>
      <span>
      [{{ lastDay.diff | prefixDiff }} | {{ lastDay.percentDiff | prefixDiff }}%]
      </span>
      <b-tooltip :target="elementId" triggers="hover">
        Prejšnji dan: {{ dayBefore.value }} [{{ dayBefore.diff | prefixDiff }}]
      </b-tooltip>
    </b-card-text>
    <b-card-text class="data-time text-center">{{ formattedDate }}</b-card-text>
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
      if (this.goodDirection == 'up') {
        if (this.diffdiff < 0) {
          return 'text-danger'
        }
        if (this.diffdiff > 0) {
          return 'text-success'
        }
        return 'text-info'
      } else {
        if (this.diffdiff > 0) {
          return 'text-danger'
        }
        if (this.diffdiff < 0) {
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
    this.lastDay.percentDiff = Math.round((this.lastDay.diff / this.lastDay.value) * 1000) / 10
    this.dayBefore.diff = this.dayBefore.value - this.day2Before.value
    this.dayBefore.percentDiff = Math.round((this.dayBefore.diff / this.dayBefore.value) * 1000) / 10

    this.diffdiff = this.lastDay.diff - this.dayBefore.diff

    this.show = true
  }
};
</script>

<style scoped lang="scss">
@import "node_modules/bootstrap/scss/functions";
@import "node_modules/bootstrap/scss/variables";

.card.card-info {
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