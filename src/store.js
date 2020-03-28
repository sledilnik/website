import Vue from 'vue'
import Vuex from 'vuex'
import StatsData from "StatsData";
import { parseISO, format } from "date-fns";
import sl from "date-fns/locale/sl";

Vue.use(Vuex)
const store = new Vuex.Store({
  state: {
    csvdata: []
  },
  getters: {
    csvdata: (state) => {
      return state.csvdata
    }
  },
  actions: {
    fetchData: async ({ commit }) => {
      let csvdata = await StatsData.data();
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

function addRadableDatesToDays(data){
  return data.map(day => {
    day.dateLocal = format(parseISO(day.date), "d. MMMM", {
      locale: sl
    });
    return day;
  });
}

export default store