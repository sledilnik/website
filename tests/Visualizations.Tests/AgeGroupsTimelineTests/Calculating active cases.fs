module AgeGroupsTimelineTests.``Calculating active cases``

open AgeGroupsTimelineViz.Analysis
open AgeGroupsTimelineViz.Synthesis
open System
open TestHelpers
open Xunit
open Swensen.Unquote

//type CasesInAgeGroupForDay = int option
//type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>
//type DatedArray<'T> = {
//    StartDate: DateTime
//    Data: int option[]
//}

let ACTIVE_DAYS_COUNT = 14

let calculateActiveCasesForGroupTimeline
    (newCasesTimeline: CasesInAgeGroupTimeline)
    : CasesInAgeGroupTimeline =
//    let newCasesArray = newCasesTimeline.Data
//
//    let x =
//        newCasesArray
//        |> Array.windowed ACTIVE_DAYS_COUNT
//        |> Array.map

    invalidOp "todo"

[<Fact>]
let ``Can calculate active cases``() =
    let totalDay0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
    let totalDay1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let baseDate = DateTime(2020, 03, 01)

    let sourceData = {
            StartDate = baseDate
            Data = [| totalDay0; totalDay1 |]
        }

    let newCasesTimelineGrouped = calculateCasesByAgeTimeline sourceData
    let timeline = extractTimelineForAgeGroup
                       (groupKey 1) newCasesTimelineGrouped

    let activeCasesTimeline = calculateActiveCasesForGroupTimeline timeline

    let expectedActiveCasesTimeline =
        { StartDate = baseDate; Data = [| 15; 40 |] }

    test <@ activeCasesTimeline = expectedActiveCasesTimeline  @>
