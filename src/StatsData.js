import * as d3 from "d3";
import moment from "moment";

class StatsData {

  static instance

  constructor() {
    if (StatsData.instance) {
      return StatsData.instance
    }
    this.init()
    StatsData.instance = this
  }

  init() {
    this.csvdata = new Promise( (resolve) => {
      this.loadData().then(data => {
        resolve(data)
      })
    })

  }

  async loadData() {
    return await d3.csv(
      "https://raw.githubusercontent.com/slo-covid-19/data/master/csv/stats.csv"
    );
  }

  async data() {
    // promise
    return this.csvdata
  }

  async lastWeek(field) {
    return await this.csvdata.then(csvdata => {
      if (csvdata && csvdata.length > 0) {
        for (let i = 0; i < csvdata.length; i++) {
          let row = csvdata[i]
          if (Date.parse(row['date']) == date.getTime()) {
            return {
              date: date,
              value: row[field],
            }    
          }
        }
        return []

      } else {
        return []
      }
    })
  }

  async getValueOn(field, date) {
    return await this.csvdata.then(csvdata => {
      if (csvdata && csvdata.length > 0) {
        for (let i = 0; i < csvdata.length; i++) {
          let row = csvdata[i]
          if (Date.parse(row['date']) == date.getTime()) {
            return {
              date: date,
              value: row[field],
            }    
          }
        }
        return {
          date: date,
          value: undefined,
        }

      } else {
        return {
          date: date,
          value: undefined,
        }
      }
    })
  }

  async getLastValue(field) {
    return await this.csvdata.then(csvdata => {
      if (csvdata && csvdata.length > 0) {
        let i = 0;
        // find last non null value
        for (i = csvdata.length - 1; i > 0; i--) {
          let row = csvdata[i];
          if (row[field]) {
            break;
          }
        }

        let lastRow = csvdata[i];
        let value = lastRow[field] || undefined;
        return {
          date: new Date(Date.parse(lastRow["date"])),
          value: value
        };
      } else {
        return {
          date: new Date().setHours(0,0,0,0),
          value: undefined
        };
      }
    })
  }
}

const instance = new StatsData()

export default instance
