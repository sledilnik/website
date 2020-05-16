import Vue from 'vue'
import VueRouter from 'vue-router'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import EmbedMakerPage from './pages/EmbedMakerPage.vue'
import TablesPage from './pages/TablesPage.vue'
import DataPage from './pages/DataPage.vue'

import * as aboutMd from './content/about.md'
import * as aboutMdEn from './content/about_en.md'
import * as linksMd from './content/links.md'
import * as contentMd from './content/FAQ.md'
import * as contentMdEn from './content/FAQ_en.md'
import * as teamMd from './content/team.md'
import * as sourcesMd from './content/sources.md'
import * as modelsMd from './content/models.md'
import * as datasourcesMd from './content/datasources.md'

Vue.use(VueRouter)

function dynamicProps(route) {
  let baseRoute = route.path.slice(4)
  let lang = route.params.lang
  let langSl = lang === 'sl'

  return typeof lang === undefined || langSl
    ? {
        name: `${baseRoute}`,
        content: getContent(),
      }
    : {
        name: `${baseRoute}-${route.params.lang}`,
        content: getContent(),
      }

  function getContent() {
    switch (baseRoute) {
      case 'FAQ':
        return langSl ? contentMd : contentMdEn
      case 'about':
        return langSl ? aboutMd : aboutMdEn
    }
  }
}

const routes = [
  {
    path: '/:lang',
    beforeEnter: (to, from, next) => {
      const language = to.params.lang 
      const supportedLanguages = ['sl', 'en']
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
        path: 'about',
        component: StaticPage,
        props: dynamicProps,
      },
      {
        path: 'stats',
        component: StatsPage,
        props: true,
      },
      {
        path: 'data',
        component: DataPage,
        props: true,
      },
      {
        path: 'tables',
        component: TablesPage,
        props: true,
      },
      {
        path: 'links',
        component: StaticPage,
        props: {
          name: 'links',
          content: linksMd,
        },
      },
      {
        path: 'FAQ',
        component: StaticPage,
        props: dynamicProps,
      },
      {
        path: 'team',
        component: StaticPage,
        props: {
          name: 'team',
          content: teamMd,
        },
      },
      {
        path: 'sources',
        component: StaticPage,
        props: {
          name: 'sources',
          content: sourcesMd,
        },
      },
      {
        path: 'models',
        component: StaticPage,
        props: {
          name: 'sources',
          content: modelsMd,
        },
      },
      {
        path: 'datasources',
        component: StaticPage,
        props: {
          name: 'datasources',
          content: datasourcesMd,
        },
      },
      {
        path: 'embed',
        component: EmbedMakerPage,
        props: true,
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
      next({ path: '/sl/stats' })
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
