<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("navbar.animation") }}</h1>
      <Youtube id="oD88hoRafug"></Youtube>
    </div>
  </div>
</template>

<script>
import Youtube from 'components/Youtube'

export default {
  name: 'AnimationPage',
  components: {
      Youtube
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.custom-container {
  margin: -24px auto 0 auto;
  max-width: 730px;

  @media only screen and (min-width: 768px) {
    margin: 0 auto 65px auto;
    box-shadow: $element-box-shadow;
  }
}

.static-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  h1 {
    margin-bottom: 32px;
    font-size: 28px;
  }
}
</style>
