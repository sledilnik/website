import Vue from 'vue'
import VueRouter from 'vue-router'

import App from './App.vue'
import TablesPage from './pages/TablesPage.vue'
import InfoCardEmbed from '@/embed/InfoCard'
import ChartEmbed from '@/embed/Chart'

import store from 'store'

import '@/main.js'
import 'style/embed.scss'

const routes = [
  {
    path: '/tables',
    component: TablesPage
  },
  {
    path: '/card/:type',
    component: InfoCardEmbed
  },
  {
    path: '/chart/:type',
    component: ChartEmbed
  },
  // primer route brez $route.params
  // {
  //   path: '/chart',
  //   component: ChartEmbed
  // },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
})

document.addEventListener('chartLoaded', function () {
  const elm = document.getElementsByTagName("main")[0]
  const height = +elm.offsetHeight + 10
  // console.log("chart je naloadan", elm.offsetHeight, height, window.name)
  window.top.postMessage(
    {
      type: "embed-size",
      height: height,
      name: window.name
    },
    "*"
  );
}, false);

new Vue({
  render: h => h(App, { props: { embed: true } }),
  router,
  store,
}).$mount('#app')
