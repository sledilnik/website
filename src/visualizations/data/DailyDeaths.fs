module Data.DailyDeaths

type DataPoint = {
    year : int
    month : int
    day : int
    deceased : int
}

let apiUrl = "https://api.sledilnik.org/api/daily-deaths-slovenia"

let loadData () =
    DataLoader.makeDataLoader<DataPoint list> apiUrl ()
