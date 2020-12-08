<template>
  <div v-if="!isMobile" class="router-link router-link-icon lang-switcher">
    <div class="lang" @click="toggleDropdown">
      <font-awesome-icon icon="globe" />
      <span v-if="showFullLang">{{
        $t('navbar.language.' + selectedLanguage)
      }}</span>
      <span v-else>{{ selectedLanguage.toUpperCase() }}</span>
      &nbsp;<font-awesome-icon icon="caret-down" />
    </div>
    <transition name="fade">
      <ul v-if="active" class="lang-list" v-on-clickaway="hideDropdown">
        <li
          v-for="(lang, index) in languages"
          :key="index"
          class="lang-list-item"
        >
          <a
            v-if="lang != selectedLanguage || !showFullLang"
            :href="
              `/${lang}/${$route.path
                .slice(4)
                .toLowerCase()
                .replace(/\/$/, '')}`
            "
            :hreflang="lang"
            class="router-link-anchor"
            :class="{ active: $i18n.i18next.language === lang }"
            @click.prevent="changeLanguage(lang)"
          >
            {{ $t('navbar.language.' + lang, { lng: lang }) }}
          </a>
        </li>
      </ul>
    </transition>
  </div>
  <div v-else class="lang-switcher-mobile">
    <div v-for="(lang, index) in languages" :key="index">
      <a
        :href="
          `/${lang}/${$route.path
            .slice(4)
            .toLowerCase()
            .replace(/\/$/, '')}`
        "
        :hreflang="lang"
        class="router-link router-link-anchor"
        :class="{ active: $i18n.i18next.language === lang }"
        @click.prevent="changeLanguage(lang)"
      >
        <font-awesome-icon icon="globe" />
        <span>{{ $t('navbar.language.' + lang, { lng: lang }) }}</span>
      </a>
    </div>
  </div>
</template>

<script>
import i18next from 'i18next'
import { mixin as clickaway } from 'vue-clickaway'

export default {
  mixins: [clickaway],
  name: 'LanguageSwitcher',
  data() {
    return {
      isMobile: false,
      showFullLang: true,
      active: false,
      languages: i18next.languages.sort(),
      selectedLanguage: i18next.language,
    }
  },
  methods: {
    onResize() {
      this.isMobile = window.innerWidth < 900
      this.showFullLang = window.innerWidth >= 1200
    },
    toggleDropdown() {
      this.active = !this.active
    },
    changeLanguage(lang) {
      if (this.$route.params.lang === lang) return
      this.$i18n.i18next.changeLanguage(lang, (err, t) => {
        if (err) return console.log('something went wrong loading', err)
        this.selectedLanguage = lang
        this.active = false
        this.$router.push({ name: this.$route.name, params: { lang } })
      })
    },
    hideDropdown() {
      this.active = false
    },
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
}
</script>

<style scoped lang="scss">
.lang-switcher {
  display: inline-block;
  cursor: pointer;
}

.lang-switcher-mobile {
  margin-top: auto;

  span {
    margin-left: 6px;
  }
}

.lang-list {
  position: absolute;
  list-style: none;
  margin: 0;
  padding: 0 10px;
  background: white;
  box-shadow: $element-box-shadow;
  border: 1px solid rgba(0, 0, 0, 0.39);
  border-radius: 6px;
  right: -1px;
  top: 32px;
  min-width: 120px;
  text-align: right;
  white-space: nowrap;

  &-item {
    margin: 8px 0;
  }
}

.fade-enter,
.fade-leave-to {
  opacity: 0;
  max-height: 0px;
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.4s;
  max-height: 500px;
}
</style>
