<template>
  <div>
    <Time-stamp :date="lastUpdate" />

    <div class="custom-container">
      <div class="static-page-wrapper">
        <h1>
          {{ $t("restrictionsPage.title") }}
        </h1>
        <div v-html-md="$t('restrictionsPage.description')"></div>

        <vue-fuse
          class="form-control my-4"
          :placeholder="$t('restrictionsPage.searchPlaceholder')"
          :keys="searchKeys"
          :list="restrictions"
          :defaultAll="true"
          :min-match-char-length="4"
          :threshold="0.3"
          :distance="1000"
          event-name="searchResults"
        ></vue-fuse>
        <!-- body -->
        <details
          v-for="item in searchResults"
          :key="item.id"
          :id="`restriction-${item.id}`"
        >
          <summary>
            {{ item.title }}<br />
            <span v-html-md="item.rule" />
          </summary>
          <div>
            <p>
              <b>{{ $t("restrictionsPage.geoValidity") }}:</b>
              <span v-html-md="item.regions" />
            </p>
            <p>
              <b>{{ $t("restrictionsPage.validity") }}:</b>&nbsp;
              <span v-if="item.valid_since"
                >od {{ item.valid_since | date("dd. MM. yyyy") }}</span
              >
              <span v-if="item.valid_until">
                do {{ item.valid_until | date("dd. MM. yyyy") }}</span
              >
              <span
                v-if="item.validity_comment"
                v-html-md="item.validity_comment"
              />
            </p>
            <p>
              <b>{{ $t("restrictionsPage.exceptions") }}:</b><br />
              <span v-html-md="item.exceptions" />
            </p>
            <p>
              <b>{{ $t("restrictionsPage.extrarule") }}:</b>
              <span v-html-md="item.extra_rules" />
            </p>
            <p>
              <b>{{ $t("restrictionsPage.notes") }}:</b>
              <span v-html-md="item.comments" />
            </p>
            <p>
              <a :href="item.legal_link" targe="_blank">{{
                $t("restrictionsPage.link")
              }}</a>
            </p>
          </div>
        </details>
      </div>
      <!-- <FloatingMenu :list="floatingMenu" title="Ukrepi" /> -->
    </div>
  </div>
</template>

<script>
import { mapActions, mapState } from "vuex";
import TimeStamp from "components/TimeStamp";

export default {
  name: "RestrictionsPage",
  components: {
    TimeStamp,
  },
  data() {
    return {
      searchKeys: ["title", "rule", "validity_comment", "extra_rules", "exceptions", "comments"],
      searchResults: [],
      floatingMenu: [],
    };
  },
  methods: {
    ...mapActions("restrictions", {
      fetchAll: "fetchAll",
    }),
  },
  computed: {
    ...mapState("restrictions", {
      restrictions: "restrictions",
      lastUpdate: "lastUpdate",
    }),
  },
  created() {
    // fetch data
    this.fetchAll();
    this.$on("searchResults", (results) => {
      this.searchResults = results;
    });
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss"></style>