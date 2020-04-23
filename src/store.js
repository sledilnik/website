import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from "d3";
import _ from 'lodash'

import axios from 'axios'
import tableDict from './tableDict.json'
import { format } from 'date-fns'
import { sl } from 'date-fns/locale';

Vue.use(Vuex)

async function exportTime(url) {
  let x = await axios.get(url)
  return new Date(x.data * 1000)
}

// loads csv, converts dates to Date objects, string values to numberical
async function loadCsv(url) {
  const data = await d3.csv(url)
  data.forEach((row) => {
    data.columns.forEach((col) => {
      if (col != "date") {
        row[col] = row[col] === "" ? null : +row[col]
      } else {
        row['date'] = new Date(row['date'])
      }
    })

  })
  return data
}

export function lastChange(data, field, cumulative) {

  const result = {
    lastDay: {
      date: new Date(),
      firstDate: undefined,
      value: undefined,
      diff: undefined,
      percentDiff: undefined
    },
    dayBefore: {
      date: new Date(),
      firstDate: undefined,
      value: undefined,
      diff: undefined,
      percentDiff: undefined
    },
    day2Before: {
      date: new Date(),
      value: undefined,
    }
  }

  let i = data.length - 1

  // console.log("lastDay found in row", i, data[i])

  while (i >= 0 && data[i][field] == null) i--
  result.lastDay.date = new Date(data[i]['date'])
  result.lastDay.value = data[i][field]

  if (cumulative) {
    while (i >= 0 && (result.lastDay.value === data[i][field])) i--
    let date = new Date(data[i + 1]['date'])
    if (data[i + 1][field] != null && result.lastDay.date.getTime() != date.getTime()) {
      result.lastDay.firstDate = date
    }
  } else {
    i--
  }


  if (i >= 0) {
    while (i >= 0 && data[i][field] == null) i--
    result.dayBefore.date = new Date(data[i]['date'])
    result.dayBefore.value = data[i][field]
  }

  if (cumulative) {
    if (i >= 0) {
      while (i >= 0 && result.dayBefore.value === data[i][field]) i--
      let date = new Date(data[i + 1]['date'])
      if (result.dayBefore.date.getTime() != date.getTime()) {
        result.dayBefore.firstDate = date
      }
    }
  } else {
    i--
  }


  if (i >= 0) {
    while (i >= 0 && data[i][field] == null) i--
    if (data[i]) {
      result.day2Before.date = new Date(data[i]['date'])
      result.day2Before.value = data[i][field]
    }
  }


  if (!result.dayBefore.value) {
    result.dayBefore = undefined
  } else {
    result.lastDay.diff = result.lastDay.value - result.dayBefore.value
    result.lastDay.percentDiff = Math.round((result.lastDay.diff / result.dayBefore.value) * 1000) / 10
  }

  if (!result.day2Before.value) {
    result.day2Before = undefined
  } else {
    result.dayBefore.diff = result.dayBefore.value - result.day2Before.value
    result.dayBefore.percentDiff = Math.round((result.dayBefore.diff / result.day2Before.value) * 1000) / 10
  }

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
    lastChange: (state, getters) => (field, cumulative) => {
      return lastChange(getters.data, field, cumulative)
    },
  },
  actions: {
    fetchData: async ({ commit }) => {

      const ts = new Date().getTime()

      const d = await exportTime(`https://raw.githubusercontent.com/sledilnik/data/master/csv/stats.csv.timestamp?nocache=${ts}`)

      const [data, regions] = await Promise.all([
        loadCsv(`https://raw.githubusercontent.com/sledilnik/data/master/csv/stats.csv?nocache=${ts}`),
        d3.csv(`https://raw.githubusercontent.com/sledilnik/data/master/csv/dict-region.csv?nocache=${ts}`),
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

      const ts = new Date().getTime()

      const d = await exportTime(`https://raw.githubusercontent.com/sledilnik/data/master/csv/hospitals.csv.timestamp?nocache=${ts}`)

      let data = await loadCsv(`https://raw.githubusercontent.com/sledilnik/data/master/csv/hospitals.csv?nocache=${ts}`)
      let hospitals = {}
      let rawData = await d3.csv(`https://raw.githubusercontent.com/sledilnik/data/master/csv/dict-hospitals.csv?nocache=${ts}`)

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

const tableData = {
  namespaced: true,
  getters: {
    tableData(state, getters, rootState) {
      return rootState.stats.data.length ? processTableData(rootState.stats.data) : []
    },
    filterTableData: (state, getters) => (dimension) => {
      const items = getters.tableData.filter(day => {
        return dimension.includes(day.dim);
      });
      const sample = items[0];
      const fields = Object.keys(sample).map((dimension, i) => {
        return {
          key: dimension,
          label: dimension,
          stickyColumn: dimension === ' ',
          sortable: true
        };
      }).filter(item => item.key !== 'dim')
      return {
        items,
        fields
      }
    }
  }
}

function processTableData(data) {
  const x = Object.keys(_.last(data)).map(dimension => {
    let newData = {}
    newData['dim'] = dimension
    newData[' '] = tableDict[dimension]

    data.slice().reverse().forEach((day, i) => {
      let date = format(day.date, 'E d.M.', {
        locale: sl
      });
      newData[date] = day[dimension]
    })
    return newData
  }).filter(val => val)
  return x
}

const store = new Vuex.Store({
  modules: {
    stats: statsStore,
    hospitals: hospitalsStore,
    tableData
  }
})

export default store