module Data.Vaccinations

open System

let url = "https://api.sledilnik.org/api/vaccinations"

type VaccinationCounts = {
    today: int option
    toDate: int option
}

type VaccinationStats = {
    year: int
    month: int
    day: int
    administered: VaccinationCounts
    administered2nd: VaccinationCounts
    usedToDate: int option
    deliveredToDate: int option
    deliveredByManufacturer: Map<string,int>
  } with
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.JsDate12h = DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h


let getOrFetch = DataLoader.makeDataLoader<VaccinationStats array> url
