module CountriesChartsTests.``Reading sample data``

open Xunit
open Swensen.Unquote

[<Fact>]
let ``Data is parsed correctly``() =
    let data = CountriesChart.parseCountriesCsv()
    test <@ data |> Map.count = 4 @>
    test <@ data.["NOR"].Data |> Array.length = 38 @>
