import ContentApiService from "../services/content-api.service";

const contentApi = new ContentApiService({});
const state = {
  faqVaccines: {
    sl: [],
    en: []
  }
};

export const FRESH_FAQ_VACCINES = 'FRESH_FAQ_VACCINES'

const getters = {
};

const actions = {
  async fetchOne({ commit }, id) {
    const lang = localStorage.getItem ("i18nextLng") || 'sl';
    const obj = await contentApi.get(`/faq/${id}/?lang=${lang}`);
    commit(FRESH_FAQ_VACCINES, { obj, lang });
  },
  async fetchAll({ dispatch }) {
    dispatch("fetch", { limit: 30 }, { timeout: 900 }); // When we hit 30, make pagination
  },
  async fetch({ commit }, { limit }) {
    const objects = await contentApi.get(`/faq/?limit=${limit}`);
    commit(FRESH_FAQ_VACCINES, objects);
  },
};

const mutations = {
  [FRESH_FAQ_VACCINES] (state, payload) {
    state.faqVaccines[payload.lang] = [payload.obj];
  },
};

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
};
