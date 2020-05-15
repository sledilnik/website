module Data.Patients

open System
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api.sledilnik.org/api/patients"
//let url = "https://covid19.rthand.com/api/patients"

type DeceasedCounts = {
    today: int option
    toDate: int option
}

type HDeceasedCounts = {
    today: int option
    toDate: int option
    icu : DeceasedCounts
}

type TDeceasedCounts = {
    today: int option
    toDate: int option
    hospital : HDeceasedCounts
    home : DeceasedCounts
}

type PatientCounts = {
    ``in``: int option
    out: int option
    today: int option
    toDate: int option
}

type TotalPatientStats = {
    outOfHospital: PatientCounts
    inHospital: PatientCounts
    icu: PatientCounts
    critical: PatientCounts
    deceased: TDeceasedCounts
}

type PatientsByFacilityStats = {
    inHospital: PatientCounts
    icu: PatientCounts
    critical: PatientCounts
    deceased: HDeceasedCounts
}

type PatientsStats = {
    dayFromStart: int
    year: int
    month: int
    day: int
    total: TotalPatientStats
    facilities: Map<string,PatientsByFacilityStats>
  } with
    member ps.Date = new DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = new DateTime(ps.year, ps.month, ps.day) |> Highcharts.Helpers.jsTime12h

let getOrFetch = DataLoader.makeDataLoader<PatientsStats []> url
