<template>
  <div class="container">
    <h1>Objave</h1>
    <div class="d-flex flex-wrap row" v-if="posts">
      <PostTeaser class="col-md-6" v-for="post in posts" :post="post" :key="post.id" />
    </div>
    <Loader v-else/>
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
