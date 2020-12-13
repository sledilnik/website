<template>
  <section :id="name">
    <div class="title-chart-wrapper">
      <div class="title-brand-wrapper">
        <h2>
          <a :href="`#${name}`" @click="smoothScroll">{{ title }}</a>
        </h2>
      </div>
      <div class="faq-and-share-wrapper">
        <div class="faq-link-wrapper">
          <div>
            <a
              class="faq-link"
              target="_blank"
              :href="`/${$i18n.i18next.language}/faq#${faqAnchor}`"
              >?</a
            >
          </div>
        </div>
      </div>
    </div>
    <div :id="visualizationId"></div>
    <div class="disclaimer" v-html-md="description"></div>
  </section>
</template>

<script>
import opts from "@/vega/opts";

export default {
  name: "VegaChart",
  props: {
    name: {
      type: String,
      required: true,
    },
    title: {
      type: String,
      required: true,
    },
    description: {
      type: String,
      required: true,
    },
    faqAnchor: {
      type: String,
      required: true,
    },
    data: {
      type: Object,
      reuqired: true,
    },
  },
  computed: {
    visualizationId() {
      return `${this.toCamel(this.name.replace("-chart", ""))}Vis`;
    },
  },
  async mounted() {
    import('vega-embed').then(embed => {
      embed.default(document.getElementById(this.visualizationId), this.data, opts);
    })
  },
  methods: {
    toCamel(s) {
      return s.replace(/([-_][a-z])/gi, ($1) => {
        return $1
          .toUpperCase()
          .replace("-", "")
          .replace("_", "");
      });
    },
    smoothScroll(e) {
      e.preventDefault();
      const offset = -100;
      const position =
        e.target.getBoundingClientRect().top + window.pageYOffset + offset;
      window.scrollTo({ top: position, behavior: "smooth" });
      window.history.pushState(null, null, e.target.hash);
    },
  },
};
</script>

<style lang="scss" scoped>
section {
  display: flex;
  flex-direction: column;
  justify-content: center;
  min-height: 433px;
}

.title-chart-wrapper {
  border-bottom: none;

  .faq-and-share-wrapper {
    display: none;
  }
}

.vega-embed {
  display: flex;
  justify-content: center;
}
</style>
