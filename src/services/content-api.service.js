import { Axios } from 'axios'

export const CONTENT_ENDPOINT_BASE = import.meta.env
    .VITE_APP_CONTENT_ENDPOINT_BASE

class ContentApiService {
    constructor() {
        this.axios = new Axios({
            baseURL: CONTENT_ENDPOINT_BASE,
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
