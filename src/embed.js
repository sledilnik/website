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
import 'style/embed.scss'

const routes = [
  {
    path: '/tables',
    component: TablesPage
  },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
})

new Vue({
  render: h => h(App, {props: {embed: true}}),
  router,
  store,
}).$mount('#app')
