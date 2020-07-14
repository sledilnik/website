module ``Calculating windowed sums``

open Xunit
open Swensen.Unquote
open Statistics

[<Fact>]
let ``Can calculate windowed sum``() =
    let sourceData = [| 1; 2; 3; 4; 5; 6; 7 |]
    let expectedSum = [| 1; 3; 6; 9; 12; 15; 18 |]

    test <@ sourceData |> calculateWindowedSumInt 3 = expectedSum @>
