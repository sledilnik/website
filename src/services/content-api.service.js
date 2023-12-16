import ApiService from "./api.service";
// import { setupCache } from "axios-cache-adapter";

// TODO inject
// export const CONTENT_ENDPOINT_BASE = process.env.VUE_APP_CONTENT_ENDPOINT_BASE;
export const CONTENT_ENDPOINT_BASE  = "";

class ContentApiService extends ApiService {
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
