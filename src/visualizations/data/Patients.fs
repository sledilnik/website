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
    inCare: int option
    outOfHospital: PatientCounts
    inHospital: PatientCounts
    needsO2: PatientCounts
    icu: PatientCounts
    critical: PatientCounts
    deceased: PatientCounts
}

type PatientsByFacilityStats = {
    inHospital: PatientCounts
    needsO2: PatientCounts
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
    member ps.JsDate = new DateTime(ps.year, ps.month, ps.day) |> Highcharts.Helpers.jsTime

let fetch () = async {
    let! code,json = Http.get url
    return
        match code with
        | 200 ->
            json
            |> SimpleJson.parse
            |> Json.convertFromJsonAs<PatientsStats []>
            |> Ok
        | err ->
            Error (sprintf "got http %d while fetching %s" err url)
}
