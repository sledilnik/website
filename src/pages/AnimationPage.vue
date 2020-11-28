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
<style lang="scss"></style>
