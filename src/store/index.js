import Vue from 'vue'
import Vuex from 'vuex'
import axios from 'axios'
import { statsStore } from './stats.store'
import { hospitalsStore } from './hospitals.store'
import { patientsStore } from './patients.store'
import { tableData } from './tables.store'

Vue.use(Vuex)

export async function exportTime(url) {
  let x = await axios.get(url)
  return new Date(x.data * 1000)
}

const store = new Vuex.Store({
  modules: {
    stats: statsStore,
    hospitals: hospitalsStore,
    patients: patientsStore,
    tableData,
  },
})

export default store
