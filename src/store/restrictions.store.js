import ContentApiService from "../services/content-api.service";
import { unionBy, map } from "lodash";

const contentApi = new ContentApiService();
const state = {
  lastUpdate: null,
  restrictions: [],
};

export const FRESH_RESTRICTIONS = 'FRESH_RESTRICTIONS'
export const LAST_UPDATE = "LAST_UPDATE"

function mapToStateItem(obj) {
  obj['valid_since'] = obj['valid_since'] ? new Date(obj['valid_since']) : null
  obj['valid_until'] = obj['valid_until'] ? new Date(obj['valid_until']) : null
  return obj
}

const getters = {
};

const actions = {
  async fetchOne({ commit }, id) {
    const obj = await contentApi.get(`/restrictions/${id}/`);
    commit(FRESH_RESTRICTIONS, [obj]);
  },
  async fetchAll({ dispatch }) {
    dispatch("fetch", { limit: 30 }, { timeout: 900 }); // When we hit 30, make pagination
  },
  async fetch({ commit }, { limit }) {
    const { meta, objects } = await contentApi.get(`/restrictions/?limit=${limit}`);
    commit(LAST_UPDATE, meta.last_update);
    commit(FRESH_RESTRICTIONS, objects);
  },
};

const mutations = {
  [FRESH_RESTRICTIONS] (state, restrictions) {

    state.restrictions = unionBy(map(restrictions, mapToStateItem), state.restrictions, "id");
  },
  [LAST_UPDATE] (state, ts) {
    state.lastUpdate = new Date(ts);
  },
};

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
};
