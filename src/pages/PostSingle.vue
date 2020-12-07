<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <template v-if="post">
        <h1>{{ post.title }}</h1>
        <div class="info-text">
          <span v-if="post.author" class="author">{{ post.author }} · </span>
          <span class="date-time">{{ $t("posts.timestamp", { date }) }}</span>
          <reading-time :content="post.body"></reading-time>
        </div>
        <div class="content" v-html="post.body"></div>
        <div class="btn-wrapper">
          <a class="btn" @click="goBack">{{ $t("pageNotFound.back") }}</a>
        </div>
      </template>
      <Loader v-else />
    </div>
  </div>
</template>

<script>
import _ from "lodash";
import Loader from "components/Loader";
import ReadingTime from "components/ReadingTime";

export default {
  components: {
    Loader,
    ReadingTime
  },
  data() {
    return {
      post: false,
    };
  },
  metaInfo() {
    //TODO
  },
  computed: {
    date() {
      return new Date(this.post.created);
    },
  },
  async created() {
    const postId = _.get(this.$route, "params.postId", false);
    if (!postId) {
      // 404
    }
    this.post = await this.contentApi.get(`/posts/${postId}`);
  },
  methods: {
    goBack() {
      return this.$router.go(-1);
    }
  }
};
</script>

<style lang="sass">
.content img
  max-width: 100%
  height: auto
</style>

<style scoped lang="scss">
.info-text {
  font-size: 14px;
  color: rgba(117, 117, 117, 1);
}

.content {
  margin-top: 30px;
}

.btn-wrapper {
  margin-top: 30px;
}

.btn {
  display: inline-block;
  color: #000;
  font-size: 14px;
  line-height: 16px;
  font-weight: bold;
  border-radius: 6px;
  padding: 11px 12px;
  border: solid 1px rgba(0, 0, 0, 0.13);
  cursor: pointer;
  background: $yellow;
  border: none;

  &:hover {
    text-decoration: none;
  }
}
</style>
