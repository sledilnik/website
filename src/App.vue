<template>
  <div id="app" :class="$route.path.slice(4)">
    <Navbar v-if="!embed" />
    <main>
      <router-view :key="$route.path" />
    </main>
    <Footer v-if="!embed" />
  </div>
</template>

<script>
import moment from 'moment'
import Navbar from './components/Navbar.vue'
import Footer from './components/Footer.vue'

export default {
  name: 'app',
  metaInfo() {
    return {
      htmlAttrs: {
        lang: this.$i18n.i18next.language,
      },
      title: this.$t('meta.title'),
      meta: [
        { name: 'description', content: this.$t('meta.description') },
        { name: 'twitter:title', content: this.$t('meta.title') },
        { name: 'twitter:description', content: this.$t('meta.description') },
        { property: 'og:title', content: this.$t('meta.title') },
        { property: 'og:description', content: this.$t('meta.description') },
      ],
      link: [
        {rel: 'canonical', href: "https://covid-19.sledilnik.org/"+this.$i18n.i18next.language+"/"+this.$route.path.slice(4).toLowerCase().replace(/\/$/, "")},
        {rel: 'alternate', hreflang: "sl", href: "https://covid-19.sledilnik.org/sl/"+this.$route.path.slice(4).toLowerCase().replace(/\/$/, "")},
        {rel: 'alternate', hreflang: "en", href: "https://covid-19.sledilnik.org/en/"+this.$route.path.slice(4).toLowerCase().replace(/\/$/, "")},
        {rel: 'alternate', hreflang: "x-default", href: "https://covid-19.sledilnik.org/en/"+this.$route.path.slice(4).toLowerCase().replace(/\/$/, "")},
      ],
    }
  },
  props: {
    embed: {
      default: false,
      type: Boolean,
    },
  },
  components: {
    Navbar,
    Footer,
  },
  created() {
    this.$store.dispatch("stats/fetchData");
    this.$store.dispatch("hospitals/fetchData");
    this.$store.dispatch("patients/fetchData");
  },
  mounted() {
    this.$store.dispatch("stats/refreshDataEvery", 300);
    this.$store.dispatch("hospitals/refreshDataEvery", 300);
    this.$store.dispatch("patients/refreshDataEvery", 300);
    
    moment.locale(this.$i18n.i18next.language)

    if (this.$route.hash) {
      const checker = setInterval(() => {
        const elm = document.querySelector(this.$route.hash)
        if (elm) {
          // element found on page
          clearInterval(checker)

          let offset = -60
          // special case for charts
          if (
            elm.tagName === 'SECTION' &&
            this.$route.hash.endsWith('-chart')
          ) {
            offset = -90
          }

          this.$scrollTo(document.querySelector(this.$route.hash), 500, {
            offset: offset,
          })
        }
      }, 100)

      setTimeout(() => {
        clearInterval(checker)
      }, 5000)
    }
  },
}
</script>

<style lang="scss">
main {
  background: $background;
}

#app {
  background: $background;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}
</style>
