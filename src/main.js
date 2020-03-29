import Vue from 'vue'
import Vuex from 'vuex'
import VueRouter from 'vue-router'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// global vue filters
import './filters'

Vue.use(Vuex)
Vue.use(VueRouter)
Vue.use(BootstrapVue)
Vue.use(IconsPlugin)

// fontawesome
library.add(faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false
