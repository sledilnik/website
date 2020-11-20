import axios from 'axios'

const state = {
  exportTime: null,
  loaded: false,
  avatar: null,
  feed: null
}

const getters = {
  data: (state) => {
    return state.data
  },

  avatar: (state) => {
    return state.avatar
  },

  feed: (state) => {
    return state.feed
  }
}

const actions = {
  fetch: async ({ commit }) => {
    const response = await axios.get(
      'https://api.rss2json.com/v1/api.json?rss_url=https://medium.com/feed/@sledilnik'
    )
    const avatar = response.data.feed.image
    const profileLink = response.data.feed.link
    const posts = response.data.items
    const filteredPosts = posts.filter(post => {
      if (!post.link.includes('@')) {
        return post
      }
    })

    commit('setAvatar', { avatar, profileLink })
    commit('setFeed', filteredPosts)
    commit('setExportTime', new Date())
  },
  refreshDataEvery: ({ dispatch }, seconds) => {
    setInterval(() => {
      dispatch('fetch')
    }, seconds * 1000)
  },
}

const mutations = {
  setData: (state, data) => {
    state.data = data
    state.loaded = true
  },

  setAvatar: (state, avatar) => {
    state.avatar = avatar
  },

  setFeed: (state, feed) => {
    state.feed = feed
  },

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
