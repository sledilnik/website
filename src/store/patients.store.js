import ApiService from '../services/api.service'
import { exportTime } from './index'

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

  runningSumPatients: (state, getters) => (start, end, field) => {
    let array = getters.data.slice(
      getters.data.length - end - 1,
      getters.data.length - start - 1
    )
    let sum = array.reduce((total, num) => total + _.get(num, field), 0)
    let x = end - start
    return sum / x
  },
}

const actions = {
  fetchData: async ({ commit }, to) => {
    const tempDate = typeof to === 'undefined' ? new Date() : new Date(to)
    const from = new Date(tempDate.setDate(tempDate.getDate() - 10))
    const data = await dataApi.get('/api/patients', {params: {from, to}})
    const d =
      typeof to === 'undefined' ? exportTime(data.headers.timestamp) : to

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
