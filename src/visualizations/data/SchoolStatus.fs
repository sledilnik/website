module Data.SchoolStatus

open System

//let url = "https://api.sledilnik.org/api/school-status"
let url = "https://api.sledilnik.org/api/school-status?id=14709&id=3545&id=229"

type SchoolAbsence = {
    year: int
    month: int
    day: int
    absentFrom : 
        {|
            year: int
            month: int
            day: int
        |}
    absentTo : 
        {|
            year: int
            month: int
            day: int
        |}
    school : string
    schoolType : string
    personType : string
    personClass : string
    reason : string
}
with
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.DateAbsentFrom = DateTime(lt.absentFrom.year, lt.absentFrom.month, lt.absentFrom.day)
    member lt.DateAbsentTo = DateTime(lt.absentTo.year, lt.absentTo.month, lt.absentTo.day)
    member lt.JsDate12hAbsentFrom = DateTime(lt.absentFrom.year, lt.absentFrom.month, lt.absentFrom.day) |> Highcharts.Helpers.jsTime12h
    member lt.JsDate12hAbsentTo = DateTime(lt.absentTo.year, lt.absentTo.month, lt.absentTo.day) |> Highcharts.Helpers.jsTime12h

type SchoolRegime = {
    year: int
    month: int
    day: int
    changedFrom : 
        {|
            year: int
            month: int
            day: int
        |}
    changedTo : 
        {|
            year: int
            month: int
            day: int
        |}
    school : string
    schoolType : string
    personClass : string
    attendees : int
    regime : string
    reason : string
}
with 
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.DateChangedFrom = DateTime(lt.changedFrom.year, lt.changedFrom.month, lt.changedFrom.day)
    member lt.DateChangedTo = DateTime(lt.changedTo.year, lt.changedTo.month, lt.changedTo.day)
    member lt.JsDate12hChangedFrom = DateTime(lt.changedFrom.year, lt.changedFrom.month, lt.changedFrom.day) |> Highcharts.Helpers.jsTime12h
    member lt.JsDate12hChangedTo = DateTime(lt.changedTo.year, lt.changedTo.month, lt.changedTo.day) |> Highcharts.Helpers.jsTime12h


type SchoolStatus = {
    absences: SchoolAbsence array
    regimes: SchoolRegime array
  }

type SchoolStatusMap = Map<string,SchoolStatus>

let getOrFetch = 
    DataLoader.makeDataLoader<SchoolStatusMap> url
