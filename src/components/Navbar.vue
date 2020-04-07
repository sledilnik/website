<template>
  <div
    class="navbar-container"
    :class="{ scrolled: scrollPosition > 80, menuOpen: menuOpened, closingMenu: closingMenu }"
  >
    <div class="navbar-logo"></div>
    <div class="🍔" @click="toggleMenu">
      <div class="line line-1"></div>
      <div class="line line-2"></div>
      <div class="line line-3"></div>
    </div>
    <div class="nav-overlay"></div>
    <div class="nav-links">
      <router-link to="stats" class="router-link"><span>Domov</span></router-link>
      <router-link to="tables" class="router-link"><span>Tabela</span></router-link>
      <router-link to="models" class="router-link"><span>Modeli</span></router-link>
      <router-link to="FAQ" class="router-link"><span>FAQ</span></router-link>
      <router-link to="about" class="router-link"><span>O projektu</span></router-link>
      <router-link to="team" class="router-link"><span>Ekipa</span></router-link>
      <router-link to="sources" class="router-link"><span>Viri</span></router-link>
      <router-link to="links" class="router-link"><span>Povezave</span></router-link>
    </div>
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
      menuOpened: false,
      closingMenu: false,
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
      if (this.menuOpened) {
        this.menuOpened = false;
        this.closingMenu = true;
        setTimeout(() => {
          this.closingMenu = false;
        }, 650);
      } else {
        this.menuOpened = true;
      }
    },
  },
  watch: {
    $route() {
      this.menuOpened = false;
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
  width: 36px;
  height: 30px;
  padding: 5px 6px;
  margin-right: -3px;
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

.menuOpen {
  .🍔 {
    $ani-duration: 0.6s;

    .line-1 {
      animation: line1 $ani-duration;
      transform: translateY(9px) rotate(-45deg);

      @keyframes line1 {
        0% {
          transform: translateY(0px) rotate(0deg);
        }
        50% {
          transform: translateY(9px) rotate(0deg);
        }
        100% {
          transform: translateY(9px) rotate(-45deg);
        }
      }
    }
    .line-2 {
      opacity: 0;
      animation: line2 $ani-duration;

      @keyframes line2 {
        0% {
          opacity: 1;
        }
        49% {
          opacity: 0;
        }
      }
    }
    .line-3 {
      animation: line3 $ani-duration;
      transform: translateY(-9px) rotate(45deg);

      @keyframes line3 {
        0% {
          transform: translateY(0px) rotate(0deg);
        }
        50% {
          transform: translateY(-9px) rotate(0deg);
        }
        100% {
          transform: translateY(-9px) rotate(45deg);
        }
      }
    }
  }
}

.closingMenu {
  .🍔 {
    $ani-duration: 0.6s;

    .line-1 {
      animation: line1-closing $ani-duration;
      transform: translateY(0px) rotate(0deg);

      @keyframes line1-closing {
        0% {
          transform: translateY(9px) rotate(-45deg);
        }
        50% {
          transform: translateY(9px) rotate(0deg);
        }
        100% {
          transform: translateY(0px) rotate(0deg);
        }
      }
    }
    .line-2 {
      opacity: 1;
      animation: line2-closing $ani-duration;

      @keyframes line2-closing {
        0% {
          opacity: 0;
        }
        49% {
          opacity: 1;
        }
      }
    }
    .line-3 {
      animation: line3-closing $ani-duration;
      transform: translateY(0px) rotate(0deg);

      @keyframes line3-closing {
        0% {
          transform: translateY(-9px) rotate(45deg);
        }
        50% {
          transform: translateY(-9px) rotate(0deg);
        }
        100% {
          transform: translateY(0px) rotate(0deg);
        }
      }
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
  min-height: 100vh;
  left: 33%;
  z-index: 100;
  background: $yellow;
  padding: 18px 0 0 15px;
  overflow: auto;
  transition: all 0.45s ease-in-out;

  .scrolled & {
    padding: 6px 0 0 15px;
  }

  &:before {
    content: 'Meni';
    display: block;
    margin-bottom: 18px;
    font-size: 21px;
    font-weight: bold;
  }

  .menuOpen & {
    display: block;
    animation: menu-transition 0.65s;

    @keyframes menu-transition {
      0% {
        transform: translateX(100%);
      }
      28% {
        transform: translateX(100%);
      }
      100% {
        transform: translateX(0%);
      }
    }
  }

  .closingMenu & {
    display: block;
    animation: menu-transition-close 0.65s;

    @keyframes menu-transition-close {
      0% {
        transform: translateX(0%);
      }
      100% {
        transform: translateX(100%);
      }
    }
  }

  @include nav-break {
    position: static;
    display: block;
    min-height: auto;
    padding: 0;

    &:before {
      display: none;
    }
  }
}

.nav-overlay {
  display: none;

  .closingMenu &,
  .menuOpen & {
    display: block;
    position: fixed;
    top: 0;
    right: 0;
    left: 0;
    min-height: 100vh;
    z-index: 99;
    background: rgb(0, 0, 0);

    @include nav-break {
      display: none;
    }
  }

  .menuOpen & {
    opacity: 0.75;
    animation: overlay-transition 0.65s ease-out;

    @keyframes overlay-transition {
      0% {
        opacity: 0;
      }

      100% {
        opacity: 0.75;
      }
    }
  }

  .closingMenu & {
    opacity: 0;
    animation: overlay-transition-close 0.65s ease-out;

    @keyframes overlay-transition-close {
      0% {
        opacity: 0.75;
      }

      100% {
        opacity: 0;
      }
    }
  }
}

.navbar-logo {
  background: url('../assets/svg/covid-19.svg') no-repeat;
  width: 145px;
  height: 40px;
}

.router-link {
  position: relative;
  display: block;
  color: rgba(0, 0, 0, 0.5);
  font-size: 14px;
  line-height: 20px;
  padding: 9px 0;

  & + .router-link {
    @include nav-break {
      margin-left: 32px;
    }
  }

  @include nav-break {
    display: inline-block;
    padding: 6px 0;
  }

  &:hover {
    text-decoration: none;
    color: rgb(0, 0, 0);
  }

  &:focus {
    outline: none;
  }

  span {
    line-height: 30px;
    display: inline-block;
  }

  &.router-link-active {
    span {
      line-height: 30px;
      display: inline-block;
      box-shadow: inset 0 -10px 0 #fff;
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
