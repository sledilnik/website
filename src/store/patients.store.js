import ApiService from '../services/api.service'
import { exportTime, ApiEndpoint } from './index'

function getSum(total, num) {
  return total + num.total.deceased.today
}

const state = {
  exportTime: null,
  loaded: false,
  data: [],
}

const getters = {
  data: (state) => {
    return state.data
  },

  runningSum: (state, getters) => (days) => {
    let array = getters.data.slice(Math.max(getters.data.length - days, 0))
    let sum = array.reduce(getSum, 0)
    return sum
  },
}

const actions = {
  fetchData: async ({ commit }, to) => {
    const tempDate = typeof to === 'undefined' ? new Date() : new Date(to)
    const from = new Date(tempDate.setDate(tempDate.getDate() - 11))
    const data = await ApiService.get(`${ApiEndpoint()}/api/patients`, from, to)
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
