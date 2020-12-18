import _ from 'lodash'
import { exportTime } from './index'
import ApiService from '../services/api.service'
import regions from '../services/dict.regions.json'
import i18n from '../i18n'

const dataApi = new ApiService({})

const infoCardConfig = () => { 
  return {
    casesActive: {
      title: i18n.t('infocard.active'),
    },
    casesAvg7Days: {
      title: i18n.t('infocard.newCases7d'),
      subTitle: i18n.t('infocard.newCases7dInfo')
    },
    casesToDateSummary: {
      title: i18n.t('infocard.confirmedToDate'),
    },
    deceasedToDate: {
      title: i18n.t('infocard.deceasedToDate'),
    },
    hospitalizedCurrent: {
      title: i18n.t('infocard.inHospital'),
    },
    icuCurrent: {
      title: i18n.t('infocard.icu'),
    },
    testsToday: {
      title: i18n.t('infocard.tests'),
    },
  } 
}

const state = {
  exportTime: null,
  loaded: false,
  data: [],
  regions: [],
  summary: infoCardConfig()
}

const getters = {
  data: (state) => {
    return state.data
  },

  regions: (state) => {
    return state.regions
  },
}

const actions = {
  fetchSummary: async ({ commit }) => {
    const { data } = await dataApi.get('/api/summary', { responseType: 'json' })
    commit('setSummary', _.defaultsDeep({}, data, infoCardConfig()))
  },

  fetchData: async function({ commit }, to) {
    const tempDate = typeof to === 'undefined' ? new Date() : new Date(to)
    const from = new Date(tempDate.setDate(tempDate.getDate() - 11))
    const data = await dataApi.get('/api/stats', {params: {from, to}})
    const d =
      typeof to === 'undefined' ? exportTime(data.headers.timestamp) : to

    commit('setData', data.data)
    commit('setRegions', regions)
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

  setSummary: (state, data) => {
    state.summary = data
  },

  setRegions: (state, regions) => {
    state.regions = regions
  },

  setExportTime: (state, exportTime) => {
    state.exportTime = exportTime
  },
}

export const statsStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
