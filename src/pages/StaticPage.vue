<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <span v-html="staticContent"></span>
    </div>
  </div>
</template>

<script>
export default {
  name: "StaticPage",
  props: {
    name: String,
    content: Promise
  },
  data() {
    return {
      staticContent: ""
    };
  },
  mounted() {
    this.content.then(data => {
      this.staticContent = data.default;
      this.$forceUpdate();
    });
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scopped lang="scss">
.custom-container {
  margin: 0 auto 65px auto;
  max-width: 760px;

  @media only screen and (min-width: 768px) {
    margin: 40px auto 65px auto;
    padding: 0 15px;
  }
}

.static-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  h1,
  h2,
  h3,
  h4 {
    margin-bottom: 24px;
  }

  h2 {
    font-size: 32px;
    width: 90%;

    & + p {
      font-size: 16px;
      margin-bottom: 48px;
    }
  }

  h3 {
    font-size: 24px;
  }

  p + p {
    margin-top: 14px;
  }

  p,
  a,
  span,
  strong {
    font-size: 14px;
    color: $text-c;
  }

  * + h2,
  * + h3,
  tr + tr {
    margin-top: 27px;
  }

  a {
    position: relative;
    font-weight: 600;
    color: $text-c;
    z-index: 1;
    background: rgba(255, 255, 255, 0);
    transition: all 0.35s ease-in-out;
    box-shadow: inset 0 -1px 0 white, inset 0 -4px $yellow;
    text-decoration: none;

    &:hover {
      text-decoration: none;
      color: $text-c;
      font-weight: 600;
      box-shadow: inset 0 -1px 0 white, inset 0 -20px $yellow;
    }
  }

  table {
    width: 100%;
    table-layout: fixed;
    text-align: left;
  }

  table {
    table-layout: fixed;
    text-align: left;
    td {
      padding: 15px 0;
      width: 50%;
      border-top: 1px solid rgba(0, 0, 0, 0.45);
    }
  }
}
</style>
