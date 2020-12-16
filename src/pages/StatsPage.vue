<template>
  <div @click="checkClick($event)">
    <Time-stamp :date="exportTime" />
    <b-container class="stats-page">
      <div class="cards-wrapper">
        <!--
        <Info-card
          :title="$t('infocard.tests')"
          field="tests.performed.today"
          good-trend="up"
          series-type="state"
        />
-->
        <Info-card
          :title="$t('infocard.confirmedToDate')"
          field="cases.confirmedToDate"
          name="cases.confirmedToDate"
          running-sum-field="cases.confirmedToday"
          series-type="state"
        />
        <!--
        <Info-card
          :title="$t('infocard.recoveredToDate')"
          field="cases.recoveredToDate"
          good-trend="up"
          series-type="state"
        />
-->
        <Info-card
          :title="$t('infocard.active')"
          field="cases.active"
          field-new-cases="cases.confirmedToday"
          field-deceased="statePerTreatment.deceased"
          name="cases.active"
          running-sum-field="cases.confirmedToday"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.newCases7d')"
          field="cases.active"
          name="newCases7d"
          running-sum-field="cases.confirmedToday"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.inHospital')"
          field="statePerTreatment.inHospital"
          total-in="total.inHospital.in"
          total-out="total.inHospital.out"
          total-deceased="total.deceased.hospital.today"
          name="statePerTreatment.inHospital"
          running-sum-field="total.inHospital.in"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.icu')"
          field="statePerTreatment.inICU"
          total-in="total.icu.in"
          total-out="total.icu.out"
          total-deceased="total.deceased.hospital.icu.today"
          name="statePerTreatment.inICU"
          running-sum-field="total.icu.in"
          series-type="state"
        />
        <Info-card
          :title="$t('infocard.deceasedToDate')"
          field="statePerTreatment.deceasedToDate"
          name="statePerTreatment.deceasedToDate"
          running-sum-field="statePerTreatment.deceased"
          series-type="state"
        />
      </div>
      <div class="posts d-flex" v-if="lastestTwoPosts && lastestTwoPosts.length">
          <PostTeaser class="col-md-6 p-0 p-md-3" v-for="post in lastestTwoPosts" :post="post" :key="post.id" />
      </div>
      <div class="posts d-flex" v-else>
          <PostTeaserSkeleton class="col-md-6 p-0 p-md-3" v-for="i in 2" :key="i" />
      </div>
      <b-row cols="12">
        <b-col>
          <div id="visualizations" class="visualizations"></div>
        </b-col>
      </b-row>
      <b-row cols="12">
        <b-col>
          <Youtube id="R_VftBj375I"></Youtube>
        </b-col>
      </b-row>
    </b-container>
    <FloatingMenu :list="charts" :title="$t('navbar.goToGraph')" />
  </div>
</template>

<script>
import { mapGetters, mapState } from "vuex";
import InfoCard from "components/cards/InfoCard";
import PostTeaser from "components/cards/PostTeaser";
import PostTeaserSkeleton from "components/cards/PostTeaserSkeleton";
import TimeStamp from "components/TimeStamp";
import Notice from "components/Notice";
import Posts from "components/Posts";
import Youtube from "components/Youtube";
import FloatingMenu from "components/FloatingMenu";
import { Visualizations } from "visualizations/App.fsproj";
import chartsFloatMenu from "components/floatingMenuDict";
import { API_ENDPOINT_BASE } from '../services/api.service';

export default {
  name: "StatsPage",
  components: {
    InfoCard,
    TimeStamp,
    PostTeaser,
    PostTeaserSkeleton,
    Youtube,
    FloatingMenu,
  },
  data() {
    return {
      loaded: false,
      charts: [],
      headerTeasers: false
    };
  },
  created(){
    this.$store.dispatch('posts/fetchLatestPosts')
  },
  mounted() {
    this.$nextTick(() => {
      // must use next tick, so whole DOM is ready and div#id=visualizations exists
      Visualizations(
        "visualizations",
        "local",
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
    ...mapGetters('posts', [
      'lastestTwoPosts'
    ])
  },
  methods: {
    checkClick(e) {
      const dropdownAll = this.$el.querySelectorAll(".share-dropdown-wrapper");

      // ignore click if the clicked element is share button or its icon or caption
      if (e.target.classList.contains("share-button-")) return;

      // else check if any of the dropdowns is opened and close it/them
      dropdownAll.forEach((el) => {
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
    loaded: function () {
      this.getCharts();
    },
  },
};
</script>

<style lang="sass">

$loader-width: 50px

.posts
  margin: 0px auto 44px
  min-height: 179px
  @media only screen and (min-width: 768px)
    margin: 0px auto 88px
  @media only screen and (max-width: 480px)
    flex-direction: column
    min-height: 247px
    

.cards-wrapper
  display: grid
  gap: 15px
  grid-template-columns: repeat(1, minmax(165px, 1fr))
  margin: 0px auto 44px

  @media only screen and (min-width: 480px)
    grid-template-columns: repeat(2, minmax(165px, 1fr))

  @media only screen and (min-width: 768px)
    gap: 30px
    margin: 0px 15px 88px

  @media only screen and (min-width: 992px)
    grid-template-columns: repeat(3, minmax(165px, 1fr))

.stats-page
  margin-top: 48px
  position: relative

  &.loaded
    section
      &::before,
      &::after
        display: none

  section
    position: relative

    &::before,
    &::after
      content: ""
      z-index: 90
      display: block
      position: absolute
      top: calc(50% - 25px)
      left: calc(50% - 25px)
      width: $loader-width
      height: $loader-width
      background-size: cover

    &::after
      background-image: url(../assets/svg/covid-animation-1.svg)

      animation: rotate1 3s infinite
      animation-timing-function: linear

      @keyframes rotate1
        0%
          transform: rotate(0deg) scale(1)
        12.5%
          transform: rotate(45deg) scale(1.3)
        25%
          transform: rotate(90deg) scale(1)
        37.5%
          transform: rotate(135deg) scale(1.3)
        50%
          transform: rotate(180deg) scale(1)
        62.5%
          transform: rotate(225deg) scale(1.3)
        75%
          transform: rotate(270deg) scale(1)
        87.5%
          transform: rotate(315deg) scale(1.3)
        100%
          transform: rotate(360deg) scale(1)

    &::before
      background-image: url(../assets/svg/covid-animation-2.svg)
      animation: rotate2 3s infinite
      animation-timing-function: linear

      @keyframes rotate2
        0%
          transform: rotate(0deg) scale(1)
        50%
          transform: rotate(-180deg) scale(1)
        100%
          transform: rotate(-360deg) scale(1)
</style>
