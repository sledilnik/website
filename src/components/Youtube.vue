<template>
  <div class="youtube-embed">
    <iframe :src="videoId"></iframe>
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
      videoId: 'https://www.youtube-nocookie.com/embed/' + this.id,
    }
  },
}
</script>

<style scoped lang="scss">
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
