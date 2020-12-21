<template>
  <div class="custom-container">
    <div class="faq-page-wrapper">
      <h1>{{ $t("faq.pageTitle") }}</h1>
      <div class="intro" v-html-md="$t('faq.intro')" />
      <vue-fuse
        class="form-control my-4"
        :placeholder="$t('faq.searchPlaceholder')"
        :keys="searchKeys"
        :list="database"
        :defaultAll="false"
        :min-match-char-length="4"
        :threshold="0.3"
        :distance="1000"
        event-name="searchResults"
      ></vue-fuse>

      <Collapsable
        v-for="item in searchResults"
        :key="item.id"
        :id="item.id"
        :title="item.question"
        :body="item.answer"
      />
      <div v-if="searchResults.length == 0">
        <div v-for="section in faq" :key="section.id">
          <h2>{{ $t(`faq.section.${section.id}`) }}</h2>
          <Collapsable
            v-for="id in section.ids"
            :key="id"
            :id="id"
            :title="$t(`faq.${id}.question`)"
            :body="$t(`faq.${id}.answer`)"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Collapsable from "@/components/Collapsable";
import faq from "@/content/faq.js";

export default {
  name: "StaticPage",
  components: {
    Collapsable,
  },
  data() {
    return {
      faq: faq,
      database: [],
      searchKeys: ["question", "answer"],
      searchResults: [],
    };
  },
  created() {
    faq.forEach((section) => {
      section.ids.forEach((id) => {
        this.database.push({
          id: id,
          section: this.$t(`faq.section.${section.id}`),
          question: this.$t(`faq.${id}.question`),
          answer: this.$t(`faq.${id}.answer`),
        });
      });
    });

    this.$on("searchResults", (results) => {
      this.searchResults = results;
    });
  },
  mounted() {
    // open question, if anchor link
    if (this.$route.hash && document.querySelector(this.$route.hash)) {
      document.querySelector(this.$route.hash).open = true;
    }
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.custom-container {
  margin: 24px auto 0 auto;
  max-width: 730px;

  @media only screen and (min-width: 768px) {
    margin: 48px auto 65px auto;
    box-shadow: $element-box-shadow;
  }
}

.faq-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;
  font-size: 14px;
  color: $text-c;
  line-height: 1.7;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  h1 {
    margin-bottom: 32px;
    font-size: 28px;
  }

  h2 {
    font-size: 21px;
    margin-bottom: 32px;
  }

  //subtitle
  .intro {
    font-size: 16px;
    color: rgba(0, 0, 0, 0.8);
    font-weight: 400;
    line-height: 1.7;
    margin-bottom: 48px;

    a {
      font-size: 16px;
    }
  }

  // p,
  // a,
  // span,
  // strong {
  //   font-size: 14px;
  //   color: $text-c;
  //   line-height: 1.7;
  // }

  // * + h1,
  // * + h2,
  // * + h3,
  // * + table {
  //   margin-top: 48px;
  // }
}

.faq-page-wrapper,
.footnote,
.link {
  a {
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

    strong {
      font-weight: 600;
    }
  }
}
</style>
