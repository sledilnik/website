import Vue from "vue";

Vue.filter("prefixDiff", function(value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});
