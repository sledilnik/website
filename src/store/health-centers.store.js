import ApiService from '../services/api.service'
import { exportTime } from './index'

const state = {
  exportTime: null,
  loaded: false,
  data: [],
}

const getters = {
  data: (state) => {
    return state.data
  },
}

const actions = {
  fetchData: async ({ commit }) => {
    const ts = new Date().getTime()

    const d = await exportTime(
      `https://raw.githubusercontent.com/sledilnik/data/master/csv/health_centers.csv.timestamp?nocache=${ts}`
    )
    const data = await ApiService.get(
      'https://api.sledilnik.org/api/health-centers'
    ).then((response) => {
      return response.data
    })

    commit('setData', data)
    commit('setExportTime', d)
  },

  refreshDataEvery: ({ dispatch }, seconds) => {
    setInterval(() => {
      dispatch('fetchData')
    }, seconds * 1000)
  },
}

const mutations = {
  setData: (state, data) => {
    state.data = data
    state.loaded = true
  },

  setExportTime: (state, exportTime) => {
    state.exportTime = exportTime
  },
}

export const healthCentersStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
