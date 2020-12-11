<template>
  <div class="post">
    <b-card no-body class="overflow-hidden">
      <b-row no-gutters>
        <b-col lg="4" class="d-none d-lg-block">
          <a v-if="post.link_to" :href="post.link_to">
            <div class="image" v-bind:style="{'background-image': `url(${post.image})`}"></div>
          </a>
          <router-link v-else :to="postLink">
            <div class="image" v-bind:style="{'background-image': `url(${post.image})`}"></div>
          </router-link>
        </b-col>
        <b-col lg="8">
          <b-card-body :title="post.title">
            <div class="text-muted small card-text" v-html="$options.filters.marked(post.blurb)"></div>
            <div class="link small">
            <a v-if="post.link_to" :href="post.link_to">Preberi več</a>
            <router-link v-else :to="postLink">Preberi več</router-link>
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
        name: 'post',
        params: {
          postId: this.post.id
        }
      };
    },
  },
};
</script>

<style lang="sass" scoped>
.post

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

.card
  height: 100%
  border-radius: 6px
  border: none
  box-shadow: $element-box-shadow

  .row
    align-items: stretch

.image
  margin: 8px
  margin-right: 0
  height: calc(100% - 16px)
  background-size: cover
  background-repeat: no-repeat
  background-position: top left

.card-body
  padding: 32px

  @media only screen and (max-width: 768px)
    padding: 26px

  @media only screen and (max-width: 480px)
    padding: 16px

.card-text
  margin-bottom: 4px

</style>

<style lang="sass">
.card-text
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
</style>
