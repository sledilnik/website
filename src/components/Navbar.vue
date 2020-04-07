<template>
  <div class="navbar-container" :class="{ scrolled: scrollPosition > 80, menuOpen: menuOpened }">
    <div class="navbar-logo"></div>
    <div class="🍔" @click="toggleMenu">
      <div class="line line-1"></div>
      <div class="line line-2"></div>
      <div class="line line-3"></div>
    </div>
    <div class="nav-overlay"></div>
    <div class="nav-links">
      <router-link to="stats" class="router-link">Domov</router-link>
      <router-link to="tables" class="router-link">Tabela</router-link>
      <router-link to="models" class="router-link">Modeli</router-link>
      <router-link to="FAQ" class="router-link">FAQ</router-link>
      <router-link to="about" class="router-link">O projektu</router-link>
      <router-link to="team" class="router-link">Ekipa</router-link>
      <router-link to="sources" class="router-link">Viri</router-link>
      <router-link to="links" class="router-link">Povezave</router-link>
    </div>
  </div>
  <!-- <b-navbar fixed="top" toggleable="lg" :class="{ scrolled: scrollPosition > 80 }">
      <b-navbar-brand to="stats"></b-navbar-brand>
      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>
      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav class="ml-auto">
          <b-nav-item to="stats">Domov</b-nav-item>
          <b-nav-item to="tables">Tabela</b-nav-item>
          <b-nav-item to="models">Modeli</b-nav-item>
          <b-nav-item to="FAQ">FAQ</b-nav-item>
          <b-nav-item to="about">O projektu</b-nav-item>
          <b-nav-item to="team">Ekipa</b-nav-item>
          <b-nav-item to="sources">Viri</b-nav-item>
          <b-nav-item to="links">Povezave</b-nav-item>
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>
  </div> -->
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
      menuOpened: true,
    };
  },
  created() {
    window.addEventListener('scroll', this.handleScroll);
  },

  methods: {
    handleScroll() {
      this.scrollPosition = window.scrollY;
    },
    toggleMenu() {
      this.menuOpened ? (this.menuOpened = false) : (this.menuOpened = true);
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
// @include nav-greak
@mixin nav-break {
  @media only screen and (min-width: 992px) {
    @content;
  }
}
.🍔 {
  width: 34px;
  height: 30px;
  padding: 5px;
  position: absolute;
  right: 15px;
  z-index: 101;
  cursor: pointer;

  @include nav-break {
    display: none;
  }

  .line {
    height: 2px;
    width: 24px;
    background: #000;

    & + .line {
      margin-top: 7px;
    }
  }
}

.navbar-container {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  overflow: hidden;
  background: $yellow;
  transition: all 0.45s ease-in-out;
  padding: 16px 15px;
  display: flex;
  align-items: center;
  z-index: 98;

  @include nav-break {
    padding: 16px 32px;
  }

  &.scrolled {
    padding-top: 4px;
    padding-bottom: 4px;

    box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3), 0 11px 12px -12px rgba(0, 0, 0, 0.22);
  }
}

.nav-links {
  margin-left: auto;
  display: none;
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  left: 30%;
  z-index: 100;
  background: #bada55;

  .menuOpen & {
    display: block;
    animation: menu-transition 0.8s;

    @keyframes menu-transition {
      0% {
        transform: translateX(100%);
      }
      30% {
        transform: translateX(100%);
      }
      100% {
        transform: translateX(0%);
      }
    }
  }

  @include nav-break {
    position: static;
    display: block;
  }
}

.nav-overlay {
  display: none;

  .menuOpen & {
    display: block;
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    z-index: 99;
    background: rgb(0, 0, 0);
    opacity: 0.75;
    animation: overlay-transition 0.5s;

    @keyframes overlay-transition {
      0% {
        opacity: 0;
      }

      100% {
        opacity: 0.75;
      }
    }
  }
}

.navbar-logo {
  background: url('../assets/svg/covid-19.svg') no-repeat;
  width: 145px;
  height: 40px;
  // margin: 7px 0;

  // @media only screen and (min-width: 768px) {
  //   margin: 7px 0 7px 15px;
  // }
}

// .navbar-toggler {
//   border: none;
//   padding: 0 4px 0 0;

//   @media only screen and (min-width: 768px) {
//     padding: 0 19px 0 0;
//   }

//   &:focus {
//     outline: none;
//   }
// }

// .navbar-toggler-icon {
//   background-image: url(../assets/svg/covid19-menu.svg);
//   width: 24px;
// }

// &.show {
//   background: $yellow !important;
// }

// .nav-item {
//   @media only screen and (min-width: 768px) {
//     margin-left: 15px;
//   }

//   @media (min-width: 992px) {
//     margin-left: 0px;
//   }

//   & + .nav-item {
//     @media (min-width: 992px) {
//       margin-left: 32px;
//     }
//   }

//   &:first-child {
//     margin-top: 12px;
//     @media (min-width: 992px) {
//       margin-top: 0;
//     }
//   }

//   &:last-child {
//     margin-bottom: 12px;

//     @media (min-width: 992px) {
//       margin-right: 15px;
//       margin-bottom: 0;
//     }
//   }
// }

.router-link {
  position: relative;
  display: block;
  color: rgba(0, 0, 0, 0.5);
  line-height: 23px;
  font-size: 18px;
  padding: 6px 16px 6px 0;

  & + .router-link {
    @include nav-break {
      margin-left: 32px;
    }
  }

  @include nav-break {
    display: inline-block;
    font-size: 14px;
    line-height: 20px;
    padding: 6px 0;
  }

  &:hover {
    text-decoration: none;
    color: rgb(0, 0, 0);
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
      right: 16px;
      bottom: 0px;
      border-bottom: 10px solid #ffffff;

      @media (min-width: 992px) {
        right: 0;
        bottom: 1px;
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
</style>
