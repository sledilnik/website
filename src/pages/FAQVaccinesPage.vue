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
          :key="`faq-${item.slug}`"
          :id="`faq-${item.slug}`">
          <summary>{{ item.question }}</summary>
          <p v-html-md="item.answer" />
        </details>
        <h2>{{ $t("faqVaccines.glossary") }}</h2>
        <vue-fuse
          class="vue-fuse-search form-control my-4"
          :placeholder="$t('restrictionsPage.searchPlaceholder')"
          :keys="searchKeysGlossary"
          :list="faqVaccines[lang][0].glossary"
          :defaultAll="true"
          :min-match-char-length="4"
          :threshold="0.3"
          :distance="1000"
          event-name="searchResultsGlossary"
        ></vue-fuse>
        <details
          v-for="item in searchResultsGlossary"
          :key="`glossary-${item.slug}`"
          :id="`glossary-${item.slug}`">
          <summary>{{ item.term }}</summary>
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
      searchKeysGlossary: ["term", "definition"],
      searchResults: [],
      searchResultsGlossary: []
    };
  },
  methods: {
    ...mapActions("faqVaccines", {
      fetchOne: "fetchOne",
    }),
    addTooltips() {
      if (this.lang && this.faqVaccines[this.lang]) {
        this.$el.querySelectorAll("span[data-term]").forEach((el) => {
          for (let entry of this.faqVaccines[this.lang]) {
            for (let term of entry.glossary) {
              if (term.slug === el.getAttribute('data-term')) {
                el.setAttribute("title", term.definition.replace(/<[^>]*>?/gm, ''));
              }
            }
          }
        });
      }
    },
  },
  computed: {
    ...mapState("faqVaccines", {
      faqVaccines: "faqVaccines",
    }),
  },
  mounted() {
    let checker = setInterval(() => {
      let elm = document.querySelector(".vue-fuse-search");
      if (elm) {
        this.loaded = true;
        clearInterval(checker);
      }
    }, 80);
  },
  created() {
    // fetch data
    this.fetchOne(1);
    this.$on("searchResults", (results) => {
      this.searchResults = results;
      this.addTooltips();
    });
    this.$on("searchResultsGlossary", (results) => {
      this.searchResultsGlossary = results;
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
</style>