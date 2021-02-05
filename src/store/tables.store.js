import i18next from 'i18next'
import { formatNumber } from '../i18n';
import { exportTime, loadCsv } from './index'

function processTableData(data) {
  const x = Object.keys(_.last(data))
    .map((dimension) => {
      let newData = {}
      newData['dim'] = dimension
      newData[' '] = i18next.t('tableDict')[dimension.replace(/\./g, '_')]

      data
        .slice()
        .reverse()
        .forEach((day, i) => {
          let date = i18next.t('tables.day', {
            date: new Date(day.date),
            interpolation: { escapeValue: false },
          })
          newData[date] = formatNumber(day[dimension])
        })
      return newData
    })
    .filter((val) => val)
  return x
}

const state = {
  exportTime: null,
  data: [],
}

const getters = {
  data: (state) => {
    return state.data
  },

  tableData(state, getters) {
    return getters.data.length
      ? processTableData(getters.data)
      : []
  },

  filterTableData: (state, getters) => (dimension) => {
    const items = getters.tableData.filter((day) => {
      return dimension.includes(day.dim)
    })
    const sample = items[0]
    const fields = Object.keys(sample)
      .map((dimension, i) => {
        return {
          key: dimension,
          label: dimension,
          stickyColumn: dimension === ' ',
          sortable: true,
        }
      })
      .filter((item) => item.key !== 'dim')
    return {
      items,
      fields,
    }
  },
}

const actions = {
  fetchData: async ({ commit }) => {
    const ts = new Date().getTime()

    const d = await exportTime(
      `https://raw.githubusercontent.com/sledilnik/data/master/csv/stats.csv.timestamp?nocache=${ts}`
    )

    const data = await loadCsv(
      `https://raw.githubusercontent.com/sledilnik/data/master/csv/stats.csv?nocache=${ts}`
    )

    commit('setData', data)
    commit('setExportTime', d)
  },
  refreshDataEvery: ({ dispatch }, seconds) => {
    setInterval(() => {
      dispatch('fetchData')
    }, seconds * 1000)
  },
}

const mutations = {
  setData: (state, data) => {
    state.data = data
    state.loaded = true
  },

  setExportTime: (state, exportTime) => {
    state.exportTime = exportTime
  },
}

export const tableData = {
  namespaced: true,
  state,
  getters,
  actions,
  mutations,
}
