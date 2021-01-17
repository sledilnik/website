<template>
  <div
    class="navbar-container"
    :class="{
      scrolled: scrollPosition > 80,
      menuOpen: menuOpened,
      closingMenu: closingMenu,
    }"
  >
    <router-link :to="{name: 'stats'}" class="navbar-logo"></router-link>
    <div class="🍔" @click="toggleMenu">
      <div class="line line-1"></div>
      <div class="line line-2"></div>
      <div class="line line-3"></div>
    </div>
    <div class="nav-overlay"></div>
    <div class="nav-links">
      <div class="nav-heading">{{ $t('navbar.menu') }}</div>
      <router-link :to="{ name: 'stats' }" class="router-link"><span>{{ $t('navbar.home') }}</span></router-link>
      <router-link :to="{ name: 'world' }" class="router-link"><span>{{ $t('navbar.world') }}</span></router-link>
      <router-link :to="{ name: 'data' }" class="router-link"><span>{{ $t('navbar.data') }}</span></router-link>
      <router-link :to="{ name: 'models' }" class="router-link"><span>{{ $t('navbar.models') }}</span></router-link>
      <router-link :to="{ name: 'posts' }" class="router-link"><span>{{ $t('navbar.posts') }}</span></router-link>
      <router-link :to="{ name: 'ostanizdrav' }" class="router-link"><span>{{ $t('navbar.ostanizdrav') }}</span></router-link>
      <router-link :to="{ name: 'faq' }" class="router-link"><span>{{ $t('navbar.faq') }}</span></router-link>
      <router-link :to="{ name: 'about' }" class="router-link"><span>{{ $t('navbar.about') }}</span></router-link>
      <router-link :to="{ name: 'donate' }" class="router-link"><span>{{ $t('navbar.donate') }}</span></router-link>
      <div class="social" v-if="showIcons">
        <a href="https://fb.me/COVID19Sledilnik" target="_blank" rel="noreferrer">
          <img src="../assets/svg/fb-icon.svg" alt="Facebook" />
        </a>
        <a href="https://twitter.com/sledilnik" target="_blank" rel="noreferrer">
          <img src="../assets/svg/tw-icon.svg" alt="Twitter" />
        </a>
        <a href="https://medium.com/sledilnik" target="_blank" rel="noreferrer">
          <img src="../assets/svg/medium-icon.svg" alt="Medium" />
        </a>
        <a href="https://www.youtube.com/channel/UCM_Sk2GZ8vTMiyBQ0SMHgJw/videos" target="_blank" rel="noreferrer">
          <img src="../assets/svg/youtube.svg" alt="YouTube" />
        </a>
        <a href="https://github.com/sledilnik" target="_blank" rel="noreferrer">
          <img src="../assets/svg/gh-icon.svg" alt="GitHub" />
        </a>
      </div>
      <LanguageSwitcher />
    </div>
  </div>
</template>

<script>
import { mapGetters, mapState } from 'vuex'
import LanguageSwitcher from './LanguageSwitcher'

