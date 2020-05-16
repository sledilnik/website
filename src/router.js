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

// TODO: refactor these two ugly functions
function dynamicPropsAbout(route) {
  console.log('lang', route.params.lang)

  if (typeof route.params.lang === undefined || route.params.lang === 'sl') {
    return {
      name: 'about',
      content: aboutMd,
    }
  } else {
    return {
      name: 'about-' + route.params.lang,
      content: aboutMdEn,
    }
  }
}

function dynamicPropsFaq(route) {
  if (typeof route.params.lang === undefined || route.params.lang === 'sl') {
    return {
      name: 'FAQ',
      content: contentMd,
    }
  } else {
    return {
      name: 'FAQ-' + route.params.lang,
      content: contentMdEn,
    }
  }
}

const routes = [
  {
    path: '/:lang',
    beforeEnter: (to, from, next) => {
      if (to.params.lang) {
        i18next.changeLanguage(to.params.lang)
      }
      next()
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
        props: dynamicPropsAbout,
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
        props: dynamicPropsFaq,
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
        path: '/embed',
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
