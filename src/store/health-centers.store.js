import ApiService, { API_ENDPOINT_BASE } from '../services/api.service'
import {
  exportTime,
} from './index'

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
  fetchData: async ({
    commit
  }) => {
    const data = await ApiService.get(`${API_ENDPOINT_BASE}/api/health-centers`)
    const d = exportTime(data.headers.timestamp)

    commit('setData', data.data)
    commit('setExportTime', d)
  },

  refreshDataEvery: ({
    dispatch
  }, seconds) => {
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