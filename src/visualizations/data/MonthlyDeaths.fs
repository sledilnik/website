module Data.MonthlyDeaths

type DataPoint = {
    year : int
    month : int
    deceased : int
}

let apiUrl = "https://api.sledilnik.org/api/monthly-deaths-slovenia"

let loadData () =
    DataLoader.makeDataLoader<DataPoint list> apiUrl ()
