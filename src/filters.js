import { parseISO, format } from "date-fns";
import sl from "date-fns/locale/sl";

import Vue from "vue";


Vue.filter("prefixDiff", function(value) {
  if (value > 0) {
    return `+${value}`;
  } else {
    return `${value}`;
  }
});


Vue.filter("formatDate", function(value, fmt) {
  
  if (!fmt) {
    fmt = "d. MMMM"
  }

  let date = null

  if (value instanceof Date) {
    date = value
  } else {
    date = parseISO(value)
  }

  return format(date, fmt, {
    locale: sl
  });

});