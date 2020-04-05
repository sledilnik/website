<template>
  <div class="navbar-container">
    <b-navbar fixed="top" toggleable="lg" :class="{ scrolled: scrollPosition > 80 }">
      <b-navbar-brand to="stats"></b-navbar-brand>
      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>
      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav class="ml-auto">
          <b-nav-item to="stats">Domov</b-nav-item>
          <b-nav-item to="tables">Tabela</b-nav-item>
          <b-nav-item to="models">Modeli</b-nav-item>
          <b-nav-item to="faq">FAQ</b-nav-item>
          <b-nav-item to="about">O projektu</b-nav-item>
          <b-nav-item to="team">Ekipa</b-nav-item>
          <b-nav-item to="sources">Viri</b-nav-item>
          <b-nav-item to="links">Povezave</b-nav-item>
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>
  </div>
</template>

<script>
export default {
  name: 'Navbar',
  props: {
    msg: String,
  },
  data() {
    return {
      scrollPosition: '',
    };
  },
  created() {
    window.addEventListener('scroll', this.handleScroll);
  },
  methods: {
    handleScroll() {
      this.scrollPosition = window.scrollY;
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.navbar-container {
  overflow: hidden;
}

nav.navbar {
  background: $yellow;
  transition: all 0.45s ease-in-out;
  padding-top: 12px;
  padding-bottom: 12px;

  .navbar-toggler {
    border: none;
    padding: 0 4px 0 0;

    &:focus {
      outline: none;
    }
  }

  .navbar-toggler-icon {
    background-image: url(../assets/svg/covid19-menu.svg);
    width: 24px;
  }

  &.scrolled {
    padding-top: 0px;
    padding-bottom: 0px;

    box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22);
  }

  &.show {
    background: $yellow !important;
  }

  .navbar-expand-lg {
    background: $yellow !important;
  }

  .navbar-brand {
    background: url('../assets/svg/covid-19.svg') no-repeat;
    width: 145px;
    height: 39px;
    margin: 7px 0;
  }

  a.nav-link {
    position: relative;
    display: inline-block;
    color: rgba(0, 0, 0, 0.5);
    line-height: 20px;
    margin-right: 15px;
    font-size: 14px;

    &:hover {
      color: rgb(0, 0, 0) !important;
    }

    &:focus {
      outline: none;
    }

    &.router-link-active {
      &:after {
        content: '';
        position: absolute;
        display: block;
        z-index: -1;
        left: 0;
        right: 0;
        bottom: 3px;
        border-bottom: 10px solid #ffffff;

        @media (min-width: 992px) {
          left: 0.5rem;
          right: 0.5rem;
        }
      }
    }

    &.router-link-active {
      font-weight: 400;
      color: #000000 !important;

      &:hover {
        color: #000000 !important;
      }
    }
  }
}
</style>
