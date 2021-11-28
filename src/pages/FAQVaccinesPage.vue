<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("faqVaccines.title") }}</h1>
      <div v-html-md="$t('faqVaccines.description')"></div>
      <div v-if="faqVaccines[lang][0]">
        <vue-fuse
          class="vue-fuse-search form-control my-4"
          :placeholder="$t('restrictionsPage.searchPlaceholder')"
          :keys="searchKeys"
          :list="faqVaccines[lang][0].faq"
          :defaultAll="true"
          :min-match-char-length="4"
          :threshold="0.3"
          :distance="1000"
          event-name="searchResults"
        ></vue-fuse>
        <details
          v-for="item in searchResults"
          :key="item.slug"
          :id="item.slug"
          @click="smoothScroll">
          <summary :id="item.slug">{{ item.question }}</summary>
          <p v-html-md="item.answer" />
        </details>
        <h2>{{ $t("faqVaccines.glossary") }}</h2>
        <details
          v-for="item in faqVaccines[lang][0].glossary"
          :key="item.slug"
          :id="item.slug"
          @click="smoothScroll">
          <summary :id="item.slug">{{ item.term }}</summary>
          <p v-html-md="item.definition" />
        </details>
      </div>
      <div v-html-md="$t('faqVaccines.credits')"></div>
      <div v-html-md="$t('faqVaccines.version')"></div>
    </div>
  </div>
</template>

<script>
import { mapActions, mapState } from "vuex";

export default {
  name: "FAQVaccinesPage",
  components: {
  },
  data() {
    return {
      loaded: false,
      lang: localStorage.getItem ("i18nextLng") || 'sl',
      searchKeys: ["answer", "question"],
      searchResults: [],
    };
  },
  methods: {
    ...mapActions("faqVaccines", {
      fetchOne: "fetchOne",
    }),
    addTooltips() {
      this.$el.querySelectorAll("span[data-term]").forEach((el) => {
        for (let entry of this.faqVaccines[this.lang]) {
          for (let term of entry.glossary) {
            if (term.slug === el.getAttribute('data-term')) {
              el.setAttribute("title", term.definition.replace(/<[^>]*>?/gm, ''));
              el.setAttribute("tabindex", 0);
            }
          }
        }
      });
    },
    smoothScroll(e) {
      const offset = -90;
      window.location.hash = e.target.id;
      window.history.pushState(null, null, e.target.hash);
      this.$scrollTo(document.querySelector(this.$route.hash), 500, {
        offset: offset,
      })
    },
  },
  computed: {
    ...mapState("faqVaccines", {
      faqVaccines: "faqVaccines",
    }),
  },
  mounted() {
    if (this.$route.hash) {
      const checker = setInterval(() => {
        const elm = document.querySelector(this.$route.hash)
        if (elm) {
          // element found on page
          clearInterval(checker)
          const offset = -90
          this.$scrollTo(document.querySelector(this.$route.hash), 500, {
            offset: offset,
          })
          this.loaded = true
        }
      }, 100)

      setTimeout(() => {
        clearInterval(checker)
      }, 5000)
    }
  },
  created() {
    // fetch data
    this.fetchOne(1);
    this.$on("searchResults", (results) => {
      this.searchResults = results;
      this.addTooltips();
    });
  },
  watch: {
    loaded: function () {
      this.addTooltips();
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
span[data-term] {
  cursor: help;
  font-weight: 600;
  transition: all 0.35s ease-in-out;
  box-shadow: inset 0 -1px 0 white, inset 0 -4px $yellow;
  text-decoration: none;
  color: rgba(0, 0, 0, 0.8);

  &:hover {
    text-decoration: none;
    color: rgba(0, 0, 0, 0.8);
    font-weight: 600;
    box-shadow: inset 0 -1px 0 white, inset 0 -20px $yellow;
  }
}
@media (pointer: coarse), (hover: none) {
  span[data-term] {
    position: relative;
    display: inline-flex;
    justify-content: center;
    outline: none;
  }
  span[data-term]:focus::after {
    content: attr(title);
    position: absolute;
    top: 90%;
    width: 200px;
    background-color: #414040;
    color: white;
    border: 1px solid;
    padding: 3px 6px;
    margin: 10px;
    font-size: 10px;
    font-weight: 200;
    line-height: 1.4;
    z-index: 1;
  }
}
</style>