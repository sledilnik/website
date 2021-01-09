module ``calcRunningAverage tests``

open Xunit
open Swensen.Unquote
open Statistics

[<Fact>]
let ``Can calculate running average for more than 7 days``() =
    let data = [| (0.0, 0.0); (1.0, 1.0); (2.0, 2.0); (3.0, 3.0)
                  (4.0, 4.0); (5.0, 5.0); (6.0, 6.0); (7.0, 7.0) |]

    let expectedAverages = [| (3.0, 3.0); (4.0, 4.0) |]

    let averages = calcRunningAverage data

    test <@ averages = expectedAverages @>

[<Fact>]
let ``Can calculate running average for exactly 7 days``() =
    let data = [| (0.0, 0.0); (1.0, 1.0); (2.0, 2.0); (3.0, 3.0)
                  (4.0, 4.0); (5.0, 5.0); (6.0, 6.0) |]

    let expectedAverages = [| (3.0, 3.0) |]

    let averages = calcRunningAverage data

    test <@ averages = expectedAverages @>

[<Fact>]
let ``calcRunningAverage returns empty array when there are less than 7 days``() =
    let data = [| (0.0, 0.0); (1.0, 1.0); (2.0, 2.0); (3.0, 3.0) |]

    let averages = calcRunningAverage data

    test <@ averages = [||] @>