export default {
  name: 'Navbar',
  components: {
    LanguageSwitcher,
  },
  data() {
    return {
      scrollPosition: '',
      menuOpened: false,
      closingMenu: false,
      dropdownVisible: false,
      showIcons: true,
    }
  },
  created() {
    window.addEventListener('scroll', this.handleScroll, { passive: true })
  },
  mounted() {
    this.onResize()
    window.addEventListener('resize', this.onResize, { passive: true })
  },
  beforeDestroy() {
    if (typeof window !== 'undefined') {
      window.removeEventListener('resize', this.onResize, { passive: true })
    }
  },
  methods: {
    handleScroll() {
      this.scrollPosition = window.scrollY
    },
    toggleMenu() {
      if (this.menuOpened) {
        this.$store.commit('general/closeMenu')
        this.closeMenu()
      } else {
        this.$store.commit('general/openMenu')
        this.menuOpened = true
      }
    },
    closeMenu() {
      this.menuOpened = false
      this.closingMenu = true
      setTimeout(() => {
        this.closingMenu = false
      }, 650)
    },
    onResize() {
      this.showIcons = window.innerWidth < 1000 || window.innerWidth >= 1250
    },
  },
  watch: {
    $route() {
      this.$store.commit('general/closeMenu')
      this.menuOpened = false
    },
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
// @include nav-greak
@mixin nav-break {
  @media only screen and (min-width: 1000px) {
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
  z-index: 1101;
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
      transform: translate3d(0, 9px, 0) rotate(-45deg);
      will-change: transform;

      @keyframes line1 {
        0% {
          transform: translate3d(0, 0px, 0) rotate(0deg);
        }
        50% {
          transform: translate3d(0, 9px, 0) rotate(0deg);
        }
        100% {
          transform: translate3d(0, 9px, 0) rotate(-45deg);
        }
      }
    }
    .line-2 {
      opacity: 0;
      animation: line2 $ani-duration;
      will-change: opacity;

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
      transform: translate3d(0, -9px, 0) rotate(45deg);
      will-change: transform;

      @keyframes line3 {
        0% {
          transform: translate3d(0, 0px, 0) rotate(0deg);
        }
        50% {
          transform: translate3d(0, -9px, 0) rotate(0deg);
        }
        100% {
          transform: translate3d(0, -9px, 0) rotate(45deg);
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
      transform: translate3d(0, 0px, 0) rotate(0deg);
      will-change: transform;

      @keyframes line1-closing {
        0% {
          transform: translate3d(0, 9px, 0) rotate(-45deg);
        }
        50% {
          transform: translate3d(0, 9px, 0) rotate(0deg);
        }
        100% {
          transform: translate3d(0, 0px, 0) rotate(0deg);
        }
      }
    }

    .line-2 {
      opacity: 1;
      animation: line2-closing $ani-duration;
      will-change: opacity;

      @keyframes line2-closing {
        0% {
          opacity: 0;
        }
        50% {
          opacity: 1;
        }
      }
    }

    .line-3 {
      animation: line3-closing $ani-duration;
      transform: translate3d(0, 0px, 0) rotate(0deg);
      will-change: transform;

      @keyframes line3-closing {
        0% {
          transform: translate3d(0, -9px, 0) rotate(45deg);
        }
        50% {
          transform: translate3d(0, -9px, 0) rotate(0deg);
        }
        100% {
          transform: translate3d(0, 0px, 0) rotate(0deg);
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
  background: $yellow;
  transition: all 0.45s ease-in-out;
  padding: 16px 15px;
  display: flex;
  align-items: center;
  z-index: 1098;

  @include nav-break {
    padding: 16px 32px;
  }

  &.scrolled {
    padding-top: 7px;
    padding-bottom: 7px;

    box-shadow: 0 6px 38px -18px rgba(0, 0, 0, 0.3),
      0 11px 12px -12px rgba(0, 0, 0, 0.22);
  }
}

.nav-links {
  margin-left: auto;
  display: none;
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  left: 33%;
  z-index: 1100;
  background: $yellow;
  padding: 20px 0 0 15px;
  overflow: auto;
  transition: all 0.4s ease-in-out;
  will-change: transform;

  @include nav-break {
    overflow: visible;
  }

  .scrolled & {
    padding: 11px 0 0 15px;

    @include nav-break {
      padding: 0;
    }
  }

  .nav-heading {
    margin-bottom: 18px;
    font-size: 21px;
    font-weight: bold;

    @include nav-break {
      display: none;
    }
  }

  .menuOpen & {
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    transform: translate3d(0, 0 0);
    animation: menu-transition 0.65s;
    will-change: transform;

    @include nav-break {
      flex-direction: row;
      align-items: center;
    }

    @keyframes menu-transition {
      0% {
        transform: translate3d(100%, 0, 0);
      }

      100% {
        transform: translate3d(0, 0, 0);
      }
    }
  }

  .closingMenu & {
    display: block;
    will-change: transform;
    transform: translate3d(100%, 0, 0);

    @include nav-break {
      transform: none;
    }
  }

  @include nav-break {
    position: static;
    display: block;
    padding: 0;
    transform: none;

    &:before {
      display: none;
    }
  }

  .social {
    display: inline-block;
    margin-left: 0;
    padding: 9px 0 36px;
    @include nav-break {
      margin-left: 32px;
      padding: 0;
    }
    img {
      width: 24px;
      opacity: .56;
    }
    a + a {
      margin-left: 16px;
      @media only screen and (max-width: 480px) {
        margin-left: 16px;
      }
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
    bottom: 0;
    z-index: 1099;
    background: rgb(0, 0, 0);

    @include nav-break {
      display: none;
    }
  }

  .menuOpen & {
    opacity: 0.75;
    animation: overlay-transition 0.65s ease-out;
    will-change: opacity;

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
    will-change: opacity;

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
  color: rgba(0, 0, 0, 0.56);
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
    font-weight: 400;
    color: #000000 !important;

    span {
      line-height: 30px;
      display: inline-block;
      box-shadow: inset 0 -10px 0 #fff;
    }

    &:hover {
      color: #000000 !important;
    }
  }

  &.router-link-icon {
    border: 1px solid rgba(0, 0, 0, 0.13);
    border-radius: 6px;
    padding: 0 6px;
    display: inline-block;
    margin: 16px auto;

    &.github {
      @media only screen and (max-width: 1135px) {
        display: none;
      }
    }

    @include nav-break {
      margin: 0 0 0 22px;
    }

    span {
      margin-left: 6px;
    }

    img {
      opacity: 0.5;

      @include nav-break {
        display: inline-block;
        width: 18px;
      }
    }

    &:hover {
      border: 1px solid rgba(0, 0, 0, 0.33);
      img {
        opacity: 0.75;
      }
    }
  }

  &-anchor {
    color: rgba(0, 0, 0, 0.56);
    text-decoration: none;
    width: 100%;
    display: block;

    &.active {
      font-weight: bold;
    }

    &:hover {
      color: rgb(0, 0, 0);
    }
  }
}
</style>
