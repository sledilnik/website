<template>
  <div class="visualizations" v-b-visible.once.350="handleVisibility">
    <div class="visualization container">
      <section class="visualization-chart" id="youtube">
        <div class="title-chart-wrapper">
          <div class="title-brand-wrapper">
            <h2><a href="#youtube">{{ $t('youtube.title') }}</a></h2>
          </div>
        </div>
        <div class="youtube-embed" v-if="isVisible">
          <iframe :src="videoId" allowfullscreen="true" loading="lazy"></iframe>
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
      isVisible: false,
      videoId: 'https://www.youtube-nocookie.com/embed/' + this.id + '?rel=0',
    }
  },
  methods: {
    handleVisibility(isVisible){
      this.isVisible = isVisible
    }
  },
}
</script>

<style scoped lang="scss">
section {
  &::before,
  &::after {
    display: none !important;
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
