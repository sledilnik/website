// this is common Vue initialization for index.js and embed.js (app entrypoints)

import Vue from 'vue'
import { LayoutPlugin, TabsPlugin, BTable, FormTextareaPlugin, BFormSelect, TooltipPlugin, CardPlugin, VBVisiblePlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faSpinner, faGlobe, faCaretDown, faCreditCard, faMobileAlt, faUniversity, faFileContract } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import './filters'
import './directives'
import './store/fsharpInterop'

// bootstrap
Vue.use(LayoutPlugin)
Vue.use(TabsPlugin)
Vue.use(FormTextareaPlugin)
Vue.use(TooltipPlugin)
Vue.use(CardPlugin)
Vue.use(VBVisiblePlugin)

Vue.component('b-table', BTable)
Vue.component('b-form-select', BFormSelect)

// fontawesome
library.add(faSpinner, faGlobe, faCaretDown, faCreditCard, faMobileAlt, faUniversity, faFileContract)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false
