import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from "d3";

Vue.use(Vuex)


const statsStore = {
  namespaced: true,
  state: {
    loaded: false,
    data: [],
    regions: []
  },
  getters: {
    data: (state) => {
      return state.data
    },
    regions: (state) => {
      return state.regions
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
      const [data, regions] = await Promise.all([
        d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"),
        d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/dict-region.csv"),
      ]);
      commit('setData', data)
      commit('setRegions', regions)
    }
  },
  mutations: {
    setData: (state, data) => {
      state.data = data
      state.loaded = true
    },
    setRegions: (state, regions) => {
      state.regions = regions
    }
  }
}

const hospitalsStore = {
  namespaced: true,
  state: {
    loaded: false,
    data: [],
    hospitals: {}
  },
  getters: {
    data: (state) => {
      return state.data
    },
    hospitals: (state) => {
      return state.hospitals
    },
    hospitalName: (state) => (id) => {
      return state.hospitals[id]
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
      

      let hospitals = {}
      let rawData = await d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/dict-hospitals.csv")

      rawData.forEach(row => {
        hospitals[row.id] = row.name
      })

      commit('setData', data, hospitals)
    }
  },
  mutations: {
    setData: (state, data, hospitals) => {
      state.loaded = true
      state.data = data
      state.hospitals = hospitals
    },
  }
}


const store = new Vuex.Store({
  modules: {
    stats: statsStore,
    hospitals: hospitalsStore,
  }
})

export default store