<template>
  <div class="section">
    <div class="youtube-embed">
      <iframe :src="videoId" allowfullscreen="true"></iframe>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Youtube',
  props: {
    id: String,
  },
  data() {
    return {
      videoId: 'https://www.youtube-nocookie.com/embed/' + this.id + '?rel=0',
    }
  },
}
</script>

<style scoped lang="scss">
.section {
  background: #fff;
  margin: 0 0 58px;
  position: relative;
  padding: 0;
  box-shadow: $element-box-shadow;

  @media only screen and (min-width: 768px) {
    padding: 32px;
    margin: 0px 15px 88px;
  }
}

.youtube-embed {
  position: relative;
  height: 0;
  overflow: hidden;
  padding-bottom: 56.25%;
}

iframe {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border: 0;
}
</style>
