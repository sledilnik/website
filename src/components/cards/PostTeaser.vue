<template>
  <div>
    <b-card
      :key="post.id"
      :title="post.title"
      :img-src="post.image"
      :img-alt="post.title"
      img-top
      tag="article"
    >
      <b-card-text>{{ post.blurb }}</b-card-text>
      <a v-if="post.link_to" :href="post.link_to" target="_blank">Preberi več</a>
      <router-link v-else :to="postLink">Preberi več</router-link>
      <template #footer>
        <small class="text-muted">{{ created }}</small>
      </template>
    </b-card>
  </div>
</template>
<script>
import { format, parseISO } from "date-fns";
export default {
  props: {
    post: Object,
  },
  computed: {
    created() {
      return format(parseISO(this.post.created), "d.M.Y, H:mm");
    },
    postLink() {
      return {
        name: 'post',
        params: {
          postId: this.post.id
        }
      };
    },
  },
};
</script>

<style lang="scss" scoped>
.card {
  margin-bottom: 30px;
}
</style>