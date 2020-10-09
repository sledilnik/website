<template>
  <div class="visualizations container">
    <section id="published">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#published">{{
              $t("charts.ostanizdrav.published.title")
            }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-published`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="publishedVis"></div>
    </section>
    <section id="usercount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#usercount">{{
              $t("charts.ostanizdrav.usercount.title")
            }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-usercount`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="usercountVis"></div>
    </section>
    <section id="userPublishedByCount">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#userPublishedByCount">{{
              $t("charts.ostanizdrav.userPublishedByCount.title")
            }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-userPublishedByCount`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="userPublishedByCountVis"></div>
    </section>
    <section id="publishedByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#publishedByRisk">{{
              $t("charts.ostanizdrav.publishedByRisk.title")
            }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-publishedByRisk`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="publishedByRiskVis"></div>
    </section>
    <section id="validByRisk">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#validByRisk">{{
              $t("charts.ostanizdrav.validByRisk.title")
            }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-validByRisk`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="validByRiskVis"></div>
    </section>
    <section id="valid">
      <div class="title-chart-wrapper">
        <div class="title-brand-wrapper">
          <h2>
            <a href="#valid">{{ $t("charts.ostanizdrav.valid.title") }}</a>
          </h2>
        </div>
        <div class="faq-and-share-wrapper">
          <div class="faq-link-wrapper">
            <div>
              <a
                class="faq-link"
                target="_blank"
                :href="`/${$i18n.i18next.language}/faq#ostanizdrav-valid`"
                >?</a
              >
            </div>
          </div>
        </div>
      </div>
      <div id="validVis"></div>
    </section>
  </div>
</template>

<script>
import embed from "vega-embed";
import vega from "vega";

import validPlot from "@/vega/valid";
import validByRiskPlot from "@/vega/validByRisk";
import userPublishedByCountPlot from "@/vega/userPublishedByCount";
import usercountPlot from "@/vega/usercount";
import publishedByRiskPlot from "@/vega/publishedByRisk";
import publishedPlot from "@/vega/published";

import i18n from "@/i18n";

export default {
  name: "OstaniZdravPage",
  async mounted() {
    let $t = (id) => i18n.i18next.t(id);

    // https://github.com/vega/vega-embed#options
    let opts = {
      actions: false,
      timeFormatLocale: { // https://github.com/d3/d3-time-format/blob/master/locale/de-DE.json
        // dateTime: "%A, der %e. %B %Y, %X",
        // date: "%d.%m.%Y",
        // time: "%H:%M:%S",
        periods: ["AM", "PM"], //not used, but required
        days: [
          $t("weekday.0"),//"So",
          $t("weekday.1"),//"Mo",
          $t("weekday.2"),//"Di",
          $t("weekday.3"),//"Mi",
          $t("weekday.4"),//"Do",
          $t("weekday.5"),//"Fr",
          $t("weekday.6"),//"Sa"
        ],
        shortDays: [
          $t("weekdayShort.0"),//"So",
          $t("weekdayShort.1"),//"Mo",
          $t("weekdayShort.2"),//"Di",
          $t("weekdayShort.3"),//"Mi",
          $t("weekdayShort.4"),//"Do",
          $t("weekdayShort.5"),//"Fr",
          $t("weekdayShort.6"),//"Sa"
        ],
        months: [
          $t("month.0"),//"Jan",
          $t("month.1"),//"Feb",
          $t("month.2"),//"Mrz",
          $t("month.3"),//"Apr",
          $t("month.4"),//"Mai",
          $t("month.5"),//"Jun",
          $t("month.6"),//"Jul",
          $t("month.7"),//"Aug",
          $t("month.8"),//"Sep",
          $t("month.9"),//"Okt",
          $t("month.10"),//"Nov",
          $t("month.11"),//"Dez",
        ],
        shortMonths: [
          $t("shortMonth.0"),//"Jan",
          $t("shortMonth.1"),//"Feb",
          $t("shortMonth.2"),//"Mrz",
          $t("shortMonth.3"),//"Apr",
          $t("shortMonth.4"),//"Mai",
          $t("shortMonth.5"),//"Jun",
          $t("shortMonth.6"),//"Jul",
          $t("shortMonth.7"),//"Aug",
          $t("shortMonth.8"),//"Sep",
          $t("shortMonth.9"),//"Okt",
          $t("shortMonth.10"),//"Nov",
          $t("shortMonth.11"),//"Dez",
        ],
      },
      formatLocale: {
        "decimal":  $t("charts.common.decimalPoint"),
        "thousands": $t("charts.common.thousandsSep"),
        "grouping": [3],
      }, // https://github.com/d3/d3-format/blob/master/locale/en-US.json
    };

    embed("#validVis", validPlot($t), opts);
    embed("#publishedVis", publishedPlot($t), opts);
    embed("#validByRiskVis", validByRiskPlot($t), opts);
    embed("#publishedByRiskVis", publishedByRiskPlot($t), opts);
    embed("#usercountVis", usercountPlot($t), opts);
    embed("#userPublishedByCountVis", userPublishedByCountPlot($t), {
      actions: false,
    });
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss" scoped>
.visualizations {
  display: flex;
  flex-direction: column;
  justify-content: center;
  margin: 0 auto !important;
  max-width: 930px;

  section {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
}

.vega-embed {
  display: flex;
  justify-content: center;
}
</style>
