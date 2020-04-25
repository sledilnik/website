module CountriesChartViz.Analysis

open System

type DailyData = {
    Day: DateTime
    TotalDeathsPerMillion: float
}

type CountryIsoCode = string

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: DailyData[]
}

type CountriesData = Map<CountryIsoCode, CountryData>

type CountriesSelection =
    | Scandinavia

/// <summary>
/// A function that calculates the moving average value for a given array of
/// day values.
/// </summary>
type MovingAverageFunc = (DailyData[]) -> DailyData

/// <summary>
/// Calculates the centered moving average for a given array of day values.
/// </summary>
/// <remarks>
/// The centered moving average takes the day that is at the center of the
/// values array as the target day of the average.
/// </remarks>
let movingAverageCentered: MovingAverageFunc = fun (daysValues) ->
    match (daysValues |> Seq.length) % 2 with
    | 1 ->
        let centerIndex = (daysValues |> Seq.length) / 2
        let targetDate = daysValues.[centerIndex].Day
        let averageValue =
            daysValues
            |> Seq.averageBy(
                fun dayValue -> dayValue.TotalDeathsPerMillion)
        { Day = targetDate; TotalDeathsPerMillion = averageValue }
    | _ -> ArgumentException "daysValues needs to be an odd number" |> raise

/// <summary>
/// Calculates the moving averages array for a given array of day values.
/// </summary>
let movingAverages
    (averageFunc: MovingAverageFunc)
    (daysOfMovingAverage: int)
    (series: DailyData[]): DailyData[] =
    series
    |> Array.windowed daysOfMovingAverage
    |> Array.map averageFunc


let parseCountriesCsv daysOfMovingAverage: CountriesData =

    let countriesData =
        Data.CountriesData.countriesRawData
        |> Seq.map (fun entry ->
            let (countryIsoCode, countryName, dateStr, deathsPerMillion)
                = entry

            let date = DateTime.Parse(dateStr)

            (countryIsoCode, countryName, date, deathsPerMillion))
        |> Seq.sortBy (fun (isoCode, _, _, _) -> isoCode)
        |> Seq.groupBy (fun (isoCode, countryName, _, _)
                         -> (isoCode, countryName))
        |> Seq.map (fun ((isoCode, countryName), countryLines) ->
            let dailyEntries =
                countryLines
                |> Seq.map(fun (_, _, date, deathsPerMillion) ->
                    { Day = date; TotalDeathsPerMillion = deathsPerMillion })
                |> Seq.toArray
                |> (movingAverages movingAverageCentered daysOfMovingAverage)
            { CountryIsoCode = isoCode
              CountryName = countryName
              Data = dailyEntries }
            )

    countriesData
    |> Seq.map (fun x -> (x.CountryIsoCode, x))
    |> Map.ofSeq
