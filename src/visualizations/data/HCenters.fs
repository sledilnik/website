module Data.HCenters

open System

let url = "https://api.sledilnik.org/api/health-centers"

let sumOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (x + y)

type Examinations = {
    medicalEmergency: int option
    suspectedCovid: int option
} with
    static member (+) (a, b) = {
        medicalEmergency = sumOption a.medicalEmergency b.medicalEmergency
        suspectedCovid = sumOption a.suspectedCovid b.suspectedCovid
    }
    static member None = {
        medicalEmergency = None
        suspectedCovid = None
    }

type PhoneTriage = {
    suspectedCovid: int option
} with
    static member (+) (a, b) = {
        suspectedCovid = sumOption a.suspectedCovid b.suspectedCovid
    }
    static member None = {
        suspectedCovid = None
    }

type Tests = {
    performed: int option
    positive: int option
} with
    static member (+) (a, b) = {
        performed = sumOption a.performed b.performed
        positive = sumOption a.positive b.positive
    }
    static member None = {
        performed = None
        positive = None
    }

type SentTo = {
    hospital: int option
    selfIsolation: int option
} with
    static member (+) (a, b) = {
        hospital = sumOption a.hospital b.hospital
        selfIsolation = sumOption a.selfIsolation b.selfIsolation
    }
    static member None = {
        hospital = None
        selfIsolation = None
    }

type TotalHcStats = {
    examinations: Examinations
    phoneTriage: PhoneTriage
    tests: Tests
    sentTo: SentTo
} with
    static member (+) (a, b) = {
        examinations =  a.examinations + b.examinations
        phoneTriage =  a.phoneTriage + b.phoneTriage
        tests =  a.tests + b.tests
        sentTo =  a.sentTo + b.sentTo
    }
    static member None = {
        examinations = Examinations.None
        phoneTriage = PhoneTriage.None
        tests = Tests.None
        sentTo = SentTo.None
    }

type HcStats = {
    year: int
    month: int
    day: int
    all: TotalHcStats
    municipalities : Map<string, Map<string,TotalHcStats>>
} with
    member ps.Date = DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = DateTime(ps.year, ps.month, ps.day)
                          |> Highcharts.Helpers.jsTime12h

let getOrFetch = DataLoader.makeDataLoader<HcStats []> url
