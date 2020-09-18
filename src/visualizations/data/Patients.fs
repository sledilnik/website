module Data.Patients

open System

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

type DepartmentCounts = {
    ``in``: int option
    out: int option
    today: int option
    toDate: int option
}

type FacilityPatientStats = {
    inHospital: DepartmentCounts
    icu: DepartmentCounts
    critical: DepartmentCounts
    deceased: HDeceasedCounts
}

type TotalPatientStats =
    {
        outOfHospital: DepartmentCounts
        inHospital: DepartmentCounts
        icu: DepartmentCounts
        critical: DepartmentCounts
        deceased: TDeceasedCounts
    }
    member this.ToFacilityStats : FacilityPatientStats =
        { inHospital = this.inHospital
          icu = this.icu
          critical = this.critical
          deceased = this.deceased.hospital }


type PatientsStats = {
    dayFromStart: int
    year: int
    month: int
    day: int
    total: TotalPatientStats
    facilities: Map<string,FacilityPatientStats>
  } with
    member ps.Date = DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = DateTime(ps.year, ps.month, ps.day)
                          |> Highcharts.Helpers.jsTime12h


let getOrFetch = DataLoader.makeDataLoader<PatientsStats []> url
