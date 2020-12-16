<template>
  <div @click="checkClick($event)">
    <Time-stamp :date="exportTime" />
    <b-container class="stats-page">
      <b-row cols="12">
        <b-col>
          <div id="visualizations" class="visualizations"></div>
        </b-col>
      </b-row>
    </b-container>
    <FloatingMenu :list="charts" :title="$t('navbar.goToGraph')" />
  </div>
</template>

<script>
import { mapState } from "vuex";
import TimeStamp from "components/TimeStamp";
import FloatingMenu from "components/FloatingMenu";
import { Visualizations } from "visualizations/App.fsproj";
import chartsFloatMenu from "components/floatingMenuDict";
import { API_ENDPOINT_BASE } from '../services/api.service';

export default {
  name: "WorldStatsPage",
  components: {
    TimeStamp,
    FloatingMenu,
  },
  data() {
    return {
      loaded: false,
      charts: [],
    };
  },
  mounted() {
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations(
        "visualizations",
        "world",
        this.$route.query,
        API_ENDPOINT_BASE
      );
    });

    // stupid spinner impl, but i do not know better (charts are react component, no clue when they are rendered)
    let checker = setInterval(() => {
      let elm = document.querySelector(".highcharts-point");
      if (elm) {
        document.querySelector(".stats-page").classList.add("loaded");
        this.loaded = true;
        clearInterval(checker);
      }
    }, 80);
  },
  computed: {
    ...mapState("stats", {
      exportTime: "exportTime",
    }),
  },
  methods: {
    checkClick(e) {
      const shareDropdowns = this.$el.querySelectorAll(
        ".share-dropdown-wrapper"
      );

      // ignore click if the clicked element is share button or its icon or caption
      if (e.target.classList.contains("share-button-")) return;

      // else check if any of the dropdowns is opened and close it/them
      shareDropdowns.forEach((el) => {
        el.classList.contains("show")
          ? el.classList.remove("show")
          : el.classList.add("hide");
      });

      // TODO: there is still an issue where if you immediately click on the same
      // share button again, it won't open the dropdown because ShareButton.fs
      // component is not aware that the dropdown was closed within this method.
      // I think the right way to do this would be to listen for clicks within App.fs
    },
    getCharts() {
      this.$el.querySelectorAll(".visualization-chart h2 a").forEach((el) => {
        const key = el.getAttribute("href").substring(1);
        const item = chartsFloatMenu[key];
        this.charts.push({
          title: item && item.titleKey ? this.$t(item.titleKey) : el.innerHTML,
          link: key,
          icon: item ? item.icon : undefined,
        });
      });
    },
  },
  watch: {
    loaded: function (newValue, oldValue) {
      this.getCharts();
    },
  },
};
</script>

<style lang="sass"></style>
