<template>
  <b-container class="page-static mt-3" :class="name">
    <b-row>
      <b-col col="12">
        <span v-html="staticContent"></span>
      </b-col>
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
      this.$forceUpdate();
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
@import "node_modules/bootstrap/scss/functions";
@import "node_modules/bootstrap/scss/variables";


.page-static {
  &.sources {
    table {
      width: 100%;
      table-layout: fixed;
      text-align: left;
    }
  }

  &.links {
    table {
      table-layout: fixed;
      text-align: left;
      td {
        width: 50%;
        border-top: 1px solid $text-muted;
      }
    }
  }
}
</style>
