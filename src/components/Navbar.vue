<template>
  <div class="mb-3">
    <b-navbar fixed="top" toggleable="lg">
      <b-navbar-brand to="about"></b-navbar-brand>

      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav class="ml-auto">
          <b-nav-item to="about">O projektu</b-nav-item>
          <b-nav-item to="data">Podatki</b-nav-item>
          <b-nav-item to="stats">Statistika</b-nav-item>
          <b-nav-item to="map">Zemljevid</b-nav-item>
          <b-nav-item to="sources">Viri podatkov</b-nav-item>
          <b-nav-item to="links">Povezave</b-nav-item>
          <b-nav-item to="team">Ekipa</b-nav-item>
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>
  </div>
</template>

<script>
export default {
  name: "HelloWorld",
  props: {
    msg: String
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
$yellow: #ffd922;

nav.navbar {
  background: $yellow;

  &.show {
    background: $yellow !important;
  }

  .navbar-expand-lg {
    background: $yellow !important;
  }

  .navbar-brand {
    background: url("/covid-19.svg") no-repeat;
    width: 145px;
    height: 40px;
    margin: 12px 0;
  }

  a.nav-link {
    color: #000 !important;
  }
}
</style>
