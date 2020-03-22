<template>
  <b-container>
    <b-row>
      <span v-html="staticContent"></span>
    </b-row>
  </b-container>
</template>

<script>
export default {
  name: "StaticPage",
  props: {
    name: String,
    content: Promise
  },
  data() {
    return {
      staticContent: ""
    };
  },
  mounted() {
    let vm = this;
    this.content.then(data => {
      vm.staticContent = data.default;
      vm.$forceUpdate();
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
