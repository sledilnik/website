<template>
  <div class="page-not-found">
    <div class="error-container">
      <h1>{{ $t("pageNotFound.notFound") }}</h1>
      <p>{{ $t("pageNotFound.somethingWentWrong") }}</p>
      <div class="links">
        <a href="#" @click="goBack" class="link">{{ $t("pageNotFound.back") }}</a>
        <router-link to="stats" class="link link-home">
          <span>{{ $t("navbar.home") }}</span>
        </router-link>
      </div>
    </div>
    <img src="../assets/caution-tape.png" alt />
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
  max-width: 1170px;
  display: flex;
  flex-direction: column;
  margin: 0 auto;
  padding: 20px;
  text-align: center;

  h1 {
    font-size: 46px;
    margin: 0 auto;
    margin-bottom: 1em;
  }

  img {
    display: block;
    max-width: 80%;
    margin: 0 auto;
  }

  .links {
    margin: 5em auto 2em auto;

    .link {
      display: inline-block;
      color: black;
      font-size: 14px;
      letter-spacing: 1px;
      font-weight: bold;
      line-height: 24px;
      border-radius: 6px;
      padding: 3px 16px;
      border: solid 1px rgba(0, 0, 0, 0.13);
      cursor: pointer;

      &:hover {
        text-decoration: none;
        border: solid 1px rgba(0, 0, 0, 0.56);
      }
    }

    .link-home {
      background: $yellow;
      background-image: url(../assets/logo.png);
      background-repeat: no-repeat;
      background-size: 28px 28px;
      background-position: 3px 50%;
      padding: 3px 8px 3px 36px;
      margin-left: 3em;
    }
  }

  @media only screen and (min-width: 768px) {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;

    h1 {
      margin-bottom: 1em;
    }

    img {
      width: 40%;
    }

    .error-container {
      width: 50%;
    }

    .links {
      .link {
        transform: scale(1.14);
      }
    }
  }
}
</style>
