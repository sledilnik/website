module AgeGroupsTimelineTests.``Calculating cases by age timeline``

open System
open DataAnalysis.DatedTypes
open DataAnalysis.AgeGroupsTimeline
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

    let sourceData = {
            StartDate = baseDate
            Data = [| totalDay0; totalDay1; totalDay2 |]
        }

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

    let expectedTimeline = {
            StartDate = baseDate
            Data = [| onDay0; onDay1; onDay2 |]
        }

    test <@ calculateDailyCasesByAgeTimeline sourceData = expectedTimeline @>

[<Fact>]
let ``Filters out leading and trailing days without any cases``() =
    let totalDay0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 0))
        |> withGroup (group 1 None (Some 0))
    let totalDay1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))
    let totalDay2 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))
    let totalDay3 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 5) (Some 23))
        |> withGroup (group 1 (Some 13) (Some 25))
    let totalDay4 =
        buildAgeGroups()
        |> withGroup (group 2 None None)
        |> withGroup (group 1 (Some 0) (Some 0))

    let baseDate = DateTime(2020, 03, 01)

    let sourceData = {
            StartDate = baseDate
            Data = [| totalDay0; totalDay1; totalDay2; totalDay3; totalDay4 |]
        }

    let onDay0 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))
    let onDay1 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 0))
        |> withGroup (group 1 (Some 0) (Some 0))
    let onDay2 =
        buildAgeGroups()
        |> withGroup (group 2 (Some 4) (Some 3))
        |> withGroup (group 1 (Some 3) (Some 10))

    let expectedTimeline = {
            StartDate = baseDate.AddDays 1.
            Data = [| onDay0; onDay1; onDay2 |]
        }

    test <@ calculateDailyCasesByAgeTimeline sourceData = expectedTimeline @>
