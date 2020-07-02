module AgeGroupsTimelineTests.``Calculating cases by age timeline``

open System
open AgeGroupsTimelineViz.Analysis
open TestHelpers
open Xunit
open Swensen.Unquote

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
          [ { Date = baseDate; Cases = onDay0 }
            { Date = baseDate.AddDays 1.; Cases = onDay1 }
            { Date = baseDate.AddDays 2.; Cases = onDay2 } ]

    test <@ calculateCasesByAgeTimeline sourceData = expectedTimeline @>
