<template>
  <div id="app">
    <Navbar v-if="!embed" />
    <main>
      <router-view :key="$route.path" />
    </main>
  </div>
</template>

<script>
import Navbar from "./components/Navbar.vue";

export default {
  name: "app",
  props: {
    embed: {
      default: false,
      type: Boolean
    }
  },
  components: {
    Navbar
  },
  created() {
    this.$store.dispatch("stats/fetchData");
    this.$store.dispatch("hospitals/fetchData");
    this.$store.dispatch("stats/refreshDataEvery", 30);
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
