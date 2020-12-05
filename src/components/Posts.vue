<template>
  <div class="container">
    {{ posts }}
  </div>
</template>

<script>
  import ApiService from '../services/api.service'

  export default {
    data() {
      return {
          loading: true,
          posts: []
      }
    },
    created() {
      this.dispatch()
    },
    methods: {
      async dispatch() {
        this.loading = false
        this.posts = await ApiService.get('http://127.0.0.1:8000/api/v1/posts/set/1;2/')
      }
    }
  }
</script>
