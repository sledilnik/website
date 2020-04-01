import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from "d3";

import axios from 'axios'

Vue.use(Vuex)

async function exportTime(url) {
  let x = await axios.get(url)
  return new Date(x.data * 1000)
}

const statsStore = {
  namespaced: true,
  state: {
    exportTime: null,
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
      const d = await exportTime("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv.timestamp")

      const [data, regions] = await Promise.all([
        d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"),
        d3.csv("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/dict-region.csv"),
      ]);
      commit('setData', data)
      commit('setRegions', regions)
      commit('setExportTime', d)
    }
  },
  mutations: {
    setData: (state, data) => {
      state.data = data
      state.loaded = true
    },
    setRegions: (state, regions) => {
      state.regions = regions
    },
    setExportTime: (state, exportTime) => {
      state.exportTime = exportTime
    }
  }
}

const hospitalsStore = {
  namespaced: true,
  state: {
    loaded: false,
    exportTime: null,
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

      const d = await exportTime("https://raw.githubusercontent.com/slo-covid-19/data/master/csv/hospitals.csv.timestamp")

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

      commit('setData', data)
      commit('setHospitals', hospitals)
      commit('setExportTime', d)
    }
  },
  mutations: {
    setData: (state, data, hospitals) => {
      state.loaded = true
      state.data = data
      state.hospitals = hospitals
    },
    setHospitals: (state, hospitals) => {
      state.loaded = true
      state.hospitals = hospitals
    },
    setExportTime: (state, d) => {
      state.loaded = true
      state.exportTime = d
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