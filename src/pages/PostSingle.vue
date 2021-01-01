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
        <figure v-if="post.image">
          <img class="figure-img img-fluid rounded" :src="post.image">
          <figcaption v-if="post.image_caption" class="figure-caption text-right font-italic">{{ post.image_caption }}</figcaption>
        </figure>
        <div v-if="post.body" class="content" v-html="$options.filters.marked(post.body)"></div>
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
import { mapGetters } from 'vuex';

export default {
  components: {
    Loader,
    ReadingTime
  },
  data(){
    return {
      postId: undefined
    }
  },
  metaInfo() {
    //TODO
  },
  computed: {
    ...mapGetters('posts', ['postById']),
    post(){
      return this.postById(this.postId)
    },
    date() {
      return new Date(this.post.created);
    },
  },
  async created() {
    const postId = _.get(this.$route, "params.postId", false);
    if (!postId) {
      // 404
      return
    }
    this.postId = postId
    this.$store.dispatch('posts/fetchPost', postId)
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
img {
  margin-top: 30px;
  max-width: 100%;
  height: auto;
}

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
