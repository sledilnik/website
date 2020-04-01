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
import Navbar from './components/Navbar.vue';
import Footer from './components/Footer.vue';

export default {
  name: 'app',
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
    this.$store.dispatch('stats/fetchData');
    this.$store.dispatch('hospitals/fetchData');
  },
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
