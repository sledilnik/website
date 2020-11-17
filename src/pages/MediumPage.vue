<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t('navbar.medium') }}</h1>
      <b-card-group deck>
        <b-card
          v-for="post in feed"
          :key="post.title"
          :title="post.title"
          :img-src="post.thumbnail"
          :img-alt="post.title"
          img-top
          tag="article"
        >
          <b-card-text>
            {{ post.description | limit(100) }}
          </b-card-text>
          <a :href="post.link">Preberi več</a>
        </b-card>
      </b-card-group>
    </div>
  </div>
</template>

<script>
import { mapState } from 'vuex'

export default {
  computed: {
    ...mapState('medium', { feed: 'feed' }),
  },
}
</script>

<style lang="scss" scoped>
.card-deck {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}

.card {
  margin-bottom: 30px;
}
</style>
