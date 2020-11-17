import Vue from "vue";
import marked from "marked"

Vue.filter("prefixDiff", function (value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});

Vue.filter('limit', function(value, size) {
  if (!value) return ''
  value = value.toString()

  if (value.length <= size) {
    return value
  }
  return value.substr(0, size) + '...'
})

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
