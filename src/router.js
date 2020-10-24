import Vue from 'vue'
import VueRouter from 'vue-router'
import VueMeta from 'vue-meta'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import TranslatableStaticPage from './pages/TranslatableStaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import PageNotFound from './pages/PageNotFound.vue'
import OstaniZdravPage from './pages/OstaniZdravPage.vue'

Vue.use(VueRouter)
Vue.use(VueMeta)

const routes = [
  {
    path: '/stats',
    redirect: `/${i18next.language}/stats`,
  },
  {
    path: '/world',
    redirect: `/${i18next.language}/world`,
  },
  {
    path: '/tables',
    redirect: `/${i18next.language}/tables`,
  },
  {
    path: '/models',
    redirect: `/${i18next.language}/models`,
  },
  {
    path: '/faq',
    redirect: `/${i18next.language}/faq`,
  },
  {
    path: '/about',
    redirect: `/${i18next.language}/about`,
  },
  {
    path: '/about/en',
    redirect: `/en/about`,
  },
  {
    path: '/team',
    redirect: `/${i18next.language}/team`,
  },
  {
    path: '/sources',
    redirect: `/${i18next.language}/sources`,
  },
  {
    path: '/links',
    redirect: `/${i18next.language}/links`,
  },
  {
    path: '/data',
    redirect: `/${i18next.language}/data`,
  },
  {
    path: '/embed',
    redirect: `/${i18next.language}/embed`,
  },
  {
    path: '/datasources',
    redirect: `/${i18next.language}/datasources`,
  },
  {
    path: '/ostanizdrav',
    redirect: `/${i18next.language}/ostanizdrav`,
  },
  {
    path: '/',
    beforeEnter: (to, from, next) => {
      next(i18next.language)
    },
  },
  {
    path: '/:lang',
    beforeEnter: (to, from, next) => {
      const language = to.params.lang
      const supportedLanguages = i18next.languages
      if (!supportedLanguages.includes(language)) {
        return next(`${i18next.language}/404`)
      }
      if (i18next.language !== language) {
        i18next.changeLanguage(language)
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
        path: '',
        redirect: 'stats',
      },
      {
        path: 'stats',
        component: StatsPage,
      },
      {
        path: 'ostanizdrav',
        component: OstaniZdravPage,
      },
      {
        path: 'world',
        component: () => import(/* webpackChunkName: "world" */'./pages/WorldStatsPage.vue'),
      },
      {
        path: 'data',
        component: () => import(/* webpackChunkName: "data" */'./pages/DataPage.vue'),
      },
      {
        path: 'tables',
        component: () => import(/* webpackChunkName: "tables" */'./pages/TablesPage.vue'),
      },
      {
        path: 'embed',
        component: () => import('./pages/EmbedMakerPage.vue'),
      },
      {
        path: 'faq',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.faq' }
      },
      {
        path: 'about',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.about' }
      },
      {
        path: 'team',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.team' }
      },
      {
        path: 'links',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.links' }
      },
      {
        path: 'sources',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.sources' }
      },
      {
        path: 'models',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.models' }
      },
      {
        path: 'datasources',
        component: TranslatableStaticPage,
        props: { localeKey: 'content.datasources' }
      },
      {
        path: '*',
        component: PageNotFound,
        // Vue Router supports meta tags, but for some reason this doesn't work
        // - https://router.vuejs.org/guide/advanced/meta.html
        // - https://alligator.io/vuejs/vue-router-modify-head/
        // meta: {
        //   metaTags: [
        //     {
        //       name: 'robots',
        //       content: 'noindex',
        //     },
        //   ],
        // },
      },
    ],
  },
  {
    path: '*',
    beforeEnter: (to, from, next) => {
      // handle legacy routes
      if (to.fullPath.substr(0, 2) === '/#') {
        const path = to.fullPath.substr(2)
        next(path)
        return
      }
      next()
    },
    component: PageNotFound,
  },
]

const router = new VueRouter({
  routes, // short for `routes: routes`
  mode: 'history',
})

router.beforeEach((to, from, next) => {
  if (to.hash === '') {
    window.scrollTo(0, 0)
  }
  next()
})

export default router
