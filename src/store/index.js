import Vue from 'vue'
import Vuex from 'vuex'
import * as d3 from 'd3'
import { statsStore } from './stats.store'
import { hospitalsStore } from './hospitals.store'
import { patientsStore } from './patients.store'
import { municipalitiesStore } from './municipalities.store'
import { healthCentersStore } from './health-centers.store'
import { tableData } from './tables.store'

Vue.use(Vuex)

export function exportTime(x) {
  return new Date(x * 1000)
}

// loads csv, converts dates to Date objects, string values to numberical
export async function loadCsv(url) {
  const data = await d3.csv(url)
  data.forEach((row) => {
    data.columns.forEach((col) => {
      if (col != 'date') {
        row[col] = row[col] === '' ? null : +row[col]
      } else {
        row['date'] = new Date(row['date'])
      }
    })
  })
  return data
}

const store = new Vuex.Store({
  modules: {
    stats: statsStore,
    hospitals: hospitalsStore,
    patients: patientsStore,
    municipalities: municipalitiesStore,
    healthCenters: healthCentersStore,
    tableData,
  },
})

export default store
