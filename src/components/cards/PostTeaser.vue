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
          <a :href="linkTo">Preberi več</a>
        </div>
      </div>
    </div>
  </div>
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
    linkTo(){
      return this.post.link_to || `/objave/${this.post.id}`
    }
  },
};
</script>

<style lang="scss"></style>
