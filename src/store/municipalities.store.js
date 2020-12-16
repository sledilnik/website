import ApiService from '../services/api.service'
import {
  exportTime,
} from './index'

const dataApi = new ApiService({})

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
    const data = await dataApi.get('/api/municipalities')
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

export const municipalitiesStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}