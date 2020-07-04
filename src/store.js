import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from 'd3'
import _ from 'lodash'
import ApiService from './services/api.service'
import regions from './services/dict.regions.json'
import axios from 'axios'

Vue.use(Vuex)

// TODO: for some reason tests fail if store is defined modularly like in store/index.js
// Keeping this file just so that tests don't fail until we figure this out.

async function exportTime(url) {
  let x = await axios.get(url)
  return new Date(x.data * 1000)
}

// loads csv, converts dates to Date objects, string values to numberical
async function loadCsv(url) {
  const data = await d3.csv(url)
  data.forEach((row) => {
    data.columns.forEach((col) => {
      if (col != 'date') {
        row[col] = row[col] === '' ? null : +row[col]
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
      percentDiff: undefined,
    },
    dayBefore: {
      date: new Date(),
      firstDate: undefined,
      value: undefined,
      diff: undefined,
      percentDiff: undefined,
    },
    day2Before: {
      date: new Date(),
      value: undefined,
    },
  }

  let i = data.length - 1

  // console.log("lastDay found in row", i, data[i])

  while (i >= 0 && data[i][field] == null) i--
  result.lastDay.date = new Date(data[i]['date'])
  result.lastDay.value = data[i][field]

  if (cumulative) {
    while (i >= 0 && result.lastDay.value === data[i][field]) i--
    let date = new Date(data[i + 1]['date'])
    if (
      data[i + 1][field] != null &&
      result.lastDay.date.getTime() != date.getTime()
    ) {
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
    result.lastDay.percentDiff =
      Math.round((result.lastDay.diff / result.dayBefore.value) * 1000) / 10
  }

  if (!result.day2Before.value) {
    result.day2Before = undefined
  } else {
    result.dayBefore.diff = result.dayBefore.value - result.day2Before.value
    result.dayBefore.percentDiff =
      Math.round((result.dayBefore.diff / result.day2Before.value) * 1000) / 10
  }

  return result
}

const statsStore = {
  namespaced: true,
  state: {
    exportTime: null,
    loaded: false,
    data: [],
    regions: [],
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
      let searchResult = getters.data.find((day) => {
        return (
          new Date(Date.parse(day.date)).setHours(0, 0, 0, 0) === date.getTime()
        )
      })
      return {
        date,
        value: searchResult ? searchResult[field] : null
      }
    },
    getLastValue: (state, getters) => (field) => {
      let result = getters.data
        .slice()
        .reverse()
        .find((day) => {
          return day[field]
        })
      return {
        date: result ?
          new Date(Date.parse(result.date)) :
          new Date(new Date().setHours(0, 0, 0, 0)),
        value: result ? result[field] : null,
      }
    },
    lastChange: (state, getters) => (field, cumulative) => {
      return lastChange(getters.data, field, cumulative)
    },
  },
  actions: {
    fetchData: async ({
      commit
    }) => {
      const ts = new Date().getTime()

      const d = await exportTime(
        `https://raw.githubusercontent.com/sledilnik/data/master/csv/stats.csv.timestamp?nocache=${ts}`
      )

      const data = await ApiService.get(
        'https://covid19.rthand.com/api/stats'
      ).then((result) => {
        return result.data
      })
      commit('setData', data)
      commit('setRegions', regions)
      commit('setExportTime', d)
    },
    refreshDataEvery: ({
      dispatch
    }, seconds) => {
      setInterval(() => {
        dispatch('fetchData')
      }, seconds * 1000)
    },
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
    },
  },
}


const store = new Vuex.Store({
  modules: {
    stats: statsStore,
  },
})

export default store