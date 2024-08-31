import Vue from 'vue'
import VueRouter from 'vue-router'
import VueMeta from 'vue-meta'
import i18next from 'i18next'

import StaticPage from './pages/StaticPage.vue'
import StatsPage from './pages/StatsPage.vue'
import PageNotFound from './pages/PageNotFound.vue'
import FAQPage from './pages/FAQPage.vue'

import aboutMdSl from './content/sl/about.md'
import aboutMdEn from './content/en/about.md'
import aboutMdHr from './content/hr/about.md'
import aboutMdDe from './content/de/about.md'
import aboutMdIt from './content/it/about.md'
import aboutMdEs from './content/es/about.md'

import dataMdSl from './content/sl/data.md'
import dataMdEn from './content/en/data.md'
import dataMdHr from './content/hr/data.md'
import dataMdDe from './content/de/data.md'
import dataMdIt from './content/it/data.md'
import dataMdEs from './content/es/data.md'

import modelsMdSl from './content/sl/models.md'
import modelsMdEn from './content/en/models.md'
import modelsMdHr from './content/hr/models.md'
import modelsMdDe from './content/de/models.md'
import modelsMdIt from './content/it/models.md'
import modelsMdEs from './content/es/models.md'

Vue.use(VueRouter)
Vue.use(VueMeta)

const mdContent = {
  about: {
    sl: aboutMdSl,
    en: aboutMdEn,
    hr: aboutMdHr,
    de: aboutMdDe,
    it: aboutMdIt,
    es: aboutMdEs,
  },
  data: {
    sl: dataMdSl,
    en: dataMdEn,
    hr: dataMdHr,
    de: dataMdDe,
    it: dataMdIt,
    es: dataMdEs,
  },
  models: {
    sl: modelsMdSl,
    en: modelsMdEn,
    hr: modelsMdHr,
    de: modelsMdDe,
    it: modelsMdIt,
    es: modelsMdEs,
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
      name: key,
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
    path: '/posts',
    redirect: `/${i18next.language}/posts`,
  },
  {
    path: '/posts/:postId',
    redirect: `/${i18next.language}/posts/:postId`,
  },
  {
    path: '/embed',
    redirect: `/${i18next.language}/embed`,
  },
  {
    path: '/podpri', // Friendly redirect
    redirect: `/${i18next.language}/donate`,
  },
  {
    path: '/donate',
    redirect: `/${i18next.language}/donate`,
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
      next({ path: i18next.language, replace: true })
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
        name: 'stats',
        component: StatsPage,
      },
      {
        path: 'world',
        name: 'world',
        component: () => import(/* webpackChunkName: "World.route" */ './pages/WorldStatsPage.vue'),
      },
      {
        path: 'tables',
        name: 'tables',
        component: () => import(/* webpackChunkName: "Tables.route" */ './pages/TablesPage.vue'),
      },
      {
        path: 'embed',
        name: 'embed',
        component: () => import(/* webpackChunkName: "Embed.route" */ './pages/EmbedMakerPage.vue'),
      },
      {
        path: 'restrictions',
        name: 'restrictions',
        component: () => import(/* webpackChunkName: "Restrictions.route" */ './pages/RestrictionsPage.vue'),
      },
      {
        path: 'faq-vaccines',
        name: 'faq-vaccines',
        component: () => import(/* webpackChunkName: "FAQVaccinesPage.route" */ './pages/FAQVaccinesPage.vue'),
        // component: FAQVaccinesPage
      },
      {
        path: 'posts',
        name: 'posts',
        component: () => import(/* webpackChunkName: "Posts.route" */ './pages/PostsPage.vue'),
      },
      {
        path: 'posts/:postId',
        name: 'post',
        component: () => import(/* webpackChunkName: "Post.route" */ './pages/PostSingle.vue'),
      },
      {
        path: 'donate',
        name: 'donate',
        component: () => import(/* webpackChunkName: "Donation.route" */ './pages/DonationPage.vue'),
      },
      {
        path: 'faq',
        name: 'faq',
        // component: import(/* webpackChunkName: "Faq.route" */ './pages/FAQPage.vue'),
        component: FAQPage
      },
      {
        path: 'podpri', // Friendly redirect
        redirect: `donate`,
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
