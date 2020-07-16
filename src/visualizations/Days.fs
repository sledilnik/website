[<RequireQualifiedAccess>]
module Days

open System

let daysInYear year =
    match DateTime.IsLeapYear year with
    | true -> 366
    | false -> 365

// NOTE: Fable's (or JS?) implementation of DateTime incorrectly calculates
// the count of days between two dates when there is a Daylight Saving Time
// switch in-between, so use this function instead.
let between (date1: DateTime) (date2: DateTime) =
    let yearsDays =
        seq { date1.Year .. (date2.Year - 1) }
        |> Seq.map daysInYear
        |> Seq.sum

    date2.DayOfYear - date1.DayOfYear + yearsDays

let add (daysToAdd: int) (date: DateTime) =
    (date.Date.AddDays (daysToAdd |> float)).Date
