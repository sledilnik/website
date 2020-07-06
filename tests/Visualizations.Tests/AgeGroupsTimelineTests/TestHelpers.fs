module AgeGroupsTimelineTests.TestHelpers

open Types

let buildAgeGroups(): AgeGroupsList = []

let groupKey groupIndex =
    { AgeFrom = groupIndex * 10 |> Some
      AgeTo = groupIndex * 10 + 1 |> Some }

let group
    groupIndex (male: int option) (female: int option): AgeGroup =
    let sum =
        match male, female with
        | Some male, Some female -> male + female |> Some
        | Some male, None -> Some male
        | None, Some female -> Some female
        | None, None -> None

    { GroupKey = groupKey groupIndex
      Male = male; Female = female; All = sum
    }

let withGroup group groups = group :: groups

let someTestMeasure = { ToDate = None; Today = None }
let someTestGroup = { Performed = someTestMeasure; Positive = someTestMeasure }

//let buildStatsDay dayIndex casesByAge =
//    { DayFromStart = dayIndex; Date = DateTime(2020, 03, 01).AddDays dayIndex
//      Phase = ""
//      Tests =
//        { Performed = someTestMeasure; Positive = someTestMeasure
//          Regular = someTestGroup; NsApr20 = someTestGroup }
//      Cases =
//    }
