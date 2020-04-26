module CountriesChartViz.Analysis

open System

type SeriesValue<'XAxis> = 'XAxis * float
type SeriesValues<'XAxis> = SeriesValue<'XAxis>[]

type CountryIsoCode = string

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: SeriesValues<DateTime>
}

type CountriesData = Map<CountryIsoCode, CountryData>

type CountriesSelection =
    | Scandinavia

/// <summary>
/// A function that returns the key for a given data value object.
/// </summary>
/// <typeparam name="'T">The type of data value object.</typeparam>
/// <typeparam name="'TKey">The type of key for the value object
/// (like int or DateTime).</typeparam>
type ValueItemKeyFunc<'T, 'TKey> = ('T -> 'TKey)

/// <summary>
/// A function that returns the value for a given data value object.
/// </summary>
/// <typeparam name="'T">The type of data value object.</typeparam>
type ValueItemValueFunc<'T> = ('T -> float)

/// <summary>
/// A function that calculates the moving average value for a given array
/// values.
/// </summary>
/// <typeparam name="'TKey">The type of key for the value
/// (like int or DateTime).</typeparam>
/// <typeparam name="'TValue">The type of value.</typeparam>
/// <param name="keyFunc">The function that returns the key of the pair.</param>
/// <param name="valueFunc">The function that returns the value of the pair.
/// </param>
/// <returns>A key-value pair containing the average value and its
/// corresponding key.</returns>
type MovingAverageFunc<'TKey, 'TValue> =
    ValueItemKeyFunc<'TKey, 'TValue> -> ValueItemValueFunc<'TKey> -> 'TKey[]
     -> ('TValue * float)

/// <summary>
/// Calculates the centered moving average for a given array of values.
/// </summary>
/// <remarks>
/// The centered moving average takes the day that is at the center of the
/// values array as the target day of the average.
/// </remarks>
/// <typeparam name="'TKey">The type of key for the value
/// (like int or DateTime).</typeparam>
/// <typeparam name="'TValue">The type of value.</typeparam>
/// <param name="keyFunc">The function that returns the key of the pair.</param>
/// <param name="valueFunc">The function that returns the value of the pair.
let movingAverageCentered: MovingAverageFunc<'TKey, 'TValue> =
    fun keyFunc valueFunc values ->
    match (values |> Seq.length) % 2 with
    | 1 ->
        let centerIndex = (values |> Seq.length) / 2
        let targetKey = values.[centerIndex] |> keyFunc
        let averageValue = values |> Seq.averageBy valueFunc
        (targetKey, averageValue)
    | _ -> ArgumentException "values array length needs to be an odd number"
           |> raise

/// <summary>
/// Calculates an array of moving averages array for a given array values.
/// </summary>
/// <typeparam name="'T">The type of the object holding an individual value.
/// <typeparam name="'TKey">The type of key for the value
/// (like int or DateTime).</typeparam>
/// <param name="keyFunc">The function that returns the key of the pair.</param>
/// <param name="valueFunc">The function that returns the value of the pair.
let movingAverages<'T, 'TKey>
    (averageFunc: MovingAverageFunc<'T, 'TKey>)
    (daysOfMovingAverage: int)
    (keyFunc: ValueItemKeyFunc<'T, 'TKey>)
    (valueFunc: ValueItemValueFunc<'T>)
    (series: 'T[])
    : ('TKey * float)[] =
    series
    |> Array.windowed daysOfMovingAverage
    |> Array.map (averageFunc keyFunc valueFunc)


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
                    (date, deathsPerMillion) )
                |> Seq.toArray
                |> (movingAverages
                        movingAverageCentered daysOfMovingAverage
                        (fun (day, _) -> day) (fun (_, value) -> value))
            { CountryIsoCode = isoCode
              CountryName = countryName
              Data = dailyEntries }
            )

    countriesData
    |> Seq.map (fun x -> (x.CountryIsoCode, x))
    |> Map.ofSeq
