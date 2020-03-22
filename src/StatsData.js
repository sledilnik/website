import * as d3 from "d3";

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
    this.csvdata = new Promise( (resolve, reject) => {
      this.loadData().then(data => {
        resolve(data)
        // this.csvdata = data
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

  async getLastValue(field) {
    await this.csvdata.then(csvdata => {
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
        let value = lastRow[field] || "N/A";
        return {
          date: new Date(Date.parse(lastRow["date"])),
          value: value
        };
      } else {
        return {
          date: new Date(),
          value: "N/A"
        };
      }
    })
  }
}

const instance = new StatsData()

export default instance