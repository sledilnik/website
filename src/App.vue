<template>
  <div id="app">
    <Navbar />
    <main>
      <router-view :key="$route.path" />
    </main>
  </div>
</template>

<script>
import Navbar from "./components/Navbar.vue";

export default {
  name: "app",
  components: {
    Navbar
  },
  created() {
    this.$store.dispatch("fetchData");
  }
};
</script>

<style lang="scss">
main {
  padding-top: 77px;
  background: $background;
}
</style>
