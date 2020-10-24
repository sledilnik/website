<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <span v-html-md="$t(localeKey)"></span>
    </div>
  </div>
</template>

<script>

import '@/style/static.scss'

export default {
  name: 'StaticPage',
  props: {
    localeKey: String,
  },
  mounted() {
    this.initDropdowns;

    // open question, if anchor link
    if (this.$route.hash && document.querySelector(this.$route.hash)) {
      document.querySelector(this.$route.hash).parentElement.open=true
    }
  },
  methods: {
    initDropdowns: function() {
      let dropdowns = document.querySelectorAll('.dropdown');
      dropdowns.forEach((dropdown) => {
        dropdown.querySelector('.dd-title').addEventListener('click', () => {
          dropdown.classList.contains('dd-show')
            ? dropdown.classList.remove('dd-show')
            : dropdown.classList.add('dd-show');
        });
      });
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

</style>
