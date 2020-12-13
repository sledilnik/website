import store from "./index";

store.watch(
  (state) => state.stats.data,
  (newVal) => {
    console.log("NEW STATS DATA", newVal);
  }
);
store.watch(
  (state) => state.patients.data,
  (newVal) => {
    console.log("NEW PATIENTS DATA", newVal);
  }
);
