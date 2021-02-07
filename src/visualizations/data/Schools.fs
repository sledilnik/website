module Data.Schools

open System

let url = "https://api.sledilnik.org/api/schools"

type SchoolCounts = {
    confirmed: int option
    active: int option
    to_quarantine: int option
    quarantined: int option
    remote: int option
}

type SchoolTypeStats = {
    employees: SchoolCounts
    attendees: SchoolCounts
    units: SchoolCounts
}

type SchoolsStats = {
    year: int
    month: int
    day: int
    schoolType: Map<string,SchoolTypeStats>
  } with
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.JsDate12h = DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h

let getOrFetch = DataLoader.makeDataLoader<SchoolsStats array> url
