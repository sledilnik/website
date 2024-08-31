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
          :shouldSort="false"
          :distance="1000"
          event-name="searchResults"
        ></vue-fuse>
        <Collapsable
          v-for="item in searchResults"
          :key="item.slug"
          :id="item.slug"
          :title="item.question"
          :body="item.answer"
        />
        <h2>{{ $t("faqVaccines.glossary") }}</h2>
        <Collapsable
          v-for="item in faqVaccines[lang][0].glossary"
          :key="item.slug"
          :id="item.slug"
          :title="item.term"
          :body="item.definition"
        />
      </div>
      <div v-html-md="$t('faqVaccines.credits')"></div>
      <div v-html-md="$t('faqVaccines.version')"></div>
    </div>
  </div>
</template>

<script>
import { mapActions, mapState } from "vuex";
import Collapsable from "@/components/Collapsable";

export default {
  name: "FAQVaccinesPage",
  components: {
    Collapsable,
  },
  data() {
    return {
      loaded: false,
      lang: localStorage.getItem ("i18nextLng") || 'sl',
      searchKeys: ["answer", "question"],
      searchResults: [],
      tooltipTitle: this.$t('embedMaker.copy'),
    };
  },
  metaInfo() {
    return {
      meta: [
        { vmid: 'og:image', property: 'og:image', content: 'https://' + location.host + '/covid-19-logo--vax-faq.png' },
      ],
    }
  },
  methods: {
    ...mapActions("faqVaccines", {
      fetchOne: "fetchOne",
    }),
    addTooltips() {
      if (this.faqVaccines[this.lang]) {
        this.$el.querySelectorAll("span[data-term]").forEach((el) => {
          for (let entry of this.faqVaccines[this.lang]) {
            for (let term of entry.glossary) {
              if (term.slug === el.getAttribute('data-term')) {
                el.setAttribute("data-definition", term.definition.replace(/<[^>]*>?/gm, ''));
                el.setAttribute("tabindex", 0);
              }
            }
          }
        });
      }
    },
    smoothScroll(e) {
      const offset = -90;
      window.location.hash = e.target.id;
      window.history.pushState(null, null, e.target.hash);
      if (e.target.hash) {
        this.$scrollTo(this.$el.querySelector(this.$route.hash), 500, {
          offset: offset,
        })
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

    if (this.$route.hash) {
      const checker = setInterval(() => {
        const elm = this.$el.querySelector(this.$route.hash)
        if (elm) {
          // element found on page
          clearInterval(checker)
          const offset = -90
          this.$scrollTo(this.$el.querySelector(this.$route.hash), 500, {
            offset: offset,
          })
          this.loaded = true;
          this.addTooltips();
          // open question, if anchor link
          if (document.querySelector(this.$route.hash)) {
            document.querySelector(this.$route.hash).open = true;
          }
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
  position: relative;
  display: inline-flex;
  justify-content: center;
  outline: none;
  cursor: help;
  font-weight: 600;
  transition: all 0.35s ease-in-out;
  color: rgba(0, 0, 0, 0.8);
  &::before {
    content: "";
    width: 100%;
    height: 100%;
    position: absolute;
    top: -2px;
    border-bottom: 2px $yellow dotted;
    z-index: 1;
  }
  &:hover::after {
    content: attr(data-definition);
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
    z-index: 2;
  }
}
</style>
