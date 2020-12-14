<template>
  <div>
    <transition name="slide">
      <div
        class="float-nav-btn"
        v-show="!isMenuOpened && visible && list.length > 0"
        @click="toggleMenu"
        key="1"
      >
        <div
          class="float-nav-img"
          :class="{ active: active }"
          :alt="title"
        />
      </div>
    </transition>
    <transition name="slide">
      <div v-show="active && list.length > 0" class="float-list" key="2">
        <h2>{{ title }}</h2>
        <ul v-scroll-lock="active">
          <li
            v-for="item in list"
            :key="item.link"
            class="float-item"
            @click="hideMenu"
          >
            <div
              :class="'float-nav-icon' + ' ' + item.icon"
              :alt="$t('navbar.goToGraph')"
            />
            <a :href="'#' + item.link" @click="scrollTo">{{ item.title }}</a>
          </li>
        </ul>
      </div>
    </transition>
    <transition name="fade">
      <div v-show="active" class="overlay" @click="hideMenu" key="3"></div>
    </transition>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'

export default {
  name: 'FloatingMenu',
  props: {
    list: {
      type: Array,
      required: true
    },
    title: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      active: false,
      visible: true,
    }
  },
  computed: {
    ...mapGetters('general', ['isMenuOpened'])
  },
  created() {
    window.addEventListener('scroll', this.handleScroll, { passive: true })
    window.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        this.active = false
      }
    })
  },
  beforeDestroy() {
    window.removeEventListener('scroll', this.handleScroll, { passive: true })
  },
  methods: {
    toggleMenu() {
      this.active = !this.active
    },
    hideMenu() {
      this.active = false
    },
    handleScroll() {
      let footer = document.querySelector('footer').getBoundingClientRect()
      let bottom = window.innerHeight - footer.top
      this.visible = bottom < -50
    },
    scrollTo(e) {
      this.$scrollTo(
        document.querySelector(e.target.getAttribute('href')),
        500,
        {
          offset: -90,
        }
      )
    }
  },
}
</script>

<style lang="scss">
@mixin nav-break {
  @media only screen and (min-width: 900px) {
    @content;
  }
}

.float-list {
  position: fixed;
  bottom: 80px;
  right: 16px;
  padding: 16px 0 0px 16px;
  z-index: 2001;
  background: white;
  font-size: 14px;
  border-radius: 6px;
  box-shadow: $element-box-shadow;
  min-width: 235px;
  max-height: 600px;

  @include nav-break {
    bottom: 98px;
    right: 32px;
  }

  h2 {
    font-size: 21px;
    line-height: 28px;
    font-weight: bold;
    margin-bottom: 24px;
  }

  ul {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 400px;
    overflow: auto;
  }

  .float-item {
    padding: 0 16px 0 0;
    display: flex;
    align-items: center;

    &:not(:last-child) {
      border-bottom: 1px solid #d8d8d8;
    }

    &:last-child {
      padding-bottom: 16px;
    }

    .float-nav-icon {
      width: 15px;
      height: 15px;
      margin-right: 8px;

      &.graph {
        background-image: url('../assets/svg/graf.svg');
      }

      &.column {
        background-image: url('../assets/svg/stolpicni.svg');
      }

      &.map {
        background-image: url('../assets/svg/zemljevid.svg');
      }
    }

    a {
      padding: 6px 0;
      display: block;
      cursor: pointer;
      text-decoration: none;
      color: rgba(0, 0, 0, 0.7);
      border-radius: 2px;
    }
  }
}

.float-nav-btn {
  position: fixed;
  bottom: 7px;
  right: 7px;
  z-index: 2001;
  cursor: pointer;
  display: inline-block;

  @include nav-break {
    right: 24px;
    bottom: 24px;
  }
}

.float-nav-img {
  background-image: url('../assets/svg/floating-close.svg');
  height: 72px;
  width: 72px;
  transition: all 0.3s;

  &.active {
    background-image: url('../assets/svg/floating-open.svg');
  }
}

.overlay {
  position: fixed;
  background: rgba(0, 0, 0, 0.75);
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 2000;
}

.slide-leave-active,
.fade-leave-active {
  transition: 0.3s;
}
.slide-enter-active,
.fade-enter-active {
  transition: 0.5s;
}
.slide-enter {
  transform: translate3d(100%, 0, 0);
  will-change: transform;
}
.slide-leave-to {
  transform: translate3d(110%, 0, 0);
  will-change: transform;
}
</style>
