module Data.Patients

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://covid19.rthand.com/api/patients"

type PatientNums = {
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
    outOfHospital: PatientNums
    inHospital: PatientNums
    needsO2: PatientNums
    icu: PatientNums
    critical: PatientNums
    deceased: PatientNums
}

type PatientsByFacilityStats = {
    inHospital: PatientNums
    needsO2: PatientNums
    icu: PatientNums
    critical: PatientNums
    deceased: PatientNums
}

type PatientsStats = {
    dayFromStart: int
    year: int
    month: int
    day: int
    total: TotalPatientStats
    facilities: Map<string,PatientsByFacilityStats>
}

let fetch url = async {
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


