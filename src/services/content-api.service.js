import ApiService from "./api.service";

export const CONTENT_ENDPOINT_BASE = process.env.VUE_APP_CONTENT_ENDPOINT_BASE;

class ContentApiService extends ApiService {
  constructor() {
    super({ baseURL: CONTENT_ENDPOINT_BASE });
  }

  async get(resource, opts) {
    // Django wants trailing slashes.
    if (resource.substr(-1) != "/") {
      resource += "/";
    }
    try {
      const { data } = await this.axios.get(resource, opts);
      return data;
    } catch (error) {
      console.log('Content API error', error)
      return {}
    }
  }
}

export default ContentApiService;
