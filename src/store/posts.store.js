import ContentApiService from "../services/content-api.service";
import _ from "lodash";

const contentApi = new ContentApiService();
const state = {
  // API will return posts, ordered by created date and pinned status (first pinned, everything ordered by date DESC)
  posts: [],
};

const getters = {
  lastestTwoPosts(state) {
    return _.take(state.posts, 2);
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
