<template>
  <div class="page-not-found">
    <h1>{{ $t("pageNotFound.notFound") }}</h1>
    <p>{{ $t("pageNotFound.somethingWentWrong") }}</p>
    <img src="../assets/caution-tape.png" :alt="$t('pageNotFound.imageDescription')" />
    <div class="links">
      <!-- <a href="#" @click="goBack" class="link">{{ $t("pageNotFound.back") }}</a> -->
      <router-link to="stats" class="link link-home">
        <span>{{ $t("navbar.home") }}</span>
      </router-link>
    </div>
  </div>
</template>

<script>
export default {
  name: "PageNotFound",
  metaInfo() {
    return {
      meta: [
        { name: 'robots', content: 'noindex' },
      ],
    }
  },
  methods: {
    goBack() {
      return this.$router.go(-1);
    }
  }
};
</script>

<style scoped lang="scss">
.page-not-found {
  max-width: 730px;
  width: 90%;
  background: #fff;
  box-shadow: $element-box-shadow;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  margin: 0 auto 96px auto;
  padding: 32px;

  h1 {
    font-size: 28px;
    margin: 0 auto;
    margin-bottom: 24px;
  }

  img {
    display: block;
    width: 90%;
    max-width: 400px;
    margin: -70px auto 0 auto;

    @media only screen and (max-width: 400px) {
      margin: -40px auto 0 auto;
    }
  }

  p {
    font-style: italic;
    font-size: 16px;
    color: rgba(0, 0, 0, 0.8);
  }

  .links {
    margin: 0 auto;

    .link {
      display: inline-block;
      color: #000;
      font-size: 14px;
      line-height: 16px;
      font-weight: bold;
      border-radius: 6px;
      padding: 11px 12px;
      border: solid 1px rgba(0, 0, 0, 0.13);
      cursor: pointer;

      &:hover {
        text-decoration: none;
      }
    }

    .link-home {
      background: $yellow;
      margin-left: 24px;
      border: none;
    }
  }
}
</style>
