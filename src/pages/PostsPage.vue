<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>Objave</h1>
      <b-card-group deck v-if="posts">
        <PostTeaser v-for="post in posts" :post="post" :key="post.id" />
      </b-card-group>
      <Loader v-else/>
    </div>
  </div>
</template>

<script>
import PostTeaser from "components/cards/PostTeaser";
import Loader from "components/Loader";

export default {
  components: {
    PostTeaser,
    Loader
  },
  metaInfo() {
    //TODO
  },
  data(){
    return {
      posts: false
    }
  },
  async created() {
    const { objects } = await this.contentApi.get("/posts");
    this.posts = objects;
  },
};
</script>

<style scoped lang="scss"></style>
