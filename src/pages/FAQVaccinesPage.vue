<template>
  <div>
    <div class="custom-container">
      <div class="static-page-wrapper">
        <h1>
          {{ $t("faqVaccines.title") }}
        </h1>
        <div v-html-md="$t('faqVaccines.description')"></div>
        <vue-fuse
          v-if="faqVaccines[0]"
          class="vue-fuse-search form-control my-4"
          :placeholder="$t('restrictionsPage.searchPlaceholder')"
          :keys="searchKeys"
          :list="faqVaccines[0].faq"
          :defaultAll="true"
          :min-match-char-length="4"
          :threshold="0.3"
          :distance="1000"
          event-name="searchResults"
        ></vue-fuse>
        <details v-for="(item, index) in searchResults" :key="index">
          <summary>{{ item.question }}</summary>
            <p v-html-md="item.answer" />
        </details>
        <div v-html-md="$t('faqVaccines.credits')"></div>
      </div>
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
      searchKeys: ["answer", "question"],
      searchResults: []
    };
  },
  methods: {
    ...mapActions("faqVaccines", {
      fetchOne: "fetchOne",
    }),
    addTooltips() {
      this.$el.querySelectorAll("span[data-term]").forEach((el) => {
        for (let faq of this.faqVaccines) {
          for (let term of faq.glossary) {
            if (term.slug === el.getAttribute('data-term')) {
              el.setAttribute("title", term.definition);
            }
          }
        }
      });
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