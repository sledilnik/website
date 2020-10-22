<template>
  <div>
    <transition name="slide">
      <div class="float-nav-btn" v-show="list.length > 0" @click="toggleMenu">
        <div
          class="float-nav-img"
          :class="{ active: active }"
          :alt="$t('navbar.goToGraph')"
        />
      </div>
    </transition>
    <div v-show="active && list.length > 0" class="float-list">
      <h2>{{ $t('navbar.goToGraph') }}</h2>
      <ul v-scroll-lock="active">
        <li v-for="item in list" :key="item[Object.keys(item)]" class="float-item" @click="hideMenu">
          <a
            :href="'#' + Object.keys(item)"
            v-scroll-to="{ el: '#' + Object.keys(item), offset: -115 }"
            >{{ item[Object.keys(item)] }}</a
          >
        </li>
      </ul>
    </div>
    <div v-show="active" class="overlay" @click="hideMenu"></div>
  </div>
</template>

<script>
export default {
  name: 'FloatingMenu',
  props: {
    list: Array,
  },
  data() {
    return {
      active: false,
    }
  },
  methods: {
    shortTitle(title) {
      return this.charts[title]
    },
    toggleMenu() {
      this.active = !this.active
    },
    hideMenu() {
      this.active = false
    },
  },
}
</script>

<style lang="scss">
@mixin nav-break {
  @media only screen and (min-width: 1150px) {
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

    &:not(:last-child) {
      border-bottom: 1px solid #d8d8d8;
    }

    &:last-child {
      padding-bottom: 16px;
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
  z-index: 2002;
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
.slide-enter-active {
  transition: 0.7s;
}
.slide-enter {
  will-change: transform;
  transform: translate(100%, 0);
}
.slide-leave-to {
  transform: translate(-100%, 0);
}
</style>
