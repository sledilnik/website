module Statistics

open System
open Highcharts

type SeriesValue<'XAxis> = 'XAxis * float
type SeriesValues<'XAxis> = SeriesValue<'XAxis>[]

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
        let averageValue =
            values |> Seq.averageBy valueFunc
        (targetKey, averageValue)
    | _ -> ArgumentException "values array length needs to be an odd number"
           |> raise

/// <summary>
/// Calculates the trailing moving average for a given array of values.
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
/// <remarks>
/// The trailing moving average takes the last day as the target day of the
/// average.
/// </remarks>
let movingAverageTrailing: MovingAverageFunc<'TKey, 'TValue> =
    fun keyFunc valueFunc values ->
    let targetKey = values |> Array.last |> keyFunc
    let averageValue = values |> Seq.averageBy valueFunc
    (targetKey, averageValue)

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

/// running average is calculated for end date of the interval (last 7 days)
let calcRunningAverage (data: ('a * float)[]) =
    let daysOfMovingAverage = 7
    let roundToDecimals = 1

    let entriesCount = data.Length

    if entriesCount >= daysOfMovingAverage then
        let cutOff = daysOfMovingAverage / 2
        let averagedDataLength = entriesCount - cutOff * 2

        let averages = Array.zeroCreate averagedDataLength

        let daysOfMovingAverageFloat = float daysOfMovingAverage
        let mutable currentSum = 0.

        let movingAverageFunc index =
            let (_, entryValue) = data.[index]

            currentSum <- currentSum + entryValue

            match index with
            | index when index >= daysOfMovingAverage - 1 ->
                let date = data.[index] |> fst
                let average =
                    currentSum / daysOfMovingAverageFloat
                    |> Utils.roundDecimals roundToDecimals

                averages.[index - (daysOfMovingAverage - 1)] <- (date, average)

                let valueToSubtract =
                    data.[index - (daysOfMovingAverage - 1)] |> snd
                currentSum <- currentSum - valueToSubtract

            | _ -> ignore()

        for i in 0 .. entriesCount-1 do
            movingAverageFunc i

        averages
    else
        [||]

let roundKeyValueFloatArray<'TKey> howManyDecimals (array: ('TKey * float)[])
    : ('TKey * float)[] =
    array
    |> Array.map (fun (key, value) ->
        (key, value |> Utils.roundDecimals howManyDecimals))

let calculateWindowedSumInt windowSize (data: int[]): int[] =
    let len = data |> Array.length

    let mutable runningSum = 0

    let runningSumForItem index =
        let addValue = data.[index]
        if index >= windowSize then
            let subValue = data.[index - windowSize]
            runningSum <- runningSum + addValue - subValue
        else
            runningSum <- runningSum + addValue
        runningSum

    Array.init len runningSumForItem
