import axios from 'axios'

const ApiService = {
  init(baseURL) {
    axios.defaults.baseURL = baseURL
  },

  get(resource) {
    return axios.get(resource)
  },
}

export default ApiService
