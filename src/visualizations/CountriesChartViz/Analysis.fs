module CountriesChartViz.Analysis

open Data.OurWorldInData
open Types
open System

type MetricToDisplay =
    | NewCasesPer100k
    | ActiveCasesPer100k
    | NewDeathsPer100k
    | TotalDeathsPer100k
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

    let transformFromRawOwid (entryRaw: DataPoint)
            : CountryDataDayEntry option =
        match metricToDisplay with
        | NewCasesPer100k ->
            match entryRaw.NewCasesPerMillion with
            | Some value -> Some { Date = entryRaw.Date; Value = value / 10. }
            | None -> None
        | ActiveCasesPer100k ->
            match entryRaw.NewCasesPerMillion with
            | Some value -> Some { Date = entryRaw.Date; Value = value / 10. }
            | None -> None
        | NewDeathsPer100k ->
            match entryRaw.NewDeathsPerMillion with
            | Some value -> Some { Date = entryRaw.Date; Value = value / 10. }
            | None -> None
        | TotalDeathsPer100k ->
            match entryRaw.TotalDeathsPerMillion with
            | Some value -> Some { Date = entryRaw.Date; Value = value / 10. }
            | None -> None
        | DeathsPerCases ->
            match entryRaw.TotalDeaths, entryRaw.TotalCases with
            | Some totalDeaths, Some totalCases ->
                if totalCases > 0 then
                    let value = (float totalDeaths) * 100.0 / (float totalCases)
                    Some { Date = entryRaw.Date; Value = value }
                else
                    None
            | _ -> None

    let groupedRaw =
        entries |> Seq.groupBy (fun entry -> entry.CountryCode)

    groupedRaw
    |> Seq.map (fun (isoCode, countryEntriesRaw) ->
        let countryEntries =
            countryEntriesRaw
            |> Seq.choose transformFromRawOwid
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

let buildFromSloveniaDomesticData
        (statsData: StatsData)
        (offsetDays: int)
        (date: DateTime)
        : DataPoint option =
    let actualTargetDate = date.AddDays(float offsetDays)

    let domesticDataForDate =
        statsData
        |> List.tryFind
               (fun dataForDate -> dataForDate.Date = actualTargetDate)

    let extractMetricIfPresent (metricValue: int option)
            : (int option * float option) =
        match metricValue with
        | Some value ->
            (Some value, (float value) / SloveniaPopulationInM |> Some)
        | None -> (None, None)

    match domesticDataForDate with
    | Some domesticDataForDate ->
        let newCases, newCasesPerM =
            extractMetricIfPresent domesticDataForDate.Cases.ConfirmedToday
        let totalCases, totalCasesPerM =
            extractMetricIfPresent domesticDataForDate.Cases.ConfirmedToDate
        let newDeaths, newDeathsPerM =
            extractMetricIfPresent
                domesticDataForDate.StatePerTreatment.Deceased
        let totalDeaths, totalDeathsPerM =
            extractMetricIfPresent
                domesticDataForDate.StatePerTreatment.DeceasedToDate

        {
            CountryCode = "SVN"; Date = date
            NewCases = newCases
            NewCasesPerMillion = newCasesPerM
            TotalCases = totalCases
            TotalCasesPerMillion = totalCasesPerM
            NewDeaths = newDeaths
            NewDeathsPerMillion = newDeathsPerM
            TotalDeaths = totalDeaths
            TotalDeathsPerMillion = totalDeathsPerM
        } |> Some
    | None -> None

let updateWithSloveniaDomesticData
        (statsData: StatsData)
        (offsetDays: int)
        (countryData: DataPoint): DataPoint option =
    match countryData.CountryCode with
    | "SVN" ->
        countryData.Date
        |> buildFromSloveniaDomesticData statsData offsetDays
    | _ -> Some countryData

let findLatestDateWithDomesticData metricToDisplay statsData: DateTime =
    let hasData (dataPoint: StatsDataPoint) =
        match metricToDisplay with
        | NewCasesPer100k ->
            dataPoint.Cases.ConfirmedToday.IsSome
        | ActiveCasesPer100k ->
            dataPoint.Cases.ConfirmedToDate.IsSome
        | NewDeathsPer100k ->
            dataPoint.StatePerTreatment.Deceased.IsSome
        | TotalDeathsPer100k ->
            dataPoint.StatePerTreatment.DeceasedToDate.IsSome
        | DeathsPerCases ->
            dataPoint.StatePerTreatment.DeceasedToDate.IsSome
            && dataPoint.Cases.ConfirmedToDate.IsSome

    let foundDataPoint =
        statsData
        |> List.rev
        |> List.find hasData

    foundDataPoint.Date.Date

let findLatestDateWithOwidData metricToDisplay owidData: DateTime =
    let hasData (dataPoint: DataPoint) =
        match metricToDisplay with
        | NewCasesPer100k -> dataPoint.NewCasesPerMillion.IsSome
        | ActiveCasesPer100k -> dataPoint.NewCasesPerMillion.IsSome
        | NewDeathsPer100k -> dataPoint.NewDeathsPerMillion.IsSome
        | TotalDeathsPer100k -> dataPoint.TotalDeathsPerMillion.IsSome
        | DeathsPerCases ->
            dataPoint.TotalDeathsPerMillion.IsSome
            && dataPoint.NewCasesPerMillion.IsSome

    let foundDataPoint =
        owidData
        |> List.rev
        |> List.find hasData

    foundDataPoint.Date.Date


let aggregateOurWorldInData
    daysOfMovingAverage
    (metricToDisplay: MetricToDisplay)
    (owidDataState: OwidDataState)
    (statsData: StatsData)
    : CountriesData option =

    let determineDayDifferenceBetweenOwidAndDomesticData owidData =
        // First calculate the difference between the latest OWID date
        // that has the target metric and the latest Slovenian domestic
        // date that has the target metric.
        // This is done so we can move the Slovenian data one
        // day forward to ensure the charts include Slovenia for the latest
        // date (so the countries can be easily compared by users).
        // See https://github.com/sledilnik/website/issues/689
        let latestDateWithDomesticData =
            statsData |> findLatestDateWithDomesticData metricToDisplay

        let latestDateWithOwidData =
            owidData |> findLatestDateWithOwidData metricToDisplay

        Days.between latestDateWithOwidData latestDateWithDomesticData

    let doAggregate (owidData: OurWorldInDataRemoteData): CountriesData option =
        match owidData with
        | Success owidData ->
            let dayDiff = determineDayDifferenceBetweenOwidAndDomesticData
                              owidData

            let dataPointsWithLocalSloveniaData =
                owidData
                |> List.choose
                       (updateWithSloveniaDomesticData statsData dayDiff)

            let groupedByCountries: CountriesData =
                dataPointsWithLocalSloveniaData
                |> (groupEntriesByCountries metricToDisplay)

            let averagedAndFilteredByCountries: CountriesData  =
                groupedByCountries
                |> Map.map (fun _ (countryData: CountryData) ->
                    let postProcessedEntries =
                        match metricToDisplay with
                        | NewCasesPer100k ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage
                        | ActiveCasesPer100k ->
                            countryData.Entries |> calculateActiveCases
                        | NewDeathsPer100k ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage
                        | TotalDeathsPer100k ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage
                        | DeathsPerCases ->
                            countryData.Entries
                            |> calculateMovingAverages daysOfMovingAverage

                    let minValueFilter =
                        match metricToDisplay with
                        | NewCasesPer100k -> 0.1
                        | ActiveCasesPer100k -> 0.1
                        | NewDeathsPer100k -> 0.001
                        | TotalDeathsPer100k -> 0.1
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
