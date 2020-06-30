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

    let sourceData =
        [ (baseDate, totalDay0);
          (baseDate.AddDays 1., totalDay1) ]

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
    let baseDate1 = baseDate.AddDays 1.

    let sourceData: CasesByAgeGroupsTimeline =
        [ { Date = baseDate; Cases = day0 }
          { Date = baseDate1; Cases = day1 } ]

    let timeline = extractTimelineForAgeGroup (groupKey 1) sourceData

    test <@ timeline.Length = 2 @>
    test <@ timeline.[0].Date = baseDate @>
    test <@ timeline.[0].Cases = Some 15 @>
    test <@ timeline.[1].Date = baseDate1 @>
    test <@ timeline.[1].Cases = Some 25 @>
