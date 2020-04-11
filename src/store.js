import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from "d3";

import axios from 'axios'

Vue.use(Vuex)

async function exportTime(url) {
  let x = await axios.get(url)
  return new Date(x.data * 1000)
}

export function lastChange(data, field) {

  const result = {
    lastDay: {
      date: new Date(),
      fistDate: undefined,
      value: undefined,
      diff: undefined,
      percentDiff: undefined
    },
    dayBefore: {
      date: new Date(),
      fistDate: undefined,
      value: undefined,
      diff: undefined,
      percentDiff: undefined
    },
    day2Before: {
      date: new Date(),
      value: undefined,
    }
  }


  // result.lastDay.date = new Date(data[data.length - 1]['date'])
  // result.lastDay.value = data[data.length - 1][field]

  let i = data.length

  while(i > 0 && data[i][field] == null) i--
  result.lastDay.date = new Date(data[i]['date'])
  result.lastDay.value = data[i][field]

  while(i > 0 && result.lastDay.value === data[i][field]) i--
  result.lastDay.fistDate = new Date(data[i+1]['date'])

  while(i > 0 && data[i][field] == null) i--
  result.dayBefore.date = new Date(data[i]['date'])
  result.dayBefore.value = data[i][field]

  while(i > 0 && result.dayBefore.value === data[i][field]) i--
  result.dayBefore.fistDate = new Date(data[i+1]['date'])

  while(i > 0 && data[i][field] == null) i--
  result.day2Before.date = new Date(data[i]['date'])
  result.day2Before.value = data[i][field]

  // while(i > 0 && result.dayBefore.value === data[i][field]) i--
  // result.day2Before.fistDate = new Date(data[i+1]['date'])

  result.lastDay.diff = result.lastDay.value - result.dayBefore.value
  result.lastDay.percentDiff = result.dayBefore.value ? Math.round((result.lastDay.diff / result.dayBefore.value) * 1000) / 10 : 0

  result.dayBefore.diff = result.dayBefore.value - result.day2Before.value
  result.dayBefore.percentDiff = result.day2Before.value ? Math.round((result.dayBefore.diff / result.day2Before.value) * 1000) / 10 : 0

  return result
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
        return new Date(Date.parse(day.date)).setHours(0, 0, 0, 0) === date.getTime()
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
    lastChange: (state, getters) => (field) => {
      return lastChange(getters.data, field)
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
    },
    refreshDataEvery: ({ dispatch }, seconds) => {
      setInterval(() => {
        dispatch('fetchData')
      }, seconds * 1000)
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
        return new Date(Date.parse(day.date)).setHours(0, 0, 0, 0) === date.getTime()
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
    },
    refreshDataEvery: ({ dispatch }, seconds) => {
      setInterval(() => {
        dispatch('fetchData')
      }, seconds * 1000)
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