import ApiService from '../services/api.service'

const state = {
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
    const data = await ApiService.get(
      'https://ostanizdrav.sledilnik.org/plots/data.json'
    )
    commit('setData', data.data)
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
}

export const ostaniZdravData = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
