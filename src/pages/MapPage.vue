<template>
  <b-container fluid class="h-100 mt-3">
    <b-row cols="12" class="h-100">
      <b-col>
        <h3 class="text-center">Potrjeno okuženi po občinah</h3>
        <iframe
          src="https://covid19.alpaka.si/zemljevid/embedded.html"
          scrolling="no"
          width="100%"
          height="100%"
          frameborder="0"
        />
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
export default {
  name: "MapPage"
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
/* iframe {
  margin: 0 auto;
} */
</style>
