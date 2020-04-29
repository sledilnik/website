module CountriesChartTests.``Grouping OWID data by countries``

open System
open Data.OurWorldInData
open Xunit
open Swensen.Unquote

type CountryDataDayEntry = {
    Date: DateTime
    TotalCases: float
    TotalCasesPerMillion : float
    TotalDeaths: float
    TotalDeathsPerMillion : float
}

let groupEntriesByCountries (entries: DataPoint list)
    : Map<CountryIsoCode, CountryDataDayEntry[]> =

    let transformFromRawOwid (entryRaw: DataPoint): CountryDataDayEntry =
        let dateStr = entryRaw.Date
        let date = DateTime.Parse(dateStr)

        { Date = date
          TotalCases = float entryRaw.TotalCases
          TotalCasesPerMillion =
              entryRaw.TotalCasesPerMillion
              |> Option.defaultValue 0.
          TotalDeaths = float entryRaw.TotalDeaths
          TotalDeathsPerMillion =
              entryRaw.TotalDeathsPerMillion
              |> Option.defaultValue 0.
        }

    let groupedRaw =
        entries |> Seq.groupBy (fun entry -> entry.CountryCode)

    groupedRaw
    |> Seq.map (fun (isoCode, countryEntriesRaw) ->
        let countryEntries =
            countryEntriesRaw
            |> Seq.map transformFromRawOwid
            |> Seq.toArray

        (isoCode, countryEntries)
    ) |> Map.ofSeq

[<Fact>]
let ``Groups entries by countries``() =
    let entries =
        [
            "SVN", "2020-04-29", 1, None, 2, Some 3.
            "AUT", "2020-04-29", 2, Some 5., 3, Some 4.
            "SVN", "2020-04-30", 3, None, 4, Some 5.
        ]
        |> List.map(fun (code, date, cases, casesPerM, deaths, deathsPerM) ->
            ( { CountryCode = code; Date = date
                TotalCases = cases; TotalCasesPerMillion = casesPerM
                TotalDeaths = deaths; TotalDeathsPerMillion = deathsPerM })
            )

    let grouped = groupEntriesByCountries entries

    test <@ grouped |> Map.count = 2 @>

    test <@ grouped.ContainsKey "SVN" @>
    test <@ grouped.["SVN"].Length = 2 @>
    test <@ grouped.["SVN"].[0].Date = DateTime(2020, 04, 29) @>
    test <@ grouped.["SVN"].[0].TotalCases = 1. @>
    test <@ grouped.["SVN"].[0].TotalCasesPerMillion = 0. @>
    test <@ grouped.["SVN"].[0].TotalDeaths = 2. @>
    test <@ grouped.["SVN"].[0].TotalDeathsPerMillion = 3. @>
    test <@ grouped.["SVN"].[1].Date = DateTime(2020, 04, 30) @>

    test <@ grouped.["AUT"].Length = 1 @>
    test <@ grouped.ContainsKey "AUT" @>
