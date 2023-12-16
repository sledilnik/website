import ApiService from "./api.service";
// import { setupCache } from "axios-cache-adapter";

// TODO inject
// export const CONTENT_ENDPOINT_BASE = process.env.VUE_APP_CONTENT_ENDPOINT_BASE;
export const CONTENT_ENDPOINT_BASE  = import.meta.env.VITE_APP_CONTENT_ENDPOINT_BASE;

class ContentApiService extends ApiService {

    constructor({baseURL} = { baseURL: CONTENT_ENDPOINT_BASE } ) {
        super({baseURL})
    }

  async get(resource, opts) {
    try {
      const { data } = await this.axios.get(resource, opts);
      return data;
    } catch (error) {
      console.log("Content API error", error);
      return {};
    }
  }
}

export default ContentApiService;
