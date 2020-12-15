import Vue from 'vue'
import router from './router'
import VueScrollTo from 'vue-scrollto'
import VScrollLock from 'v-scroll-lock'
import i18n from './i18n'
import App from './App.vue'
import store from './store/index'
import plugins from './plugins/index'
import './main.js'
import 'style/index.scss'
import VueDateFns from "vue-date-fns";
import VueFuse from 'vue-fuse';

Vue.use(plugins);
Vue.use(VueScrollTo, { offset: 60 });
Vue.use(VScrollLock);
Vue.use(VueDateFns);
Vue.use(VueFuse);

new Vue({
  render: (h) => h(App),
  router,
  store,
  i18n,
}).$mount('#app')
