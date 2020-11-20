// this is common Vue initialization for index.js and embed.js (app entrypoints)

import Vue from 'vue'
import { LayoutPlugin, TabsPlugin, BTable, FormTextareaPlugin, BFormSelect, TooltipPlugin, CardPlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faSpinner, faGlobe, faCaretDown } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import Trend from 'vuetrend'

import './filters'
import './directives'

// bootstrap
Vue.use(LayoutPlugin)
Vue.use(TabsPlugin)
Vue.use(FormTextareaPlugin)
Vue.use(TooltipPlugin)
Vue.use(CardPlugin)
Vue.component('b-table', BTable)
Vue.component('b-form-select', BFormSelect)

// fontawesome
library.add(faSpinner, faGlobe, faCaretDown)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.use(Trend)

Vue.config.productionTip = false
