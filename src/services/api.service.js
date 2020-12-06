import axios from 'axios'
import axiosETAGCache from 'axios-etag-cache'

export const API_ENDPOINT_BASE = window.location.search.indexOf('stage') > 0 ? 'https://api-stage.sledilnik.org' : 'https://api.sledilnik.org'

const ApiService = {
  init(baseURL) {
    axios.defaults.baseURL = baseURL
  },

  get(resource, opts) {
    return axiosETAGCache(axios).get(resource, opts)
  },

  fetchPosts() {
    
  }
}

export default ApiService