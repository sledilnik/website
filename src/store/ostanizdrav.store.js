import _ from 'lodash'
import { exportTime, ApiEndpoint } from './index'
import ApiService from '../services/api.service'
import regions from '../services/dict.regions.json'
import axios from 'axios'

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
