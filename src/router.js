import Vue from 'vue'
import VueRouter from 'vue-router'
import VueMeta from 'vue-meta'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import PageNotFound from './pages/PageNotFound.vue'
import OstaniZdravPage from './pages/OstaniZdravPage.vue'

import * as aboutMdSl from './content/sl/about.md'
import * as aboutMdEn from './content/en/about.md'
import * as aboutMdHr from './content/hr/about.md'
import * as aboutMdDe from './content/de/about.md'
import * as aboutMdIt from './content/it/about.md'

import * as contentMdSl from './content/sl/faq.md'
import * as contentMdEn from './content/en/faq.md'
import * as contentMdHr from './content/hr/faq.md'
import * as contentMdDe from './content/de/faq.md'
import * as contentMdIt from './content/it/faq.md'

import * as dataMdSl from './content/sl/data.md'
import * as dataMdEn from './content/en/data.md'
import * as dataMdHr from './content/hr/data.md'
import * as dataMdDe from './content/de/data.md'
import * as dataMdIt from './content/it/data.md'

import * as modelsMdSl from './content/sl/models.md'
import * as modelsMdEn from './content/en/models.md'
import * as modelsMdHr from './content/hr/models.md'
import * as modelsMdDe from './content/de/models.md'
import * as modelsMdIt from './content/it/models.md'

Vue.use(VueRouter)
Vue.use(VueMeta)

const mdContent = {
  faq: {
    sl: contentMdSl,
    en: contentMdEn,
    hr: contentMdHr,
    de: contentMdDe,
    it: contentMdIt,
  },
  about: {
    sl: aboutMdSl,
    en: aboutMdEn,
    hr: aboutMdHr,
    de: aboutMdDe,
    it: aboutMdIt,
  },
  data: {
    sl: dataMdSl,
    en: dataMdEn,
    hr: dataMdHr,
    de: dataMdDe,
    it: dataMdIt,
  },
  models: {
    sl: modelsMdSl,
    en: modelsMdEn,
    hr: modelsMdHr,
    de: modelsMdDe,
    it: modelsMdIt,
  },
}

function dynamicProps(route) {
  let baseRoute = route.path
    .slice(4)
    .toLowerCase()
    .replace(/\/$/, '')
  let lang = route.params.lang

  return {
    name: lang === 'en' ? `${baseRoute}-${lang}` : `${baseRoute}`,
    content: mdContent[baseRoute][lang || 'sl'],
  }
}

function mdContentRoutes() {
  const mdContentRoutes = []

  Object.keys(mdContent).forEach((key) => {
    mdContentRoutes.push({
      path: key,
      component: StaticPage,
      props: dynamicProps,
    })
  })

  return mdContentRoutes
}

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
    path: '/embed',
    redirect: `/${i18next.language}/embed`,
  },
  {
    path: '/ostanizdrav',
    redirect: `/${i18next.language}/ostanizdrav`,
  },
  {
    path: '/links', // Retired page
    redirect: `/${i18next.language}/about`,
  },
  {
    path: '/team', // Retired page
    redirect: `/${i18next.language}/about`,
  },
  {
    path: '/sources', // Retired page
    redirect: `/${i18next.language}/data`,
  },
  {
    path: '/datasources', // Retired page
    redirect: `/${i18next.language}/data`,
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
        path: 'tables',
        component: () => import(/* webpackChunkName: "tables" */'./pages/TablesPage.vue'),
      },
      {
        path: 'embed',
        component: () => import('./pages/EmbedMakerPage.vue'),
      },
      {
        path: 'restrictions',
        component: () => import('./pages/RestrictionsPage.vue'),
      },
      {
        path: 'links', // Retired page
        redirect: `about`,
      },
      {
        path: 'team', // Retired page
        redirect: `about`,
      },
      {
        path: 'sources', // Retired page
        redirect: `data`,
      },
      {
        path: 'datasources', // Retired page
        redirect: `data`,
      },
      ...mdContentRoutes(),
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
