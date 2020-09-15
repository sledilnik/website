module AgeGroupsChartTests.``Merging infections and deaths``

open Types
open Xunit
open Swensen.Unquote

let infections1 = {
    GroupKey = { AgeFrom = Some 10; AgeTo = Some 20 }
    Male = Some 20; Female = Some 10; All = Some 30
}

let infections2 = {
    GroupKey = { AgeFrom = Some 20; AgeTo = Some 30 }
    Male = Some 25; Female = Some 15; All = Some 40
}

let deaths1 = {
    GroupKey = { AgeFrom = Some 10; AgeTo = Some 20 }
    Male = Some 2; Female = Some 1; All = Some 3
}

let deaths2 = {
    GroupKey = { AgeFrom = Some 20; AgeTo = Some 30 }
    Male = Some 3; Female = Some 4; All = Some 7
}

let expectedInfectionsOnly1: AgeGroupsChart.InfectionsAndDeathsForAgeGroup = {
    GroupKey = { AgeFrom = Some 10; AgeTo = Some 20 }
    InfectionsMale = Some 20; InfectionsFemale = Some 10
    DeathsMale = None; DeathsFemale = None
}

let expectedInfectionsAndDeaths1: AgeGroupsChart.InfectionsAndDeathsForAgeGroup = {
    GroupKey = { AgeFrom = Some 10; AgeTo = Some 20 }
    InfectionsMale = Some 20; InfectionsFemale = Some 10
    DeathsMale = Some 2; DeathsFemale = Some 1
}

let expectedInfectionsAndDeaths2: AgeGroupsChart.InfectionsAndDeathsForAgeGroup = {
    GroupKey = { AgeFrom = Some 20; AgeTo = Some 30 }
    InfectionsMale = Some 25; InfectionsFemale = Some 15
    DeathsMale = Some 3; DeathsFemale = Some 4
}

[<Fact>]
let ``Merging empty groups results in empty result`` () =
    let infections = []
    let deaths = []

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result |> Array.isEmpty @>

[<Fact>]
let ``Group can have infections and no deaths``() =
    let infections = [ infections1 ]
    let deaths = []

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result = [| expectedInfectionsOnly1 |] @>

[<Fact>]
let ``If group has no infections and has deaths, it is ignored as invalid``() =
    let infections = []
    let deaths = [ deaths1 ]

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result |> Array.isEmpty @>

[<Fact>]
let ``Merges infections and deaths together``() =
    let infections = [ infections1 ]
    let deaths = [ deaths1 ]

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result = [| expectedInfectionsAndDeaths1 |] @>

[<Fact>]
let ``Matches age groups by their keys``() =
    let infections = [ infections1; infections2 ]
    let deaths = [ deaths2; deaths1 ]

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result = [|
        expectedInfectionsAndDeaths1; expectedInfectionsAndDeaths2
    |] @>

[<Fact>]
let ``Returns groups sorted descending by age``() =
    let infections = [ infections2; infections1 ]
    let deaths = [ deaths2; deaths1 ]

    let result =
        AgeGroupsChart.mergeInfectionsAndDeathsByGroups infections deaths

    test <@ result = [|
        expectedInfectionsAndDeaths1; expectedInfectionsAndDeaths2
    |] @>
