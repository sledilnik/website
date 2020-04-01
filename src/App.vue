<template>
  <div id="app">
    <Navbar v-if="!embed" />
    <main>
      <router-view :key="$route.path" />
    </main>
    <Footer v-if="!embed" />
  </div>
</template>

<script>
import Navbar from "./components/Navbar.vue";
import Footer from "./components/Footer.vue";

export default {
  name: "app",
  props: {
    embed: {
      default: false,
      type: Boolean
    }
  },
  components: {
    Navbar,
    Footer
  },
  created() {
    this.$store.dispatch("stats/fetchData");
    this.$store.dispatch("hospitals/fetchData");
  },
  mounted() {
    this.$store.dispatch("stats/refreshDataEvery", 300);
    this.$store.dispatch("hospitals/refreshDataEvery", 300);
    if (this.$route.hash) {
      const checker = setInterval(() => {
        const elm = document.querySelector(this.$route.hash);
        if (elm) {
          // element found on page
          clearInterval(checker);

          let offset = -60
          // special case for charts
          if (elm.tagName === "SECTION" && this.$route.hash.endsWith("-chart")) {
            offset = -90
          }

          this.$scrollTo(document.querySelector(this.$route.hash), 500, {
            offset: offset
          });
        }
      }, 100);

      setTimeout(() => {
        clearInterval(checker)
      }, 5000);
    }
  }
};
</script>

<style lang="scss">
main {
  background: $background;
}

#app {
  background: $background;
}
</style>
