module AgeGroupsChartTests.``Sorting age groups``

open Types
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Younger come first``() =
    let group2 = { AgeFrom = Some 10; AgeTo = Some 20 }
    let group1 = { AgeFrom = Some 5; AgeTo = Some 9 }
    let group3 = { AgeFrom = Some 21; AgeTo = Some 30 }

    let sorted = [ group2; group1; group3 ] |> List.sort

    test <@ sorted = [ group1; group2; group3 ] @>

