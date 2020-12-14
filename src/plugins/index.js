import ApiService from "../services/api.service";
import ContentApiService from "../services/content-api.service";

const SledilnikPlugins = {
  install(Vue, options) {
    Vue.prototype.dataApi = new ApiService({});
    Vue.prototype.contentApi = new ContentApiService();
  },
};

export default SledilnikPlugins;
