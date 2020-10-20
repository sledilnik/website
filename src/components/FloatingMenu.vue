<template>
  <div>
    <transition name="slide">
      <div
        class="float-nav-btn"
        :class="{ active: active }"
        v-if="list.length > 0"
        @click="toggleMenu"
      >
        <img
          src="../assets/svg/floating-close.svg"
          alt="Close"
          v-if="!active"
        />
        <img src="../assets/svg/floating-open.svg" alt="Open" v-if="active" />
      </div>
    </transition>
    <transition name="fade">
      <div
        v-if="active && list.length > 0"
        v-on-clickaway="hideMenu"
        class="float-list"
      >
        <h2>Pojdi na graf</h2>
        <ul>
          <li v-for="item in list" :key="item" class="float-item">
            <a
              :href="'#' + item"
              v-scroll-to="{ element: '#' + item, offset: -100 }"
              v-on-clickaway="hideMenu"
              >{{ shortTitle(item) }}</a
            >
          </li>
        </ul>
      </div>
    </transition>
    <div v-if="active" class="overlay"></div>
  </div>
</template>

<script>
import { mixin as clickaway } from 'vue-clickaway'

export default {
  mixins: [clickaway],
  name: 'FloatingMenu',
  props: {
    list: Array,
  },
  data() {
    return {
      active: false,
      charts: {
        'metrics-comparison-chart': this.$t('charts.metricsComparison.title'),
        'spread-chart': this.$t('charts.spread.title'),
        'daily-comparison-chart': this.$t('charts.dailyComparison.title'),
        'patients-chart': this.$t('charts.patients.title'),
        'map-chart': this.$t('charts.map.title'),
        'municipalities-chart': this.$t('charts.municipalities.title'),
        'europe-chart': this.$t('charts.europe.title'),
        'age-groups-trends-chart': this.$t('charts.ageGroups.title'),
        'tests-chart': this.$t('charts.tests.title'),
        'sources-chart': this.$t('charts.spread.title'),
        'hcenters-chart': this.$t('charts.hCenters.title'),
        'infections-chart': this.$t('charts.infections.title'),
        'cases-chart': this.$t('charts.cases.title'),
        'age-groups-chart': this.$t('charts.ageGroupsTimeline.title'),
        'rmap-chart': this.$t('charts.rmap.title'),
        'regions-chart': this.$t('charts.regions.title'),
        'regions-chart-100k': this.$t('charts.regions100k.title'),
        'world-chart': this.$t('charts.world.title'),
        'countries-active-chart': this.$t(
          'charts.countriesActiveCasesPer1M.title'
        ),
        'countries-cases-chart': this.$t('charts.countriesNewCasesPer1M.title'),
        'countries-deaths-chart': this.$t(
          'charts.countriesTotalDeathsPer1M.title'
        ),
      },
    }
  },
  methods: {
    shortTitle(title) {
      return this.charts[title]
    },
    toggleMenu() {
      this.active = !this.active
    },
    hideMenu() {
      this.active = false
    },
  },
}
</script>

<style lang="scss">
.float-list {
  position: fixed;
  bottom: 80px;
  right: 16px;
  padding: 16px 0 16px 16px;
  z-index: 2000;
  background: white;
  font-size: 14px;
  border-radius: 6px;
  box-shadow: $element-box-shadow;
  min-width: 235px;
  max-height: 600px;

  @media only screen and (min-width: 768px) {
    bottom: 98px;
    right: 32px;
  }

  h2 {
    font-size: 21px;
    line-height: 28px;
    font-weight: bold;
    margin-bottom: 24px;
  }

  ul {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 400px;
    overflow: auto;
  }

  .float-item {
    padding: 0 16px 0 0;

    &:not(:last-child) {
      border-bottom: 1px solid #d8d8d8;
    }

    a {
      margin: 6px 0;
      display: block;
      cursor: pointer;
      text-decoration: none;
      color: rgba(0, 0, 0, 0.7);
      border-radius: 2px;
    }
  }
}

.float-nav-btn {
  position: fixed;
  bottom: 7px;
  right: 7px;
  z-index: 2001;
  cursor: pointer;
  display: inline-block;

  @media only screen and (min-width: 768px) {
    right: 24px;
    bottom: 24px;
  }
}

.overlay {
  position: fixed;
  background: rgba(0, 0, 0, 0.75);
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 1999;
}

.fade-enter,
.fade-leave-to {
  opacity: 0;
  max-height: 0px;
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.4s;
  max-height: 800px;
}

.slide-leave-active,
.slide-enter-active {
  transition: 0.7s;
}
.slide-enter {
  transform: translate(100%, 0);
}
.slide-leave-to {
  transform: translate(-100%, 0);
}
</style>
