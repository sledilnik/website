<template>
  <div class="container">
    <a class="notice-link" :href="post.link_to">
      <div class="notice">
        <div v-html="post.blurb"></div>
        <div class="notice-button">
          <img src="../assets/svg/go-to.svg" alt="Go to news" />
        </div>
      </div>
    </a>
  </div>
</template>
<script>
export default {
  props: {
    post: Object,
  },
};
</script>

<style scoped lang="scss">
.container {
  padding-left: 0;
  padding-right: 0;

  @media only screen and (min-width: 768px) {
    padding-left: 15px;
    padding-right: 15px;
  }
}

.notice-link {
  text-decoration: none;
}

.notice {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fffbe8;
  padding: 18px;
  margin: 0px auto 48px;
  width: 100%;
  cursor: pointer;
  font-size: 14px;
  color: $text-c;
  text-decoration: none;
  box-shadow: $element-box-shadow;
  min-height: 64px;

  p {
    margin-bottom: 0 !important;
  }

  @media only screen and (min-width: 480px) {
    padding: 26px;
  }

  @media only screen and (min-width: 768px) {
    padding: 20px 24px 20px 32px;
  }

  &-button {
    margin-left: 16px;
  }

  span b {
    padding-right: 8px;
  }
}
</style>
