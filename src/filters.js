import Vue from "vue";
import marked from "marked"
import { formatNumber } from "./i18n";

Vue.filter("prefixDiff", function (value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});

Vue.filter("number", function(value) {
  return formatNumber(value);
});

Vue.filter("percent", function(value) {
  return formatNumber(value / 100, {
    style: "percent",
    minimumFractionDigits: 1,
    maximumFractionDigits: 2,
    signDisplay: "always",
  });
});

/**
 * markdown filter
 * you can use it in Vue template and it will render markdown in user's browser
 * <div v-html="$options.filters.marked($t('charts.ostanizdrav.description'))"></div>
 * You have to use it with v-html directive, because moustache templating (stuf with `{{ ... }}`) will escape generated HTML
 * 
 * @deprecated use htmlMd directive (<div v-html-md="$t('charts.ostanizdrav.description')"></div>)
 */
Vue.filter("marked", function (input) {
  return marked(input)
})
