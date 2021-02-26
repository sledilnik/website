<template>
  <div id="app" :class="[embed ? $route.path.slice(7) : $route.path.slice(4)]">
    <Navbar v-if="!embed" />
    <main>
      <router-view :key="$route.path" />
    </main>
    <Footer v-if="!embed" />
  </div>
</template>

<script>
import Navbar from './components/Navbar.vue'
import Footer from './components/Footer.vue'

export default {
  name: 'app',
  metaInfo() {
    var pathWithoutLanguage = this.$route.path.slice(4).toLowerCase().replace(/\/$/, "");
    var links = [
        {rel: 'canonical', href: `${process.env.VUE_APP_URL}/${this.$i18n.i18next.language}/${pathWithoutLanguage}`},
        {rel: 'alternate', hreflang: "x-default", href: `${process.env.VUE_APP_URL}/${pathWithoutLanguage}`},
      ];
    this.$i18n.i18next.languages.forEach(lang => {
      links = links.concat({rel: 'alternate', hreflang: `${lang}`, href: `${process.env.VUE_APP_URL}/${lang}/${pathWithoutLanguage}`})
    });

    return {
      htmlAttrs: {
        lang: this.$i18n.i18next.language,
      },
      title: this.$t('meta.title'),
      meta: [
        { vmid: 'description', name: 'description', content: this.$t('meta.description') },
        { vmid: 'og:title', property: 'og:title', content: this.$t('meta.title') },
        { vmid: 'og:description', property: 'og:description', content: this.$t('meta.description') },
      ],
      link: links,
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
    if (Object.keys(this.$route.query).length > 0 && this.$route.query.showDate) {
      let date = this.$route.query.showDate
      this.$store.dispatch("patients/fetchData", date)
      this.$store.dispatch('stats/fetchSummary', date)
    } else {
      this.$store.dispatch('stats/fetchSummary')
      this.$store.dispatch("patients/fetchData")
    }
  },
  mounted() {
    if (this.$route.hash) {
      const checker = setInterval(() => {
        const elm = document.querySelector(this.$route.hash)
        if (elm) {
          // element found on page
          clearInterval(checker)

          const offset = -90

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

.visualization-chart {
  -webkit-filter: grayscale(100%) blur(0.8px) brightness(100%) contrast(110%);
  filter: grayscale(100%) blur(0.8px) brightness(100%) contrast(110%);

  // ignore all mouse events (click, hover...)
  pointer-events: none;

  // prevent text selection
  -webkit-touch-callout: none; /* iOS Safari */
  -webkit-user-select: none; /* Safari */
   -khtml-user-select: none; /* Konqueror HTML */
     -moz-user-select: none; /* Old versions of Firefox */
      -ms-user-select: none; /* Internet Explorer/Edge */
          user-select: none; /* Non-prefixed version, currently
                                supported by Chrome, Edge, Opera and Firefox */
}
</style>
