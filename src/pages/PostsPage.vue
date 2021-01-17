<template>
  <div class="custom-container">
    <div class="static-page-wrapper posts-page">
      <h1>{{ $t("navbar.posts") }}</h1>
      <b-card-group deck v-if="posts && posts.length">
        <PostTeaser v-for="post in posts" :post="post" :key="post.id" />
      </b-card-group>
      <Loader v-else />
    </div>
  </div>
</template>

<script>
import PostTeaser from "components/cards/PostTeaser";
import Loader from "components/Loader";
import { mapState } from 'vuex';

export default {
  components: {
    PostTeaser,
    Loader,
  },
  metaInfo() {
    //TODO
  },
  created(){
    this.$store.dispatch('posts/fetchAllPosts')
  },
  computed: {
    ...mapState("posts", ["posts"]),
  },
};
</script>

<style scoped lang="sass">
.post__teaser
  width: 100%
  margin-bottom: 30px

  @media only screen and (max-width: 768px)
    padding: 0
    margin-bottom: 32px
    &:first-child
      margin-left: 0px
    &:last-child
      margin-right: 0px
</style>

<style lang="sass">
.posts-page
  .card-text
    p,
    p:first-child
      display: block
      margin-bottom: 28px
</style>
