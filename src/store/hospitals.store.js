import { exportTime } from './index'
import hospitalsJSON from '../services/dict.hospitals.json'
import ApiService from '../services/api.service'

const state = {
  loaded: false,
  exportTime: null,
  data: [],
  hospitals: {},
}

const getters = {
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
    return state.data.map((row) => {
      return [Date.parse(row['date']), row[field]]
    })
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
    return { date, value: searchResult ? searchResult[field] : null }
  },
  getLastValue: (state, getters) => (field) => {
    let result = getters.data
      .slice()
      .reverse()
      .find((day) => {
        return day[field]
      })
    return {
      date: result
        ? new Date(Date.parse(result.date))
        : new Date(new Date().setHours(0, 0, 0, 0)),
      value: result ? result[field] : null,
    }
  },
}

const actions = {
  fetchData: async ({ commit }) => {
    const ts = new Date().getTime()

    const d = await exportTime(
      `https://raw.githubusercontent.com/sledilnik/data/master/csv/hospitals.csv.timestamp?nocache=${ts}`
    )

    const data = await ApiService.get('https://api.sledilnik.org/api/hospitals').then((result) => {
      return result.data
    })

    let hospitals = {}
    hospitalsJSON.forEach((row) => {
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
  },
}

const mutations = {
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

export const hospitalsStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
