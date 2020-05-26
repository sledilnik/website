import Vue from 'vue'
import router from './router'
import VueScrollTo from 'vue-scrollto'
import i18n from './i18n'
import App from './App.vue'
import store from 'store'
import '@/main.js'
import 'style/index.scss'

Vue.use(VueScrollTo, { offset: 60 })

new Vue({
  render: (h) => h(App),
  router,
  store,
  i18n,
}).$mount('#app')
