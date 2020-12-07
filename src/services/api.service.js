import axiosETAGCache from "axios-etag-cache";

export const API_ENDPOINT_BASE = process.env.VUE_APP_API_ENDPOINT_BASE;

class ApiService {
  constructor({ baseURL = API_ENDPOINT_BASE }) {
    this.axios = axiosETAGCache({ baseURL });
  }

  get(resource, opts) {
    return this.axios.get(resource, opts);
  }
}

export default ApiService;
