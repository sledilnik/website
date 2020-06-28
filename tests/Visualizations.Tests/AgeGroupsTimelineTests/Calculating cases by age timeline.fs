module AgeGroupsTimelineTests.``Calculating cases by age timeline``

open System
open AgeGroupsTimelineViz.Analysis
open TestHelpers
open Types
open Xunit
open Swensen.Unquote

//type StatsDataPoint =
//    { DayFromStart : int
//      Date : System.DateTime
//      Phase : string
//      Tests : Tests
//      Cases : Cases
//      StatePerTreatment : Treatment
//      StatePerAgeToDate : AgeGroupsList
//      DeceasedPerAgeToDate : AgeGroupsList
//      HospitalEmployeePositiveTestsToDate : int option
//      RestHomeEmployeePositiveTestsToDate : int option
//      RestHomeOccupantPositiveTestsToDate : int option
//      UnclassifiedPositiveTestsToDate : int option
//    }

let calculateCasesByAgeTimeline
    (totalCasesByAgeGroups: (DateTime * AgeGroupsList) list)
    : CasesByAgeGroupsTimeline =

    None :: (totalCasesByAgeGroups |> List.map Some)
    |> List.pairwise
    |> List.map
           (fun (prevDayRecord, currentDayRecord) ->
                match prevDayRecord, currentDayRecord with
                | Some (_, prevDay), Some (date, currentDay) ->
                    { Date = date
                      Cases =
                          calcCasesByAgeForDay (Some prevDay) currentDay }
                | None, Some (date, currentDay) ->
                    { Date = date
                      Cases = calcCasesByAgeForDay None currentDay }
                | _ -> invalidOp "BUG: this should never happen"
            )

[<Fact>]
let ``Can calculate timeline``() =
    let totalDay0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
    let totalDay1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))
    let totalDay2 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 5) (Some 23))
        |> withGroup (group 1 (Some 13) (Some 25))

    let baseDate = DateTime(2020, 03, 01)

    let sourceData =
        [ (baseDate, totalDay0);
          (baseDate.AddDays 1., totalDay1);
          (baseDate.AddDays 2., totalDay2) ]

    let onDay0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
    let onDay1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 5))
        |> withGroup (group 1 (Some 5) (Some 5))
    let onDay2 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 4) (Some 3))
        |> withGroup (group 1 (Some 3) (Some 10))

    let expectedTimeline =
          [ { Date = baseDate; Cases = onDay0 };
              { Date = baseDate.AddDays 1.; Cases = onDay1 };
              { Date = baseDate.AddDays 2.; Cases = onDay2 } ]

    test <@ calculateCasesByAgeTimeline sourceData = expectedTimeline @>
