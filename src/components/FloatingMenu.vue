<template>
  <div>
    <transition name="slide">
      <div
        class="float-nav-btn"
        v-show="visible && list.length > 0"
        @click="toggleMenu"
        key="1"
      >
        <div
          class="float-nav-img"
          :class="{ active: active }"
          :alt="$t('navbar.goToGraph')"
        />
      </div>
    </transition>
    <transition name="slide">
      <div v-show="active && list.length > 0" class="float-list" key="2">
        <h2>{{ $t('navbar.goToGraph') }}</h2>
        <ul v-scroll-lock="active">
          <li
            v-for="item in list"
            :key="item.link"
            class="float-item"
            @click="hideMenu"
          >
            <div
              :class="'float-nav-icon' + ' ' + get(item.link, 'icon')"
              :alt="$t('navbar.goToGraph')"
            />
            <a
              :href="'#' + item.link"
              v-scroll-to="{ el: '#' + item.link, offset: -115 }"
              >{{ get(item.link, 'titleMenu') }}</a
            >
          </li>
        </ul>
      </div>
    </transition>
    <transition name="fade">
      <div v-show="active" class="overlay" @click="hideMenu" key="3"></div>
    </transition>
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
      visible: true,
      charts: {
        'metrics-comparison-chart': {
          titleMenu: this.$t('charts.metricsComparison.titleMenu_SVN'),
          icon: 'graph',
        },
        'spread-chart': {
          titleMenu: this.$t('charts.spread.titleMenu'),
          icon: 'graph',
        },
        'daily-comparison-chart': {
          titleMenu: this.$t('charts.dailyComparison.titleMenu'),
          icon: 'column',
        },
        'patients-chart': {
          titleMenu: this.$t('charts.patients.titleMenu'),
          icon: 'column',
        },
        'care-patients-chart': {
          titleMenu: this.$t('charts.carePatients.titleMenu'),
          icon: 'column',
        },
        'map-chart': {
          titleMenu: this.$t('charts.map.titleMenu'),
          icon: 'map',
        },
        'municipalities-chart': {
          titleMenu: this.$t('charts.municipalities.titleMenu'),
          icon: 'column',
        },
        'europe-chart': {
          titleMenu: this.$t('charts.europe.titleMenu'),
          icon: 'map',
        },
        'age-groups-trends-chart': {
          titleMenu: this.$t('charts.ageGroupsTimeline.titleMenu'),
          icon: 'column',
        },
        'tests-chart': {
          titleMenu: this.$t('charts.tests.titleMenu'),
          icon: 'column',
        },
        'hc-cases-chart': {
          titleMenu: this.$t('charts.hcCases.titleMenu'),
          icon: 'graph',
        },
        'sources-chart': {
          titleMenu: this.$t('charts.sources.titleMenu'),
          icon: 'column',
        },
        'hcenters-chart': {
          titleMenu: this.$t('charts.hCenters.titleMenu'),
          icon: 'graph',
        },
        'infections-chart': {
          titleMenu: this.$t('charts.infections.titleMenu'),
          icon: 'graph',
        },
        'cases-chart': {
          titleMenu: this.$t('charts.cases.titleMenu'),
          icon: 'column',
        },
        'age-groups-chart': {
          titleMenu: this.$t('charts.ageGroups.titleMenu'),
          icon: 'column',
        },
        'rmap-chart': {
          titleMenu: this.$t('charts.rmap.titleMenu'),
          icon: 'map',
        },
        'regions-chart': {
          titleMenu: this.$t('charts.regions.titleMenu'),
          icon: 'graph',
        },
        'regions-chart-100k': {
          titleMenu: this.$t('charts.regions100k.titleMenu'),
          icon: 'graph',
        },
        'world-chart': {
          titleMenu: this.$t('charts.world.titleMenu'),
          icon: 'map',
        },
        'countries-active-chart': {
          titleMenu: this.$t('charts.countriesActiveCasesPer1M.titleMenu'),
          icon: 'graph',
        },
        'countries-cases-chart': {
          titleMenu: this.$t('charts.countriesNewCasesPer1M.titleMenu'),
          icon: 'graph',
        },
        'countries-new-deaths-chart': {
          titleMenu: this.$t('charts.countriesNewDeathsPer100k.titleMenu'),
          icon: 'graph',
        },
        'countries-total-deaths-chart': {
          titleMenu: this.$t('charts.countriesTotalDeathsPer1M.titleMenu'),
          icon: 'graph',
        },
        'phase-diagram-chart': {
          titleMenu: this.$t('charts.phaseDiagram.titleMenu'),
          icon: 'graph',
        },
        'deceased-chart': {
          titleMenu: this.$t('charts.deceased.titleMenu'),
          icon: 'column',
        },
        'youtube': {
          titleMenu: this.$t('youtube.titleMenu'),
          icon: 'map',
        },
      },
    }
  },
  created() {
    window.addEventListener('scroll', this.handleScroll, { passive: true })
    window.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        this.active = false
      }
    })
  },
  beforeDestroy() {
    window.removeEventListener('scroll', this.handleScroll, { passive: true })
  },
  methods: {
    get(chart, value) {
      return this.charts[chart][value]
    },
    toggleMenu() {
      this.active = !this.active
    },
    hideMenu() {
      this.active = false
    },
    handleScroll() {
      let footer = document.querySelector('footer').getBoundingClientRect()
      let bottom = window.innerHeight - footer.top
      this.visible = bottom < -50
    }
  },
}
</script>

<style lang="scss">
@mixin nav-break {
  @media only screen and (min-width: 850px) {
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
    display: flex;
    align-items: center;

    &:not(:last-child) {
      border-bottom: 1px solid #d8d8d8;
    }

    &:last-child {
      padding-bottom: 16px;
    }

    .float-nav-icon {
      width: 15px;
      height: 15px;
      margin-right: 8px;

      &.graph {
        background-image: url('../assets/svg/graf.svg');
      }

      &.column {
        background-image: url('../assets/svg/stolpicni.svg');
      }

      &.map {
        background-image: url('../assets/svg/zemljevid.svg');
      }
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
  z-index: 1097;
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
.fade-leave-active {
  transition: 0.3s;
}
.slide-enter-active,
.fade-enter-active {
  transition: 0.5s;
}
.slide-enter {
  transform: translate3d(100%, 0, 0);
  will-change: transform;
}
.slide-leave-to {
  transform: translate3d(110%, 0, 0);
  will-change: transform;
}
</style>
