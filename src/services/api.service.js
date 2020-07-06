import axios from 'axios'
import axiosETAGCache from 'axios-etag-cache'


const ApiService = {
  init(baseURL) {
    axios.defaults.baseURL = baseURL
  },

  get(resource) {
    return axiosETAGCache(axios).get(resource)
  },
}

export default ApiService