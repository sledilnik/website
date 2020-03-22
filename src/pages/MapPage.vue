<template>
  <b-container fluid class="h-100">
    <b-row cols="12" class="h-100">
      <b-col>
        <h1 class="text-center">Stanje okužb s COVID-19 po občinah</h1>
        <iframe
          src="https://covid19.alpaka.si/zemljevid/embedded.html"
          scrolling="no"
          width="100%"
          height="90%"
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
