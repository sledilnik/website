import Vue from 'vue'
import VueRouter from 'vue-router'

import App from './App.vue'
import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import MapPage from './pages/MapPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'

import store from 'store'

import '@/main.js'
import 'style/index.scss'

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
      content: import('./content/about.md')
    }
  },
  {
    path: '/stats',
    component: StatsPage
  },
  {
    path: '/data',
    component: DataPage
  },
  {
    path: '/map',
    component: MapPage
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
      content: import('./content/links.md')
    }
  },
  {
    path: '/team',
    component: StaticPage,
    props: {
      name: 'team',
      content: import('./content/team.md')
    }
  },
  {
    path: '/sources',
    component: StaticPage,
    props: {
      name: 'sources',
      content: import('./content/sources.md')
    }
  },
  {
    path: '/datasources',
    component: StaticPage,
    props: {
      name: 'datasources',
      content: import('./content/datasources.md')
    }
  },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
  scrollBehavior() {
    // possible arguments to, from, savedPosition - removed, so lint does not complain
    return { x: 0, y: 0 }
  },
})

new Vue({
  render: h => h(App),
  router,
  store,
}).$mount('#app')
