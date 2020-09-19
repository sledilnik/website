import ApiService from '../services/api.service'
import { exportTime, ApiEndpoint } from './index'

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
  fetchData: async ({ commit }, to) => {
    let data, d
    if (typeof to !== 'undefined') {
      data = await ApiService.get(`${ApiEndpoint}/api/patients`, to, to)
      d = exportTime(data.headers.timestamp)
    } else {
      const yesterday = new Date(new Date().setDate(new Date().getDate() - 2))
      data = await ApiService.get(`${ApiEndpoint}/api/patients`, yesterday)
      d = exportTime(data.headers.timestamp)
    }
    commit('setData', data.data)
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

export const patientsStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
