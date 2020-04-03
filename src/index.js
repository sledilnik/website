import Vue from 'vue'
import VueRouter from 'vue-router'
import VueScrollTo from 'vue-scrollto'
import { sync } from 'vuex-router-sync'

import App from './App.vue'
import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import EmbedMakerPage from './pages/EmbedMakerPage.vue'
import AdvancedStatsPage from './pages/AdvancedStatsPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'

import store from './store'

import '@/main.js'
import 'style/index.scss'

import * as aboutMd from './content/about.md'
import * as aboutMdEn from './content/about_en.md'
import * as linksMd from './content/links.md'
import * as contentMd from './content/FAQ.md'
import * as contentMdEn from './content/FAQ_en.md'
import * as teamMd from './content/team.md'
import * as sourcesMd from './content/sources.md'
import * as modelsMd from './content/models.md'
import * as datasourcesMd from './content/datasources.md'

Vue.use(VueScrollTo)

const routes = [
  {
    path: '*',
    redirect: '/stats'
  },
  {
    path: '/about',
    component: StaticPage,
    props: {
      name: 'about',
      content: aboutMd,
    }
  },
  {
    path: '/about/en',
    component: StaticPage,
    props: {
      name: 'about-en',
      content: aboutMdEn,
    }
  },
  {
    path: '/stats',
    component: StatsPage
  },
  {
    path: '/advancedstats',
    component: AdvancedStatsPage
  },
  {
    path: '/data',
    component: DataPage
  },
  {
    path: '/tables',
    component: TablesPage
  },
  {
    path: '/links',
    component: StaticPage,
    props: {
      name: 'links',
      content: linksMd
    }
  },
  {
    path: '/FAQ',
    component: StaticPage,
    props: {
      name: 'FAQ',
      content: contentMd
    }
  },
  {
    path: '/FAQ/en',
    component: StaticPage,
    props: {
      name: 'FAQ-en',
      content: contentMdEn
    }
  },
  {
    path: '/team',
    component: StaticPage,
    props: {
      name: 'team',
      content: teamMd
    }
  },
  {
    path: '/sources',
    component: StaticPage,
    props: {
      name: 'sources',
      content: sourcesMd
    }
  },
  {
    path: '/models',
    component: StaticPage,
    props: {
      name: 'sources',
      content: modelsMd
    }
  },
  {
    path: '/datasources',
    component: StaticPage,
    props: {
      name: 'datasources',
      content: datasourcesMd
    },
  },
  {
    path: '/embed',
    component: EmbedMakerPage,
  },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
})

sync(store, router)
new Vue({
  render: h => h(App),
  router,
  store,
}).$mount('#app')
