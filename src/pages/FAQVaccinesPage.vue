<template>
  <div>
    <div class="custom-container">
      <div class="static-page-wrapper" v-if="faqVaccines && faqVaccines[0]">
        <h1>{{ faqVaccines[0].name }}</h1>
        <div v-html-md="faqVaccines[0].name"></div>
        <vue-fuse
          class="form-control my-4"
          :placeholder="$t('restrictionsPage.searchPlaceholder')"
          :keys="searchKeys"
          :list="faqVaccines[0].faq"
          :defaultAll="true"
          :min-match-char-length="4"
          :threshold="0.3"
          :distance="1000"
          event-name="searchResults"
        ></vue-fuse>
        <div
          v-for="(item, index) in faqVaccines"
          :key="index"
          :id="`faq-${index}`"
        >
          <details v-for="(faq, index) in item.faq" :key="index">
            <summary>{{ faq.question }}</summary>
              <p v-html="faq.answer" />
          </details>
        </div>
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
      searchKeys: ["answer", "question"],
      searchResults: []
    };
  },
  methods: {
    ...mapActions("faqVaccines", {
      fetchOne: "fetchOne",
    }),
  },
  computed: {
    ...mapState("faqVaccines", {
      faqVaccines: "faqVaccines",
    }),
  },
  created() {
    // fetch data
    this.fetchOne(1);
    this.$on("searchResults", (results) => {
      this.searchResults = results;
    });
    // TODO: match all the hits with items from glossary 
    // document.querySelectorAll('[data-term="value"]').setAttribute('title', '...');
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss"></style>