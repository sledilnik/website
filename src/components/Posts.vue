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
