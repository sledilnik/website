import axios from 'axios'

export const CONTENT_ENDPOINT_BASE = import.meta.env
    .VITE_APP_CONTENT_ENDPOINT_BASE

class ContentApiService {
    constructor({ baseURL = CONTENT_ENDPOINT_BASE } = {}) {
        this.axios = axios.create({
            baseURL: baseURL,
        })
    }

    async get(resource, opts) {
        try {
            const { data } = await this.axios.get(resource, opts)
            return data
        } catch (error) {
            console.log('Content API error', error)
            return {}
        }
    }
}

export default ContentApiService
