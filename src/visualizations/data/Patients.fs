module Data.Patients

open System
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://covid19.rthand.com/api/patients"

type PatientCounts = {
    ``in``: int option
    out: int option
    today: int option
    toDate: int option
    total: int option
    hospital: int option
    home: int option
}

type TotalPatientStats = {
    inCare: int option // obsolete
    outOfHospital: PatientCounts
    inHospital: PatientCounts
    //needsO2: PatientCounts
    icu: PatientCounts
    critical: PatientCounts
    deceased: PatientCounts
}

type PatientsByFacilityStats = {
    inHospital: PatientCounts
    //needsO2: PatientCounts
    icu: PatientCounts
    critical: PatientCounts
    deceased: PatientCounts
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
