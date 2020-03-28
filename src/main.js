import Vue from 'vue'
import App from './App.vue'
import VueRouter from 'vue-router'
import VueMoment from 'vue-moment'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

import 'style/main.scss'
// import 'bootstrap/dist/css/bootstrap.css'
// import 'bootstrap-vue/dist/bootstrap-vue.css'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import MapPage from './pages/MapPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'

import moment from 'moment'
import 'moment/locale/sl'

moment.locale('sl')

import store from './store'

Vue.use(VueRouter)
Vue.use(BootstrapVue)
Vue.use(IconsPlugin)
Vue.use(VueMoment)

// fontawesome
library.add(faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false

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
