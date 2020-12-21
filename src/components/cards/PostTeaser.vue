<template>
  <div class="post__teaser">
    <b-card no-body class="card overflow-hidden">
      <div class="d-flex h-100 flex-column flex-lg-row h-100">
        <div class="col-lg-4 h-100 d-block py-2 pl-2 pr-0">
          <a class="h-100" v-if="post.link_to" :href="post.link_to" target="_blank">
            <div
              class="teaser__image"
              v-bind:style="{ 'background-image': `url(${post.image_thumb})` }"
            ></div>
          </a>
          <router-link v-else :to="postLink">
            <div
              class="teaser__image"
              v-bind:style="{ 'background-image': `url(${post.image_thumb})` }"
            ></div>
          </router-link>
        </div>
        <div class="col-lg-8 p-0">
          <b-card-body :title="post.title">
            <div
              class="text-muted card-text"
              v-html="$options.filters.marked(post.blurb)"
            ></div>
            <div class="link small">
              <a v-if="post.link_to" class="stretched-link" :href="post.link_to" target="_blank">Preberi več</a>
              <router-link v-else class="stretched-link" :to="postLink">Preberi več</router-link>
            </div>
          </b-card-body>
        </div>
      </div>
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
    postLink() {
      return {
        name: "post",
        params: {
          postId: this.post.id,
        },
      };
    },
  },
};
</script>

<style lang="sass">
.post__teaser
  .card-title
    font-size: 18px
    line-height: 24px

  .card
    height: 100%
    border-radius: 6px
    border: none
    box-shadow: $element-box-shadow

    .row
      align-items: stretch

  .card-body
    padding: 32px

    @media only screen and (max-width: 768px)
      padding: 26px

    @media only screen and (max-width: 480px)
      padding: 16px

  @media only screen and (max-width: 768px) and (min-width: 481px)
    &:first-child
      padding-right: 7.5px !important
    &:last-child
      padding-left: 7.5px !important

  @media only screen and (max-width: 480px)
    &:first-child
      padding: 0
      margin-left: 0
      margin-bottom: 15px
    &:last-child
      padding: 0
      margin-right: 0

.card-text
  font-size: 14px
  margin-bottom: 4px
  p
    display: none

  p:first-child
    display: block
    margin-bottom: 0
    overflow: hidden
    text-overflow: ellipsis
    display: -webkit-box
    -webkit-line-clamp: 2
    -webkit-box-orient: vertical

.teaser__image
  min-height: 130px
  margin-right: 0
  height: 100%
  background-size: cover
  background-repeat: no-repeat
  background-position: bottom center

</style>
