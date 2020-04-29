module CountriesChartTests.``Averaging country data``

open System
open Xunit
open Swensen.Unquote
open CountriesChartTests.``Grouping OWID data by countries``

let calculateMovingAverages
    daysOfMovingAverage
    (countryEntries: CountryDataDayEntry[]) =

    let entriesCount = countryEntries.Length
    let cutOff = daysOfMovingAverage / 2
    let averagesSetLength = entriesCount - cutOff * 2

    let averages: CountryDataDayEntry[] = Array.zeroCreate averagesSetLength

    let daysOfMovingAverageFloat = float daysOfMovingAverage
    let mutable currentCasesSum = 0.
    let mutable currentCases1MSum = 0.
    let mutable currentDeathsSum = 0.
    let mutable currentDeaths1MSum = 0.

    let movingAverageFunc index =
        let entry = countryEntries.[index]

        currentCasesSum <-
            currentCasesSum + entry.TotalCases
        currentCases1MSum <-
            currentCases1MSum + entry.TotalCasesPerMillion
        currentDeathsSum <- currentDeathsSum + entry.TotalDeaths
        currentDeaths1MSum <- currentDeaths1MSum + entry.TotalDeathsPerMillion

        match index with
        | index when index >= daysOfMovingAverage - 1 ->
            let date = countryEntries.[index - cutOff].Date
            let casesAvg = currentCasesSum / daysOfMovingAverageFloat
            let cases1MAvg = currentCases1MSum / daysOfMovingAverageFloat
            let deathsAvg = currentDeathsSum / daysOfMovingAverageFloat
            let deaths1MAvg = currentDeaths1MSum / daysOfMovingAverageFloat

            averages.[index - (daysOfMovingAverage - 1)] <- {
                Date = date
                TotalCases = casesAvg; TotalCasesPerMillion = cases1MAvg
                TotalDeaths = deathsAvg; TotalDeathsPerMillion = deaths1MAvg
            }

            let entryToRemove = countryEntries.[index - (daysOfMovingAverage - 1)]
            currentCasesSum <-
                currentCasesSum - entryToRemove.TotalCases
            currentCases1MSum <-
                currentCases1MSum - entryToRemove.TotalCasesPerMillion
            currentDeathsSum <- currentDeathsSum - entryToRemove.TotalDeaths
            currentDeaths1MSum <-
                currentDeaths1MSum - entryToRemove.TotalDeathsPerMillion

        | _ -> ignore()

    for i in 0 .. entriesCount-1 do
        movingAverageFunc i

    averages


[<Fact>]
let ``Calculates centered moving average of country entries data``() =
    let entries =
        [
            "2020-04-29", 1., 2., 3., 4.
            "2020-04-30", 2., 3., 4., 5.
            "2020-05-01", 3., 4., 5., 6.
            "2020-05-02", 4., 5., 6., 7.
            "2020-05-03", 5., 6., 7., 8.
            "2020-05-04", 6., 7., 8., 9.
            "2020-05-05", 7., 8., 9., 10.
        ]
        |> List.map (fun (dateStr, cases, cases1M, deaths, deaths1M) ->
            { Date = DateTime.Parse(dateStr)
              TotalCases = cases; TotalCasesPerMillion = cases1M
              TotalDeaths = deaths; TotalDeathsPerMillion = deaths1M
            })
        |> List.toArray

    let averages = entries |> calculateMovingAverages 5

    test <@ averages.Length = 3 @>
    test <@ averages.[0].Date = DateTime(2020, 05, 01) @>
    test <@ averages.[0].TotalCases = (1. + 2. + 3. + 4. + 5.) / 5. @>
    test <@ averages.[0].TotalCasesPerMillion = (2. + 3. + 4. + 5. + 6.) / 5. @>
    test <@ averages.[0].TotalDeaths = (3. + 4. + 5. + 6. + 7.) / 5. @>
    test <@ averages.[0].TotalDeathsPerMillion = (4. + 5. + 6. + 7. + 8.) / 5. @>
    test <@ averages.[1].Date = DateTime(2020, 05, 02) @>
    test <@ averages.[1].TotalCases = (2. + 3. + 4. + 5. + 6.) / 5. @>
    test <@ averages.[1].TotalCasesPerMillion = (3. + 4. + 5. + 6. + 7.) / 5. @>
    test <@ averages.[1].TotalDeaths = (4. + 5. + 6. + 7. + 8.) / 5. @>
    test <@ averages.[1].TotalDeathsPerMillion = (5. + 6. + 7. + 8. + 9.) / 5. @>
    test <@ averages.[2].Date = DateTime(2020, 05, 03) @>
