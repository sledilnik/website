import axios from 'axios'
import axiosETAGCache from 'axios-etag-cache'


const ApiService = {
  init(baseURL) {
    axios.defaults.baseURL = baseURL
  },

  get(resource, from, to) {
    return axiosETAGCache(axios).get(resource, { params: { from, to } })
  },
}

export default ApiService