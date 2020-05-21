import i18next from 'i18next'

function processTableData(data) {
  const x = Object.keys(_.last(data))
    .map((dimension) => {
      let newData = {}
      newData['dim'] = dimension
      newData[' '] = i18next.t('tableDict')[dimension]

      data
        .slice()
        .reverse()
        .forEach((day, i) => {
          let date = i18next.t('tables.day', {
            date: new Date(day.date),
            interpolation: { escapeValue: false },
          })
          newData[date] = day[dimension]
        })
      return newData
    })
    .filter((val) => val)
  return x
}

const getters = {
  tableData(state, getters, rootState) {
    return rootState.stats.data.length
      ? processTableData(rootState.stats.data)
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

export const tableData = {
  namespaced: true,
  getters,
}
