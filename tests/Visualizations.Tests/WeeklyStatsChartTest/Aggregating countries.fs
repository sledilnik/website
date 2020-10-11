module WeeklyStatsChartTest.``Aggregating countries``

open Types
open Xunit
open Swensen.Unquote

open WeeklyStatsChart

[<Fact>]
let ``Empty list in -> empty map out`` () =
    let sourceData: Map<string, int option> list = []
    let expectedResult: (string*int)[] = [||]

    test <@ sourceData |> countryTotals = expectedResult @>

[<Fact>]
let ``Correctly sums up values by country`` () =
    let sourceData: Map<string, int option> list =
        [ Map.empty.Add("hr", Some 12).Add("nl", None)
          Map.empty.Add("hr", Some 4).Add("be", Some 2).Add("nl", None)
          Map.empty.Add("se", Some 1).Add("ch", None) ]

    let expectedResult: (string*int)[] = [|
        ("hr", 16)
        ("be", 2)
        ("se", 1)
    |]

    test <@ sourceData |> countryTotals = expectedResult @>
