<template>
  <b-container fluid class="h-100 mt-3">
    <b-row cols="12" class="h-100">
      <b-col>
        <h2 class="text-center">Potrjeno okuženi po občinah</h2>
        <!-- Dokler se ne znebimo iframa naj bo visina fixna -->
        <iframe
          src="https://covid19.alpaka.si/zemljevid/embedded.html"
          scrolling="auto"
          width="100%"
          height="2000px"
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
<style scoped lang="scss">
h2 {
  margin-bottom: 35px;

  // hides white stripe on top of map
  &:after {
    content: "";
    position: absolute;
    left: 0;
    right: 0;
    top: 70px;
    display: block;
    height: 10px;
    background: $background;
  }
}
</style>
