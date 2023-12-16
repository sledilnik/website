import axios from 'axios'

export const API_ENDPOINT_BASE = import.meta.env.VITE_APP_API_ENDPOINT_BASE

export default class ApiService {
    constructor({ baseURL = API_ENDPOINT_BASE } = {}) {
        this.axios = axios.create({
            baseURL: API_ENDPOINT_BASE,
        })
    }

    get(resource, opts) {
        return this.axios.get(resource, opts)
    }
}
