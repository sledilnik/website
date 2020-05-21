import tableDict from '../tableDict.json'
import { format } from 'date-fns'
import { sl } from 'date-fns/locale'

function processTableData(data) {
  const x = Object.keys(_.last(data))
    .map((dimension) => {
      let newData = {}
      newData['dim'] = dimension
      newData[' '] = tableDict[dimension]

      data
        .slice()
        .reverse()
        .forEach((day, i) => {
          let date = format(day.date, 'E d.M.', {
            locale: sl,
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
