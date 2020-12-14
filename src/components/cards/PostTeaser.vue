<template>
  <div class="teaser__post">
    <b-card no-body class="card overflow-hidden">
      <b-row no-gutters class="h-100">
        <b-col lg="4" class="h-100 d-none d-lg-block py-2 pl-2">
          <a v-if="post.link_to" :href="post.link_to" target="_blank">
            <div
              class="teaser__image"
              v-bind:style="{ 'background-image': `url(${post.image})` }"
            ></div>
          </a>
          <router-link v-else :to="postLink">
            <div
              class="teaser__image"
              v-bind:style="{ 'background-image': `url(${post.image})` }"
            ></div>
          </router-link>
        </b-col>
        <b-col lg="8">
          <b-card-body :title="post.title">
            <div
              class="text-muted small card-text"
              v-html="$options.filters.marked(post.blurb)"
            ></div>
            <div class="link small">
              <a class="stretched-link" v-if="post.link_to" :href="post.link_to" target="_blank">Preberi več</a>
              <router-link class="stretched-link" v-else :to="postLink">Preberi več</router-link>
            </div>
          </b-card-body>
        </b-col>
      </b-row>
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
.teaser__post
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

  @media only screen and (max-width: 768px)
    padding-left: 7.5px
    padding-right: 7.5px

    &:first-child
      margin-left: -7.5px
    &:last-child
      margin-right: -7.5px

  @media only screen and (max-width: 480px)
    &:first-child
      padding: 0
      margin-left: 0
      margin-bottom: 15px
    &:last-child
      padding: 0
      margin-right: 0

.card-text
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
  margin-right: 0
  height: 100%
  background-size: cover
  background-repeat: no-repeat
  background-position: top left

</style>
