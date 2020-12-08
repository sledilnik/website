import axios from 'axios'
import axiosETAGCache from 'axios-etag-cache'


const ApiService = {
  init(baseURL) {
    axios.defaults.baseURL = baseURL
  },

  get(resource, opts) {
    return axiosETAGCache(axios).get(resource, opts)
  },
}

export default ApiService