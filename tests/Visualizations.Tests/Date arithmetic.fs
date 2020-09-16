module ``Date arithmetic``

open System
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Can calculate difference of one day``() =
    let date1 = DateTime(2020, 02, 01)
    let date2 = DateTime(2020, 02, 02)
    let expectedDiff = 1

    test <@ Days.between date1 date2 = expectedDiff @>
    test <@ date1 |> Days.add expectedDiff = date2 @>

[<Fact>]
let ``Can calculate difference of dates across daylight saving``() =
    let date1 = DateTime(2020, 02, 01)
    let date2 = DateTime(2020, 04, 05)
    let expectedDiff = 64

    test <@ Days.between date1 date2 = expectedDiff @>
    test <@ date1 |> Days.add expectedDiff = date2 @>

[<Fact>]
let ``Can calculate difference of dates between years``() =
    let date1 = DateTime(2015, 02, 01)
    let date2 = DateTime(2020, 04, 05)
    let expectedDiff = 1890

    test <@ Days.between date1 date2 = expectedDiff @>
    test <@ date1 |> Days.add expectedDiff = date2 @>
