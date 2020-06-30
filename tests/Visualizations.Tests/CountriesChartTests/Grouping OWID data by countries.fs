module CountriesChartTests.``Grouping OWID data by countries``

open Data.OurWorldInData
open CountriesChartViz.Analysis
open System
open Xunit
open Swensen.Unquote


[<Fact>]
let ``Groups entries by countries``() =
    let entries =
        [
            "SVN", System.DateTime(2020, 4, 29), 1, None, 2, Some 3.
            "AUT", System.DateTime(2020, 4, 29), 2, Some 5., 3, Some 4.
            "SVN", System.DateTime(2020, 4, 30), 3, None, 4, Some 5.
        ]
        |> List.map(fun (code, date, cases, casesPerM, deaths, deathsPerM) ->
            ( { CountryCode = code; Date = date
                NewCases = 0; NewCasesPerMillion = None
                TotalCases = cases; TotalCasesPerMillion = casesPerM
                TotalDeaths = deaths; TotalDeathsPerMillion = deathsPerM })
            )

    let grouped = groupEntriesByCountries entries

    test <@ grouped |> Map.count = 2 @>

    test <@ grouped.ContainsKey "SVN" @>
    test <@ grouped.["SVN"].Entries.Length = 2 @>

    let sampleEntry = grouped.["SVN"].Entries.[0]
    test <@ sampleEntry.Date = DateTime(2020, 04, 29) @>
    test <@ sampleEntry.TotalCases = 1. @>
    test <@ sampleEntry.TotalCasesPerMillion = 0. @>
    test <@ sampleEntry.TotalDeaths = 2. @>
    test <@ sampleEntry.TotalDeathsPerMillion = 3. @>
    test <@ grouped.["SVN"].Entries.[1].Date = DateTime(2020, 04, 30) @>

    test <@ grouped.["AUT"].Entries.Length = 1 @>
    test <@ grouped.ContainsKey "AUT" @>
