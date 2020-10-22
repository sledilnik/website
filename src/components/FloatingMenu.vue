<template>
  <div>
    <transition name="slide">
      <div class="float-nav-btn" v-show="list.length > 0" @click="toggleMenu">
        <div
          class="float-nav-img"
          :class="{ active: active }"
          :alt="$t('navbar.goToGraph')"
        />
      </div>
    </transition>
    <div v-show="active && list.length > 0" class="float-list">
      <h2>{{ $t('navbar.goToGraph') }}</h2>
      <ul v-scroll-lock="active">
        <li v-for="item in list" :key="item" class="float-item">
          <a
            :href="'#' + item"
            @click="hideMenu"
            v-scroll-to="{ element: '#' + item, offset: -100 }"
            >{{ shortTitle(item) }}</a
          >
        </li>
      </ul>
    </div>
    <div v-show="active" class="overlay" @click="hideMenu"></div>
  </div>
</template>

<script>
export default {
  name: 'FloatingMenu',
  props: {
    list: Array,
  },
  data() {
    return {
      active: false,
      charts: {
        'metrics-comparison-chart': this.$t('charts.metricsComparison.title_SVN'),
        'spread-chart': this.$t('charts.spread.title'),
        'daily-comparison-chart': this.$t('charts.dailyComparison.title'),
        'patients-chart': this.$t('charts.patients.title'),
        'map-chart': this.$t('charts.map.title'),
        'municipalities-chart': this.$t('charts.municipalities.title'),
        'europe-chart': this.$t('charts.europe.title'),
        'age-groups-trends-chart': this.$t('charts.ageGroups.title'),
        'tests-chart': this.$t('charts.tests.title'),
        'sources-chart': this.$t('charts.sources.title'),
        'hcenters-chart': this.$t('charts.hCenters.title'),
        'infections-chart': this.$t('charts.infections.title'),
        'cases-chart': this.$t('charts.cases.title'),
        'age-groups-chart': this.$t('charts.ageGroupsTimeline.title'),
        'rmap-chart': this.$t('charts.rmap.title'),
        'regions-chart': this.$t('charts.regions.title'),
        'regions-chart-100k': this.$t('charts.regions100k.title'),
        'world-chart': this.$t('charts.world.title'),
        'countries-active-chart': this.$t('charts.countriesActiveCasesPer1M.title'),
        'countries-cases-chart': this.$t('charts.countriesNewCasesPer1M.title'),
        'countries-deaths-chart': this.$t('charts.countriesTotalDeathsPer1M.title'),
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
@mixin nav-break {
  @media only screen and (min-width: 1150px) {
    @content;
  }
}

.float-list {
  position: fixed;
  bottom: 80px;
  right: 16px;
  padding: 16px 0 0px 16px;
  z-index: 2001;
  background: white;
  font-size: 14px;
  border-radius: 6px;
  box-shadow: $element-box-shadow;
  min-width: 235px;
  max-height: 600px;

  @include nav-break {
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

    &:last-child {
      padding-bottom: 16px;
    }

    a {
      padding: 6px 0;
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
  z-index: 2002;
  cursor: pointer;
  display: inline-block;

  @include nav-break {
    right: 24px;
    bottom: 24px;
  }
}

.float-nav-img {
  background-image: url('../assets/svg/floating-close.svg');
  height: 72px;
  width: 72px;
  transition: all 0.3s;

  &.active {
    background-image: url('../assets/svg/floating-open.svg');
  }
}

.overlay {
  position: fixed;
  background: rgba(0, 0, 0, 0.75);
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 2000;
}

.slide-leave-active,
.slide-enter-active {
  transition: 0.7s;
}
.slide-enter {
  will-change: transform;
  transform: translate(100%, 0);
}
.slide-leave-to {
  transform: translate(-100%, 0);
}
</style>
