// import { axiosETAGCache } from "axios-etag-cache";
// import { setupCache } from "axios-cache-adapter";
import { Axios } from 'axios'

const FIFTEEN_MINUTES = 15 * 60 * 1000
// const defaultCache = setupCache({
//   maxAge: FIFTEEN_MINUTES,
//   exclude: {
//     query: false
//   }
// });

// TODO: inject this
// export const API_ENDPOINT_BASE = process.env.VUE_APP_API_ENDPOINT_BASE;
export const API_ENDPOINT_BASE = import.meta.env.VITE_APP_API_ENDPOINT_BASE

export default class ApiService {
    constructor({ baseURL = API_ENDPOINT_BASE }) {
        // this.axios = axiosETAGCache({ baseURL, adapter: cache.adapter });
        this.axios = new Axios({
            baseURL: API_ENDPOINT_BASE,
        })
    }

    get(resource, opts) {
        return this.axios.get(resource, opts)
    }
}
