module CountriesChartTests.``Averaging country data``

open CountriesChartViz.Analysis
open System
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Calculates centered moving average of country entries data``() =
    let entries =
        [
            "2020-04-29", 1., 2., 3., 4., 5., 6.
            "2020-04-30", 2., 3., 4., 5., 6., 7.
            "2020-05-01", 3., 4., 5., 6., 7., 8.
            "2020-05-02", 4., 5., 6., 7., 8., 9.
            "2020-05-03", 5., 6., 7., 8., 9., 10.
            "2020-05-04", 6., 7., 8., 9., 10., 11.
            "2020-05-05", 7., 8., 9., 10., 11., 12.
        ]
        |> List.map (fun (dateStr, cases, cases1M, deaths, deaths1M,
                          newCases, newCases1M) ->
            { Date = DateTime.Parse(dateStr)
              TotalCases = cases; TotalCasesPerMillion = cases1M
              NewCases = newCases; NewCasesPerMillion = newCases1M
              TotalDeaths = deaths; TotalDeathsPerMillion = deaths1M
            })
        |> List.toArray

    let averages = entries |> calculateMovingAverages 5

    test <@ averages.Length = 3 @>
    test <@ averages.[0].Date = DateTime(2020, 05, 01) @>
    test <@ averages.[0].TotalCases = (1. + 2. + 3. + 4. + 5.) / 5. @>
    test <@ averages.[0].TotalCasesPerMillion = (2. + 3. + 4. + 5. + 6.) / 5. @>
    test <@ averages.[0].NewCases = (5. + 6. + 7. + 8. + 9.) / 5. @>
    test <@ averages.[0].NewCasesPerMillion = (6. + 7. + 8. + 9. + 10.) / 5. @>
    test <@ averages.[0].TotalDeaths = (3. + 4. + 5. + 6. + 7.) / 5. @>
    test <@ averages.[0].TotalDeathsPerMillion = (4. + 5. + 6. + 7. + 8.) / 5. @>
    test <@ averages.[1].Date = DateTime(2020, 05, 02) @>
    test <@ averages.[1].TotalCases = (2. + 3. + 4. + 5. + 6.) / 5. @>
    test <@ averages.[1].TotalCasesPerMillion = (3. + 4. + 5. + 6. + 7.) / 5. @>
    test <@ averages.[1].TotalDeaths = (4. + 5. + 6. + 7. + 8.) / 5. @>
    test <@ averages.[1].TotalDeathsPerMillion = (5. + 6. + 7. + 8. + 9.) / 5. @>
    test <@ averages.[2].Date = DateTime(2020, 05, 03) @>
