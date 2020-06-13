import Vue from 'vue'
import VueRouter from 'vue-router'
import VueMeta from 'vue-meta'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import EmbedMakerPage from './pages/EmbedMakerPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'

import * as aboutMd from './content/about.md'
import * as aboutMdEn from './content/about_en.md'
import * as aboutMdMk from './content/about_mk.md'
import * as aboutMdSq from './content/about_sq.md'
import * as linksMd from './content/links.md'
import * as linksMdEn from './content/links_en.md'
import * as linksMdMk from './content/links_mk.md'
import * as linksMdSq from './content/links_sq.md'
import * as contentMd from './content/faq.md'
import * as contentMdEn from './content/faq_en.md'
import * as contentMdMk from './content/faq_mk.md'
import * as contentMdSq from './content/faq_sq.md'
import * as teamMd from './content/team.md'
import * as teamMdEn from './content/team_en.md'
import * as teamMdMk from './content/team_mk.md'
import * as teamMdSq from './content/team_sq.md'
import * as sourcesMd from './content/sources.md'
import * as sourcesMdEn from './content/sources_en.md'
import * as sourcesMdMk from './content/sources_mk.md'
import * as sourcesMdSq from './content/sources_sq.md'
import * as modelsMd from './content/models.md'
import * as modelsMdEn from './content/models_en.md'
import * as modelsMdMk from './content/models_mk.md'
import * as modelsMdSq from './content/models_sq.md'
import * as datasourcesMd from './content/datasources.md'
import * as datasourcesMdEn from './content/datasources_en.md'
import * as datasourcesMdMk from './content/datasources_mk.md'
import * as datasourcesMdSq from './content/datasources_sq.md'

Vue.use(VueRouter)
Vue.use(VueMeta)

const mdContent = {
  faq: {
    sl: contentMd,
    en: contentMdEn,
    mk: contentMdMk,
    sq: contentMdSq,
  },
  about: {
    sl: aboutMd,
    en: aboutMdEn,
    mk: aboutMdMk,
    sq: aboutMdSq,
  },
  team: {
    sl: teamMd,
    en: teamMdEn,
    mk: teamMdMk,
    sq: teamMdSq,
  },
  links: {
    sl: linksMd,
    en: linksMdEn,
    mk: linksMdMk,
    sq: linksMdSq,
  },
  sources: {
    sl: sourcesMd,
    en: sourcesMdEn,
    mk: sourcesMdMk,
    sq: sourcesMdSq,
  },
  models: {
    sl: modelsMd,
    en: modelsMdEn,
    mk: modelsMdMk,
    sq: modelsMdSq,
  },
  datasources: {
    sl: datasourcesMd,
    en: datasourcesMdEn,
    mk: datasourcesMdMk,
    sq: datasourcesMdSq,
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
    path: '/:lang',
    beforeEnter: (to, from, next) => {
      const language = to.params.lang
      const supportedLanguages = i18next.languages
      if (!supportedLanguages.includes(language)) {
        return next(`${i18next.language}/stats`)
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
        path: '/',
        redirect: 'stats',
      },
      {
        path: 'stats',
        component: StatsPage,
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
      next({ path: `/${process.env.VUE_APP_DEFAULT_LANGUAGE}/stats` })
    },
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
