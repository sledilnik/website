<template>
  <div class="container">
    <template v-if="post">
      <h1>{{ post.title }}</h1>
      <div v-html="post.body"></div>
    </template>
    <Loader v-else />
  </div>
</template>

<script>
import _ from "lodash";
import Loader from "components/Loader";

export default {
  components: {
    Loader,
  },
  data() {
    return {
      post: false,
    };
  },
  metaInfo() {},
  async created() {
    const postId = _.get(this.$route, "params.id", false);
    if (!postId) {
      // 404
    }
    this.post = await this.contentApi.get(`/posts/${postId}`);
  },
};
</script>

<style scoped lang="scss"></style>
