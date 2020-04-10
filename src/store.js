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
  
  const lastLastValue = data.slice().reverse().find(row => {
    return row[field]
  })

  if (!lastLastValue) {
    return {
      lastDay: {
        date: new Date(),
        value: 0,
        diff: 0,
        percentDiff: 0
      },
      dayBefore: {
        date: new Date(),
        value: 0,
        diff: 0,
        percentDiff: 0
      },
    }
  }

  const lastValue = data.slice().reverse().find(row => {
    return row[field] == lastLastValue[field]
  })

  const previousValue = data.slice().reverse().find(row => {
    return row[field] != lastValue[field] && Date.parse(row['date']) < Date.parse(lastValue['date'])
  })
  const prePreviousValue = data.slice().reverse().find(row => {
    return row[field] != previousValue[field] && Date.parse(row['date']) < Date.parse(previousValue['date'])
  })

  const lastDayValue =  lastValue[field] || null
  const dayBeforeValue =  previousValue[field] || null

  const lastDayDiff = lastValue[field] - previousValue[field]
  const dayBeforeDiff = previousValue[field] - prePreviousValue[field]
  
  const lastDayPercentDiff = lastDayDiff != 0 ? Math.round((lastDayDiff / dayBeforeValue) * 1000) / 10 : 0

  return {
    lastDay: {
      date: lastValue ? new Date(Date.parse(lastValue.date)) : null,
      value: lastDayValue,
      diff: lastDayDiff,
      percentDiff: lastDayPercentDiff,
    },
    dayBefore: {
      date: previousValue ? new Date(Date.parse(previousValue.date)) : null, 
      value: dayBeforeValue,
      diff: dayBeforeDiff,
    }
  }
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