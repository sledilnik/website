<template>
  <div class="visualizations">
    <div class="visualization container">
      <section class="visualization-chart" id="youtube">
        <div class="title-chart-wrapper">
          <div class="title-brand-wrapper">
            <h2><a href="#youtube">{{ $t('youtube.title') }}</a></h2>
          </div>
        </div>
        <div class="youtube-embed">
          <iframe :src="videoId" allowfullscreen="true"></iframe>
        </div>
      </section>
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
      videoId: 'https://www.youtube.com/embed/' + this.id + '?rel=0',
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
