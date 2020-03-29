<template>
  <div id="app">
    <Navbar v-if="!embed"/>
    <router-view :key="$route.path" />
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
    this.$store.dispatch("fetchData");
  }
};
</script>

<style lang="scss">
</style>
