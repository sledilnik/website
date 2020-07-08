import Vue from 'vue'
import VueRouter from 'vue-router'
import VueMeta from 'vue-meta'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import WorldStatsPage from './pages/WorldStatsPage.vue'
import EmbedMakerPage from './pages/EmbedMakerPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'
import PageNotFound from './pages/PageNotFound.vue'

import * as aboutMdSl from './content/sl/about.md'
import * as aboutMdEn from './content/en/about.md'
import * as aboutMdHr from './content/hr/about.md'

import * as linksMdSl from './content/sl/links.md'
import * as linksMdEn from './content/en/links.md'
import * as linksMdHr from './content/hr/links.md'

import * as contentMdSl from './content/sl/faq.md'
import * as contentMdEn from './content/en/faq.md'
import * as contentMdHr from './content/hr/faq.md'

import * as teamMdSl from './content/sl/team.md'
import * as teamMdEn from './content/en/team.md'
import * as teamMdHr from './content/hr/team.md'

import * as sourcesMdSl from './content/sl/sources.md'
import * as sourcesMdEn from './content/en/sources.md'
import * as sourcesMdHr from './content/hr/sources.md'

import * as modelsMdSl from './content/sl/models.md'
import * as modelsMdEn from './content/en/models.md'
import * as modelsMdHr from './content/hr/models.md'

import * as datasourcesMdSl from './content/sl/datasources.md'
import * as datasourcesMdEn from './content/en/datasources.md'
import * as datasourcesMdHr from './content/hr/datasources.md'

Vue.use(VueRouter)
Vue.use(VueMeta)

const mdContent = {
  faq: {
    sl: contentMdSl,
    en: contentMdEn,
    hr: contentMdHr,
  },
  about: {
    sl: aboutMdSl,
    en: aboutMdEn,
    hr: aboutMdHr,
  },
  team: {
    sl: teamMdSl,
    en: teamMdEn,
    hr: teamMdHr,
  },
  links: {
    sl: linksMdSl,
    en: linksMdEn,
    hr: linksMdHr,
  },
  sources: {
    sl: sourcesMdSl,
    en: sourcesMdEn,
    hr: sourcesMdHr,
  },
  models: {
    sl: modelsMdSl,
    en: modelsMdEn,
    hr: modelsMdHr,
  },
  datasources: {
    sl: datasourcesMdSl,
    en: datasourcesMdEn,
    hr: datasourcesMdHr,
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
        path: 'world',
        component: WorldStatsPage,
      },
      {
        path: 'data',
        component: DataPage,
      },
      {
        path: 'tables',
        component: TablesPage,
      },
      {
        path: 'embed',
        component: EmbedMakerPage,
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
