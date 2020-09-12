module CountriesChartViz.Analysis

open Data.OurWorldInData
open Types
open System

type MetricToDisplay = NewCasesPer1M | ActiveCasesPer1M | TotalDeathsPer1M

type CountryDataDayEntry = {
    Date: DateTime
    Value: float
}

type CountryData = {
    IsoCode : CountryIsoCode
    Entries: CountryDataDayEntry[]
}

type CountriesData = Map<CountryIsoCode, CountryData>

let groupEntriesByCountries
    (metricToDisplay: MetricToDisplay) (entries: DataPoint list)
    : CountriesData =

    let transformFromRawOwid (entryRaw: DataPoint): CountryDataDayEntry =
        let valueToUse =
            match metricToDisplay with
            | NewCasesPer1M ->
                entryRaw.NewCasesPerMillion |> Option.defaultValue 0.
            | ActiveCasesPer1M ->
                entryRaw.NewCasesPerMillion |> Option.defaultValue 0.
            | TotalDeathsPer1M ->
                entryRaw.TotalDeathsPerMillion |> Option.defaultValue 0.

        { Date = entryRaw.Date; Value = valueToUse }

    let groupedRaw =
        entries |> Seq.groupBy (fun entry -> entry.CountryCode)

    groupedRaw
    |> Seq.map (fun (isoCode, countryEntriesRaw) ->
        let countryEntries =
            countryEntriesRaw
            |> Seq.map transformFromRawOwid
            |> Seq.toArray

        (isoCode, { IsoCode = isoCode; Entries = countryEntries } )
    ) |> Map.ofSeq

let calculateActiveCases (countryEntries: CountryDataDayEntry[]) =
    let entriesCount = countryEntries.Length

    let mutable runningActiveCases = 0.
    Array.init entriesCount (fun i ->
        let countryEntry = countryEntries.[i]

        runningActiveCases <- runningActiveCases + countryEntry.Value
        if i >= 14 then
            runningActiveCases <-
                runningActiveCases - countryEntries.[i - 14].Value

        { countryEntry with Value = runningActiveCases }
    )

let calculateMovingAverages
    daysOfMovingAverage
    (countryEntries: CountryDataDayEntry[]) =

    let entriesCount = countryEntries.Length
    let cutOff = daysOfMovingAverage / 2
    let averagesSetLength = entriesCount - cutOff * 2

    let averages: CountryDataDayEntry[] = Array.zeroCreate averagesSetLength

    let daysOfMovingAverageFloat = float daysOfMovingAverage
    let mutable currentNewCasesSum = 0.

    let movingAverageFunc index =
        let entry = countryEntries.[index]

        currentNewCasesSum <- currentNewCasesSum + entry.Value

        match index with
        | index when index >= daysOfMovingAverage - 1 ->
            let date = countryEntries.[index - cutOff].Date
            let newCasesAvg = currentNewCasesSum / daysOfMovingAverageFloat

            averages.[index - (daysOfMovingAverage - 1)] <- {
                Date = date; Value = newCasesAvg }

            let entryToRemove =
                countryEntries.[index - (daysOfMovingAverage - 1)]
            currentNewCasesSum <- currentNewCasesSum - entryToRemove.Value

        | _ -> ignore()

    for i in 0 .. entriesCount-1 do
        movingAverageFunc i

    averages

type OwidDataState =
    | NotLoaded
    | PreviousAndLoadingNew of OurWorldInDataRemoteData
    | Current of OurWorldInDataRemoteData

let aggregateOurWorldInData
    daysOfMovingAverage
    (metricToDisplay: MetricToDisplay)
    (owidDataState: OwidDataState)
    : CountriesData option =

    let doAggregate (owidData: OurWorldInDataRemoteData): CountriesData option =
        match owidData with
        | Success dataPoints ->
            let groupedByCountries: CountriesData =
                dataPoints |> (groupEntriesByCountries metricToDisplay)

            let averagedAndFilteredByCountries: CountriesData  =
                groupedByCountries
                |> Map.map (fun _ (countryData: CountryData) ->
                    let postProcessedEntries =
                        match metricToDisplay with
                        | NewCasesPer1M ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage
                        | ActiveCasesPer1M ->
                            countryData.Entries |> calculateActiveCases
                        | TotalDeathsPer1M ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage
                        |> Array.skipWhile (fun entry -> entry.Value < 0.1)
                    { countryData with Entries = postProcessedEntries })

            Some averagedAndFilteredByCountries
        | _ -> None

    match owidDataState with
    | PreviousAndLoadingNew owidData -> doAggregate owidData
    | Current owidData -> doAggregate owidData
    | NotLoaded -> None
