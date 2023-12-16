import ApiService from '../services/api.service'
import ContentApiService from '../services/content-api.service'

export default {
    install(Vue, options) {
        Vue.prototype.dataApi = new ApiService({})
        Vue.prototype.contentApi = new ContentApiService({})
    },
}
