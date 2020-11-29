module Data.LabTests

open System

let url = "https://api.sledilnik.org/api/lab-tests"

type TestCounts = {
    today: int option
    toDate: int option
}

type TestsStats = {
    performed: TestCounts
    positive: TestCounts
}

type LabTestsStats = {
    year: int
    month: int
    day: int
    total: TestsStats
    data: Map<string,TestsStats>
    labs: Map<string,TestsStats>
  } with
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.JsDate12h = DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h


let getOrFetch = DataLoader.makeDataLoader<LabTestsStats array> url
