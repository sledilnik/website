module Data.HCenters

open System
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api2.sledilnik.org/api/health-centers"

let sumOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (x + y)

type HcCounts = {
    medicalEmergency: int option    // examinations
    suspectedCovid: int option      // examinations, phoneTriage
    performed: int option           // tests
    positive: int option            // tests
    hospital: int option            // sentTo
    selfIsolation: int option       // sentTo
} with
    static member ( + ) (x: HcCounts, y: HcCounts) = { 
        medicalEmergency = sumOption x.medicalEmergency y.medicalEmergency 
        suspectedCovid = sumOption x.suspectedCovid y.suspectedCovid 
        performed = sumOption x.performed y.performed 
        positive = sumOption x.positive y.positive 
        hospital = sumOption x.hospital y.hospital 
        selfIsolation = sumOption x.selfIsolation y.selfIsolation 
    }
    static member None = { 
        medicalEmergency = None 
        suspectedCovid = None
        performed = None
        positive = None
        hospital = None
        selfIsolation = None
    }

type TotalHcStats = {
    examinations: HcCounts
    phoneTriage: HcCounts
    tests: HcCounts
    sentTo: HcCounts
} with
    static member ( + ) (x: TotalHcStats, y: TotalHcStats) = { 
        examinations =  x.examinations + y.examinations
        phoneTriage =  x.phoneTriage + y.phoneTriage
        tests =  x.tests + y.tests
        sentTo =  x.sentTo + y.sentTo
    }
    static member None = { 
        examinations = HcCounts.None 
        phoneTriage = HcCounts.None
        tests = HcCounts.None
        sentTo = HcCounts.None
    }

type HcStats = {
    year: int
    month: int
    day: int
    all: TotalHcStats
    municipalities : Map<string, Map<string,TotalHcStats>>
} with
    member ps.Date = new DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = new DateTime(ps.year, ps.month, ps.day) |> Highcharts.Helpers.jsTime12h

let getOrFetch = DataLoader.makeDataLoader<HcStats []> url
