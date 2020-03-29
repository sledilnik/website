import Vue from 'vue'
import Vuex from 'vuex'
import { parseISO, format } from "date-fns";
import sl from "date-fns/locale/sl";
import * as d3 from "d3";

Vue.use(Vuex)
const store = new Vuex.Store({
  state: {
    csvdata: []
  },
  getters: {
    csvdata: (state) => {
      return state.csvdata
    },
    getValueOn: (state, getters) => (field, date) => {
      let searchResult = getters.csvdata.find(day => {
        return Date.parse(day.date) === date.getTime()
      })

      return { date, value: searchResult ? searchResult[field] : 0 }
    },
    getLastValue: (state, getters) => (field) => {
      let result = getters.csvdata.slice().reverse().find(day => {
        return day[field]
      })
      return {
        date: result ? new Date(Date.parse(result.date)) : new Date().setHours(0, 0, 0, 0),
        value: result ? result[field] : 0
      }
    },
  },
  actions: {
    fetchData: async ({ commit }) => {
      let csvdata = await d3.csv(
        "https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"
      );
      csvdata = addRadableDatesToDays(csvdata)
      commit('setCsvData', csvdata)
    }
  },
  mutations: {
    setCsvData: (state, csvdata) => {
      state.csvdata = csvdata
    }
  }
})

function addRadableDatesToDays(data) {
  return data.map(day => {
    day.dateLocal = format(parseISO(day.date), "d. MMMM", {
      locale: sl
    });
    return day;
  });
}

export default store