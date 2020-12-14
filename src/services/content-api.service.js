import ApiService from "./api.service";
import { setupCache } from "axios-cache-adapter";

export const CONTENT_ENDPOINT_BASE = process.env.VUE_APP_CONTENT_ENDPOINT_BASE;

class ContentApiService extends ApiService {
  constructor({ cache } = {}) {
    const cacheObj = cache ? setupCache(cache) : undefined;
    super({ baseURL: CONTENT_ENDPOINT_BASE, cache: cacheObj });
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
