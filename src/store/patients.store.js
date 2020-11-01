import _ from 'lodash'
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

  lastData: (state, getters) => (start, end, field) => {
    let array = getters.data.slice(
      getters.data.length - end,
      getters.data.length - start
    )
    return array.map((obj) => {
      return _.get(obj, field)
    })
  },

  runningSum: (state, getters) => (start, end, field) => {
    let array = getters.data.slice(
      getters.data.length - end,
      getters.data.length - start
    )
    let sum = array.reduce((total, num) => total + _.get(num, field), 0)
    return sum
  },
}

const actions = {
  fetchData: async ({ commit }, to) => {
    const tempDate = typeof to === 'undefined' ? new Date() : new Date(to)
    const from = new Date(tempDate.setDate(tempDate.getDate() - 17))
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
