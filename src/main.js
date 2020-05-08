import Vue from 'vue'
import Vuex from 'vuex'
import VueRouter from 'vue-router'
import HighchartsVue from 'highcharts-vue'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight, faSpinner } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

import 'whatwg-fetch'
// global vue filters
import './filters'

// this is common Vue initialization for index.js and embed.js (app entrypoints)

Vue.use(Vuex)
Vue.use(VueRouter)
Vue.use(BootstrapVue)
Vue.use(IconsPlugin)
Vue.use(HighchartsVue)

// fontawesome
library.add(faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight, faSpinner)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false
