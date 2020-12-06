import ApiService from "./api.service";

export const CONTENT_ENDPOINT_BASE = process.env.VUE_APP_CONTENT_ENDPOINT_BASE;

class ContentApiService extends ApiService {
  constructor() {
    super({ baseURL: CONTENT_ENDPOINT_BASE });
  }

  async get(resource, opts) {
    const { data } = await this.axios.get(resource, opts);
    return data;
  }
}

export default ContentApiService;
