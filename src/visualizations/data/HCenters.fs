module Data.HCenters

open System
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api.sledilnik.org/api/health-centers"

type HcCounts = {
    medicalEmergency: int option    // examinations
    suspectedCovid: int option      // examinations, phoneTriage
    performed: int option           // tests
    positive: int option            // tests
    hospital: int option            // sentTo
    selfIsolation: int option       // sentTo
}

type TotalHcStats = {
    examinations: HcCounts
    phoneTriage: HcCounts
    tests: HcCounts
    sentTo: HcCounts
}

type HcStats = {
    year: int
    month: int
    day: int
    all: TotalHcStats
    // TODO: parse municipalities (per region, hc)
} with
    member ps.Date = new DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = new DateTime(ps.year, ps.month, ps.day) |> Highcharts.Helpers.jsTime12h

let getOrFetch = DataLoader.makeDataLoader<HcStats []> url
