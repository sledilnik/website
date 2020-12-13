import axiosETAGCache from "axios-etag-cache";
import { setupCache } from "axios-cache-adapter";

const FIFTEEN_MINUTES = 15 * 60 * 1000;
const defaultCache = setupCache({
  maxAge: FIFTEEN_MINUTES,
  exclude: {
    query: false
  }
});

export const API_ENDPOINT_BASE = process.env.VUE_APP_API_ENDPOINT_BASE;

class ApiService {
  constructor({ baseURL = API_ENDPOINT_BASE, cache = defaultCache }) {
    this.axios = axiosETAGCache({ baseURL, adapter: cache.adapter });
  }

  get(resource, opts) {
    return this.axios.get(resource, opts);
  }
}

export default ApiService;
