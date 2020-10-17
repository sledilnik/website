<template>
  <div>
    <div class="float-nav" @click="toggleMenu">
      <div class="float-nav-btn">
        <font-awesome-icon icon="directions" />
        <span class="float-nav-txt">Navigacija</span>
      </div>
    </div>
    <transition name="fade">
      <ul
        v-if="active && list.length > 0"
        class="float-list"
        v-on-clickaway="hideMenu"
      >
        <li v-for="item in list" :key="item" class="float-item">
          <a
            :href="'#' + item"
            v-scroll-to="{ element: '#' + item, offset: -100 }"
            v-on-clickaway="hideMenu"
            >{{ shortTitle(item) }}</a
          >
        </li>
      </ul>
    </transition>
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
        'metrics-comparison-chart': 'Stanje SLO',
        'spread-chart': 'Prirast',
        'daily-comparison-chart': 'Dnevi – primerjava',
        'patients-chart': 'Hospitalizirani',
        'map-chart': 'Občine – zemljevid',
        'municipalities-chart': 'Občine – primeri',
        'europe-chart': 'Stanje Evropa',
        'age-groups-trends-chart': 'Starost – delež',
        'tests-chart': 'Testiranje',
        'sources-chart': 'Vir okužb',
        'hcenters-chart': 'Zdr. domovi – obravnava',
        'infections-chart': 'Potrjeni – struktura',
        'cases-chart': 'Potrjeni – stanje',
        'age-groups-chart': 'Starost – spol',
        'rmap-chart': 'Regije – zemljevid',
        'regions-chart-100k': 'Regije – primeri',
        'world-chart': 'Stanje',
        'countries-active-chart': 'Aktivni',
        'countries-cases-chart': 'Primeri',
        'countries-deaths-chart': 'Smrti',
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

<style scoped lang="scss">
.float-list {
  position: fixed;
  bottom: 46px;
  right: 15px;
  list-style: none;
  margin: 0;
  padding: 10px 20px;
  z-index: 5;
  background: white;
  font-size: 14px;
  font-weight: bold;
  text-align: right;
  border-radius: 6px;
  box-shadow: $element-box-shadow;

  @media only screen and (min-width: 768px) {
    bottom: 70px;
    right: 32px;
  }

  .float-item {
    margin: 8px 0;
    padding: 0;

    a {
      display: block;
      cursor: pointer;
      margin: 4px;
      text-decoration: none;
      color: rgba(0, 0, 0, 0.7);
      border-radius: 2px;
    }
  }
}

.float-nav {
  position: fixed;
  bottom: 15px;
  right: 15px;
  z-index: 6;

  @media only screen and (min-width: 768px) {
    right: 32px;
    bottom: 32px;
  }

  .float-nav-btn {
    background-color: white;
    color: #212529;
    padding: 3px 8px;
    text-align: center;
    border-radius: 6px;
    border: 1px solid rgba(0, 0, 0, 0.13);
    cursor: pointer;
    display: inline-block;

    @media only screen and (min-width: 768px) {
      padding: 3px 12px 3px 4px;
    }

    .float-nav-txt {
      display: none;
      font-size: 14px;
      line-height: 24px;
      font-weight: bold;

      @media only screen and (min-width: 768px) {
        display: inline;
        margin-left: 10px;
      }
    }
  }
}

.fade-enter,
.fade-leave-to {
  opacity: 0;
  max-height: 0px;
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.4s;
  max-height: 500px;
}
</style>
