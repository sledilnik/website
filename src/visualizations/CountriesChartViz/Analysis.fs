module CountriesChartViz.Analysis

open Data.OurWorldInData
open Types
open System

type MetricToDisplay =
    | NewCasesPer1M
    | ActiveCasesPer1M
    | TotalDeathsPer1M
    | DeathsPerCases

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
                (entryRaw.NewCasesPerMillion |> Option.defaultValue 0.) / 10.
            | ActiveCasesPer1M ->
                (entryRaw.NewCasesPerMillion |> Option.defaultValue 0.) / 10.
            | TotalDeathsPer1M ->
                (entryRaw.TotalDeathsPerMillion |> Option.defaultValue 0.) / 10.
            | DeathsPerCases ->
                if entryRaw.TotalCases > 0 then
                    (float entryRaw.TotalDeaths) * 100.0
                        / (float entryRaw.TotalCases)
                else
                    0.


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
    let mutable currentSum = 0.

    let movingAverageFunc index =
        let entry = countryEntries.[index]

        currentSum <- currentSum + entry.Value

        match index with
        | index when index >= daysOfMovingAverage - 1 ->
            let date = countryEntries.[index - cutOff].Date
            let average = currentSum / daysOfMovingAverageFloat

            averages.[index - (daysOfMovingAverage - 1)] <- {
                Date = date; Value = average }

            let entryToRemove =
                countryEntries.[index - (daysOfMovingAverage - 1)]
            currentSum <- currentSum - entryToRemove.Value

        | _ -> ignore()

    for i in 0 .. entriesCount-1 do
        movingAverageFunc i

    averages

type OwidDataState =
    | NotLoaded
    | PreviousAndLoadingNew of OurWorldInDataRemoteData
    | Current of OurWorldInDataRemoteData

let SloveniaPopulationInM =
    (Utils.Dictionaries.regions.["si"].Population
    |> Utils.optionToInt
    |> float)
    / 1000000.

let buildFromSloveniaDomesticData (statsData: StatsData) (date: DateTime)
        : DataPoint =
    let domesticDataForDate =
        statsData
        |> List.tryFind(fun dataForDate -> dataForDate.Date = date)

    match domesticDataForDate with
    | Some domesticDataForDate ->
        let newCases =
            domesticDataForDate.Cases.ConfirmedToday
            |> Utils.optionToInt
        let newCasesPerMillion =
            (float newCases) / SloveniaPopulationInM |> Some
        let totalCases =
            domesticDataForDate.Cases.ConfirmedToDate
            |> Utils.optionToInt
        let totalCasesPerMillion =
            (float totalCases) / SloveniaPopulationInM |> Some
        let totalDeaths =
            domesticDataForDate.StatePerTreatment.DeceasedToDate
            |> Utils.optionToInt
        let totalDeathsPerMillion =
            (float totalDeaths) / SloveniaPopulationInM |> Some

        {
            CountryCode = "SVN"; Date = date
            NewCases = newCases; NewCasesPerMillion = newCasesPerMillion
            TotalCases = totalCases; TotalCasesPerMillion = totalCasesPerMillion
            TotalDeaths = totalDeaths; TotalDeathsPerMillion = totalDeathsPerMillion
        }
    | None ->
        {
            CountryCode = "SVN"; Date = date
            NewCases = 0; NewCasesPerMillion = None
            TotalCases = 0; TotalCasesPerMillion = None
            TotalDeaths = 0; TotalDeathsPerMillion = None
        }

let updateWithSloveniaDomesticData
        (statsData: StatsData) (countryData: DataPoint): DataPoint =
    match countryData.CountryCode with
    | "SVN" -> countryData.Date |> buildFromSloveniaDomesticData statsData
    | _ -> countryData

let aggregateOurWorldInData
    daysOfMovingAverage
    (metricToDisplay: MetricToDisplay)
    (owidDataState: OwidDataState)
    (statsData: StatsData)
    : CountriesData option =

    let doAggregate (owidData: OurWorldInDataRemoteData): CountriesData option =
        match owidData with
        | Success dataPoints ->
            let dataPointsWithLocalSloveniaData =
                dataPoints
                |> List.map (updateWithSloveniaDomesticData statsData)

            let groupedByCountries: CountriesData =
                dataPointsWithLocalSloveniaData
                |> (groupEntriesByCountries metricToDisplay)

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
                        | DeathsPerCases ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage

                    let minValueFilter =
                        match metricToDisplay with
                        | NewCasesPer1M -> 0.1
                        | ActiveCasesPer1M -> 0.1
                        | TotalDeathsPer1M -> 0.1
                        | DeathsPerCases -> 0.001

                    let trimmedEntries =
                        postProcessedEntries
                        |> Array.skipWhile
                               (fun entry -> entry.Value < minValueFilter)

                    { countryData with Entries = trimmedEntries })

            Some averagedAndFilteredByCountries
        | _ -> None

    match owidDataState with
    | PreviousAndLoadingNew owidData -> doAggregate owidData
    | Current owidData -> doAggregate owidData
    | NotLoaded -> None
