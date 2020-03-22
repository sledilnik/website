<template>
  <div>
    <b-navbar toggleable="lg" type="dark" variant="info">
      <b-navbar-brand to="about">COVID19 Slovenija</b-navbar-brand>

      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav>
          <b-nav-item to="about">O projektu</b-nav-item>
          <b-nav-item to="data">Podatki</b-nav-item>
          <b-nav-item to="stats">Statistika</b-nav-item>
          <b-nav-item to="viz">Vizualizacije</b-nav-item>
          <b-nav-item to="map">Zemljevid</b-nav-item>
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
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
