<template>
  <b-container class="page-static" :class="name">
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
    this.content.then(data => {
      this.staticContent = data.default;
      this.$forceUpdate()
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.page-static.sources {
  table {
    width: 100%;
    table-layout: fixed;
    text-align: left;
  }
}
</style>
