import Vue from 'vue'
import VueRouter from 'vue-router'
import store from 'store'
import i18n from './i18n'
import i18next from 'i18next'
import moment from 'moment'

import App from './App.vue'
import TablesPage from './pages/TablesPage.vue'
import InfoCardEmbed from '@/embed/InfoCard'
import ChartEmbed from '@/embed/Chart'

import '@/main.js'
import 'style/embed.scss'

Vue.use(VueRouter)

const routes = [
  {
    path: '/tables',
    component: TablesPage,
    beforeEnter: (to, from, next) => {
      i18next.changeLanguage('sl', (err, t) => {
        if (err) return console.log('something went wrong loading', err)
        moment.locale('sl')
      })
      return next()
    },
  },
  {
    path: '/card/:type',
    component: InfoCardEmbed,
    beforeEnter: (to, from, next) => {
      i18next.changeLanguage('sl', (err, t) => {
        if (err) return console.log('something went wrong loading', err)
        moment.locale('sl')
      })
      return next()
    },
  },
  {
    path: '/chart/:type',
    component: ChartEmbed,
    beforeEnter: (to, from, next) => {
      i18next.changeLanguage('sl', (err, t) => {
        if (err) return console.log('something went wrong loading', err)
        moment.locale('sl')
      })
      return next()
    },
  },
  {
    path: '/:lang',
    beforeEnter: (to, from, next) => {
      const language = to.params.lang
      const supportedLanguages = i18next.languages
      if (!supportedLanguages.includes(language)) {
        return next()
      }
      if (i18n.language !== language) {
        i18next.changeLanguage(language, (err, t) => {
          if (err) return console.log('something went wrong loading', err)
          moment.locale(language)
        })
      }
      return next()
    },
    component: {
      render(c) {
        return c('router-view')
      },
    },
    children: [
      {
        path: 'tables',
        component: TablesPage,
      },
      {
        path: 'card/:type',
        component: InfoCardEmbed,
      },
      {
        path: 'chart/:type',
        component: ChartEmbed,
      },
      // primer route brez $route.params
      // {
      //   path: '/chart',
      //   component: ChartEmbed
      // },
    ],
  },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
})

document.addEventListener(
  'chartLoaded',
  function() {
    setTimeout(() => {
      const elm = document.getElementsByTagName('main')[0]
      const height = +elm.offsetHeight + 10
      // console.log("chart je naloadan", elm.offsetHeight, height, window.name)
      window.top.postMessage(
        {
          type: 'embed-size',
          height: height,
          name: window.name,
        },
        '*'
      )
    }, 50)
  },
  false
)

new Vue({
  render: (h) => h(App, { props: { embed: true } }),
  router,
  store,
  i18n,
}).$mount('#app')
