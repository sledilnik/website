<template>
  <div class="container">
    {{ posts }}
  </div>
</template>

<script>
  export default {
    data() {
      return {
          loading: true,
          posts: []
      }
    },
    created() {
    },
    methods: {
    }
  }
</script>
<style lang="scss" scoped>
.card-deck {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}
</style>
