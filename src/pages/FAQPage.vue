<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("faq.pageTitle") }}</h1>
      <span v-html-md="$t('faq.into')" />
      <vue-fuse
        class="form-control my-4"
        placeholder="Vnesite iskani niz"
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

    console.log(this.database);

    this.$on("searchResults", (results) => {
      this.searchResults = results;
    });
  },
  mounted() {
    // open question, if anchor link
    if (this.$route.hash && document.querySelector(this.$route.hash)) {
      document.querySelector(this.$route.hash).parentElement.open = true;
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

.static-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  h1 {
    margin-bottom: 32px;
  }

  .dropdown + h2,
  .dropdown + h3,
  h1 + h2 {
    margin-top: 64px;
  }

  h2,
  h3,
  h4 {
    margin-bottom: 24px;
  }

  h1 {
    font-size: 28px;
  }

  h2 {
    font-size: 21px;
  }

  h3 {
    font-size: 18px;
  }

  p:not(:last-of-type) {
    margin-bottom: 28px;
  }

  //subtitle
  h1 + p > em {
    display: block;
    font-size: 16px;
    font-style: italic;
    color: rgba(0, 0, 0, 0.8);
    font-weight: 400;
    line-height: 1.7;
    margin-bottom: 48px;

    a {
      font-size: 16px;
    }
  }

  p,
  a,
  span,
  strong {
    font-size: 14px;
    color: $text-c;
    line-height: 1.7;
  }

  * + h1,
  * + h2,
  * + h3,
  * + table {
    margin-top: 48px;
  }

  tr + tr {
    margin-top: 27px;
  }

  table {
    width: 100%;
    table-layout: fixed;
    text-align: left;
  }

  table {
    table-layout: fixed;
    text-align: left;
    td {
      padding: 15px 0;
      width: 50%;
      border-top: 1px solid rgba(0, 0, 0, 0.45);
    }
  }

  //dropdown HTML in MD
  h1 + details,
  h2 + details,
  h3 + details {
    margin-top: 48px;
  }

  .img-link {
    display: block;
    box-shadow: none;
    margin-bottom: 24px;

    &:hover {
      box-shadow: none;
    }

    img {
      width: 100%;
    }
  }
}

.static-page-wrapper,
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
