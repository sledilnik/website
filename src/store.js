import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from "d3";

Vue.use(Vuex)


const statsStore = {
  namespaced: true,
  state: {
    data: []
  },
  getters: {
    data: (state) => {
      return state.data
    },
    getValueOn: (state, getters) => (field, date) => {
      if (!date) {
        return {}
      }
      let searchResult = getters.data.find(day => {
        return new Date(Date.parse(day.date)).setHours(0,0,0,0) === date.getTime()
      })
      return { date, value: searchResult ? searchResult[field] : null }
    },
    getLastValue: (state, getters) => (field) => {
      let result = getters.data.slice().reverse().find(day => {
        return day[field]
      })
      return {
        date: result ? new Date(Date.parse(result.date)) : new Date(new Date().setHours(0, 0, 0, 0)),
        value: result ? result[field] : null
      }
    },
  },
  actions: {
    fetchData: async ({ commit }) => {
      let data = await d3.csv(
        "https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"
      );
      commit('setData', data)
    }
  },
  mutations: {
    setData: (state, data) => {
      state.data = data
    }
  }
}

const hospitalsStore = {
  namespaced: true,
  state: {
    data: []
  },
  getters: {
    data: (state) => {
      return state.data
    },
    getSeries: (state) => (field) => {
      return state.data.map(row => {
        return [
          Date.parse(row['date']),
          row[field]
        ]
      })
    },
    getValueOn: (state, getters) => (field, date) => {
      if (!date) {
        return {}
      }
      let searchResult = getters.data.find(day => {
        return new Date(Date.parse(day.date)).setHours(0,0,0,0) === date.getTime()
      })
      return { date, value: searchResult ? searchResult[field] : null }
    },
    getLastValue: (state, getters) => (field) => {
      let result = getters.data.slice().reverse().find(day => {
        return day[field]
      })
      return {
        date: result ? new Date(Date.parse(result.date)) : new Date(new Date().setHours(0, 0, 0, 0)),
        value: result ? result[field] : null
      }
    },
  },
  actions: {
    fetchData: async ({ commit }) => {
      let data = await d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/hospitals.csv", (row) => {
        Object.keys(row).forEach(col => {
          if (col != 'date') {
            row[col] = +row[col]
          }
        });
        return row
      });
      commit('setData', data)
    }
  },
  mutations: {
    setData: (state, data) => {
      state.data = data
    }
  }
}


const store = new Vuex.Store({
  modules: {
    stats: statsStore,
    hospitals: hospitalsStore,
  }
})

export default store