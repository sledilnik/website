module AgeGroupsTimelineTests.``Sythensizing data``

open System
open AgeGroupsTimelineViz.Analysis
open AgeGroupsTimelineViz.Synthesis
open TestHelpers
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Can fetch age groups from timeline``() =
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

    let timeline = calculateCasesByAgeTimeline sourceData

    let ageGroupsKeys = listAgeGroups timeline

    test <@ ageGroupsKeys = [ groupKey 1; groupKey 2 ] @>

[<Fact>]
let ``Can extract data for individual age group``() =
    let day0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
    let day1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let baseDate = DateTime(2020, 03, 01)

    let sourceData = {
            StartDate = baseDate
            Data = [| day0; day1 |]
        }

    let timeline =
        sourceData
        |> extractTimelineForAgeGroup (groupKey 1) NewCases

    test <@ timeline.StartDate = baseDate @>
    test <@ timeline.Data = [| 15; 25 |] @>
