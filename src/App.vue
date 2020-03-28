<template>
  <div id="app">
    <Navbar />
    <router-view :key="$route.path" />
  </div>
</template>

<script>
import Navbar from "./components/Navbar.vue";

export default {
  name: "app",
  components: {
    Navbar
  },
  created(){
    this.$store.dispatch('fetchData')
  }
};
</script>

<style lang="scss">
</style>
