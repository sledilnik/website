module AgeGroupsTimelineTests.``Calculating cases by age for day``

open DataAnalysis.AgeGroupsTimeline
open AgeGroupsTimelineTests.TestHelpers
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Can calculate data for first day``() =
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcCasesByAgeForDay None currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 1 (Some 10) (Some 15) @>

[<Fact>]
let ``Can calculate data when both days have same groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 2 @>
    test <@ resultGroup.[0] = group 1 (Some 5) (Some 5) @>
    test <@ resultGroup.[1] = group 2 (Some 1) (Some 5) @>

[<Fact>]
let ``Can calculate data when prev day is missing some groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 1 (Some 5) (Some 10))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 2 @>
    test <@ resultGroup.[0] = group 1 (Some 5) (Some 5) @>
    test <@ resultGroup.[1] = group 2 (Some 1) (Some 20) @>

[<Fact>]
let ``Can calculate data when current day is missing some groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 (Some 1) (Some 5) @>

[<Fact>]
let ``Can calculate data when prev day group has missing numbers``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 None None)
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 (Some 1) (Some 20) @>

[<Fact>]
let ``Can calculate data when current day group has missing numbers``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 None None)

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 None None @>

[<Fact>]
let ``Sets 0 for negative values``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 19))

    let resultGroup = calcCasesByAgeForDay prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 (Some 0) (Some 0) @>
