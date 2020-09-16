module CountriesChartTests.``Averaging country data``

open CountriesChartViz.Analysis
open System
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Calculates centered moving average of country entries data``() =
    let entries =
        [
            "2020-04-29", 1.
            "2020-04-30", 2.
            "2020-05-01", 3.
            "2020-05-02", 4.
            "2020-05-03", 5.
            "2020-05-04", 6.
            "2020-05-05", 7.
        ]
        |> List.map (fun (dateStr, value) ->
            { Date = DateTime.Parse(dateStr); Value = value })
        |> List.toArray

    let averages = entries |> calculateMovingAverages 5

    test <@ averages.Length = 3 @>
    test <@ averages.[0].Date = DateTime(2020, 05, 01) @>
    test <@ averages.[0].Value = (1. + 2. + 3. + 4. + 5.) / 5. @>
    test <@ averages.[1].Date = DateTime(2020, 05, 02) @>
    test <@ averages.[1].Value = (2. + 3. + 4. + 5. + 6.) / 5. @>
    test <@ averages.[2].Date = DateTime(2020, 05, 03) @>
