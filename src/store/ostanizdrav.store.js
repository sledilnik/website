import _ from 'lodash'
import ApiService from '../services/api.service'

const state = {
  exportTime: null,
  loaded: false,
}

const getters = {
}

const actions = {
  fetchData: async ({ commit }, to) => {
    const resp = await ApiService.get('https://ostanizdrav.sledilnik.org/plots/timestamp.json')
    commit('setExportTime', new Date(resp.data.unix * 1000))
  },
  refreshDataEvery: ({ dispatch }, seconds) => {
    setInterval(() => {
      dispatch('fetchData')
    }, seconds * 1000)
  },
}

const mutations = {
  setExportTime: (state, exportTime) => {
    state.exportTime = exportTime
  },
}

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
