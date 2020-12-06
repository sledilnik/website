<template>
  <div>
    <div class="card mb-3" style="max-width: 540px;">
      <div class="d-flex">
        <div class="col-md-4 px-0" v-if="post.image">
          <img :src="post.image" class="card-img" :alt="post.title" />
        </div>
        <div class="col-md-8">
          <div class="card-body">
            <h5 class="card-title">{{ post.title }}</h5>
            <p class="card-text" v-html="post.blurb"></p>
            <p class="card-text">
              <small class="text-muted">Objavljeno {{ created }}</small>
            </p>
            <a v-if="post.link_to" :href="post.link_to" target="_blank">Preberi več</a>
            <router-link v-else :to="postLink">Preberi več</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import i18next from "i18next";
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
      return `/${i18next.language}/posts/${this.post.id}`;
    },
  },
};
</script>

<style lang="scss"></style>
