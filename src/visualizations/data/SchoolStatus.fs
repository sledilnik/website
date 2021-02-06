module Data.SchoolStatus

open System

let url = "https://api-stage.sledilnik.org/api/school-status"

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
    school : int
    schoolType : string
    personType : string
    personClass : string
    reason : string
}
with
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.AbsentFromDate = DateTime(lt.absentFrom.year, lt.absentFrom.month, lt.absentFrom.day)
    member lt.AbsentToDate = DateTime(lt.absentTo.year, lt.absentTo.month, lt.absentTo.day)

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
    school : int
    schoolType : string
    personClass : string
    attendees : int
    regime : string
    reason : string
}
with 
    member lt.Date = DateTime(lt.year, lt.month, lt.day)
    member lt.ChangedFromDate = DateTime(lt.changedFrom.year, lt.changedFrom.month, lt.changedFrom.day)
    member lt.ChangedToDate = DateTime(lt.changedTo.year, lt.changedTo.month, lt.changedTo.day)


type SchoolStatus = {
    absences: SchoolAbsence array
    regimes: SchoolRegime array
  }

type SchoolStatusMap = Map<string,SchoolStatus>

let getOrFetch = DataLoader.makeDataLoader<SchoolStatusMap> url
