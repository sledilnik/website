import ContentApiService from "../services/content-api.service";
import _ from "lodash";

const contentApi = new ContentApiService();
const state = {
  posts: [],
};

const getters = {
  postsByDateDescending(state) {
    return state.posts.sort((el1, el2) => {
      return new Date(el2.created) - new Date(el1.created);
    });
  },
  lastestTwoPosts(state, getters) {
    return _.take(getters.postsByDateDescending, 2);
  },
  postById: (state) => (id) => {
    if (!id || !state.posts) {
      return;
    }
    return state.posts.find((post) => {
      return post && post.id.toString() === id.toString();
    });
  },
};

const actions = {
  async fetchPost({ commit }, id) {
    const post = await contentApi.get(`/posts/${id}/`);
    commit("FRESH_POSTS", [post]);
  },
  async fetchLatestPosts({ dispatch }) {
    dispatch("fetchPosts", { limit: 2 }, { timeout: 900 });
  },
  async fetchAllPosts({ dispatch }) {
    dispatch("fetchPosts", { limit: 30 }, { timeout: 900 }); // When we hit 30, make pagination
  },
  async fetchPosts({ commit }, { limit }) {
    const { objects } = await contentApi.get(`/posts/?limit=${limit}`);
    commit("FRESH_POSTS", objects);
  },
};

const mutations = {
  FRESH_POSTS(state, posts) {
    state.posts = _.unionBy(posts, state.posts, "id");
  },
};

export const postsStore = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
};
