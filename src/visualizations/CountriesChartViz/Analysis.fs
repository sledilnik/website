module CountriesChartViz.Analysis

open Data.OurWorldInData
open Types
open System

type CountryDataDayEntry = {
    Date: DateTime
    NewCases: float
    NewCasesPerMillion : float
    TotalCases: float
    TotalCasesPerMillion : float
    TotalDeaths: float
    TotalDeathsPerMillion : float
}

type CountryData = {
    IsoCode : CountryIsoCode
    Entries: CountryDataDayEntry[]
}

type CountriesData = Map<CountryIsoCode, CountryData>

let groupEntriesByCountries (entries: DataPoint list): CountriesData =

    let transformFromRawOwid (entryRaw: DataPoint): CountryDataDayEntry =
        { Date = entryRaw.Date
          NewCases = float entryRaw.NewCases
          NewCasesPerMillion =
              entryRaw.NewCasesPerMillion
              |> Option.defaultValue 0.
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

        (isoCode, { IsoCode = isoCode; Entries = countryEntries } )
    ) |> Map.ofSeq

let calculateMovingAverages
    daysOfMovingAverage
    (countryEntries: CountryDataDayEntry[]) =

    let entriesCount = countryEntries.Length
    let cutOff = daysOfMovingAverage / 2
    let averagesSetLength = entriesCount - cutOff * 2

    let averages: CountryDataDayEntry[] = Array.zeroCreate averagesSetLength

    let daysOfMovingAverageFloat = float daysOfMovingAverage
    let mutable currentNewCasesSum = 0.
    let mutable currentNewCases1MSum = 0.
    let mutable currentCasesSum = 0.
    let mutable currentCases1MSum = 0.
    let mutable currentDeathsSum = 0.
    let mutable currentDeaths1MSum = 0.

    let movingAverageFunc index =
        let entry = countryEntries.[index]

        currentNewCasesSum <- currentNewCasesSum + entry.NewCases
        currentNewCases1MSum <- currentNewCases1MSum + entry.NewCasesPerMillion
        currentCasesSum <- currentCasesSum + entry.TotalCases
        currentCases1MSum <- currentCases1MSum + entry.TotalCasesPerMillion
        currentDeathsSum <- currentDeathsSum + entry.TotalDeaths
        currentDeaths1MSum <- currentDeaths1MSum + entry.TotalDeathsPerMillion

        match index with
        | index when index >= daysOfMovingAverage - 1 ->
            let date = countryEntries.[index - cutOff].Date
            let newCasesAvg = currentNewCasesSum / daysOfMovingAverageFloat
            let newCases1MAvg = currentNewCases1MSum / daysOfMovingAverageFloat
            let casesAvg = currentCasesSum / daysOfMovingAverageFloat
            let cases1MAvg = currentCases1MSum / daysOfMovingAverageFloat
            let deathsAvg = currentDeathsSum / daysOfMovingAverageFloat
            let deaths1MAvg = currentDeaths1MSum / daysOfMovingAverageFloat

            averages.[index - (daysOfMovingAverage - 1)] <- {
                Date = date
                NewCases = newCasesAvg; NewCasesPerMillion = newCases1MAvg
                TotalCases = casesAvg; TotalCasesPerMillion = cases1MAvg
                TotalDeaths = deathsAvg; TotalDeathsPerMillion = deaths1MAvg
            }

            let entryToRemove =
                countryEntries.[index - (daysOfMovingAverage - 1)]
            currentNewCasesSum <-
                currentNewCasesSum - entryToRemove.NewCases
            currentNewCases1MSum <-
                currentNewCases1MSum - entryToRemove.NewCasesPerMillion
            currentCasesSum <- currentCasesSum - entryToRemove.TotalCases
            currentCases1MSum <-
                currentCases1MSum - entryToRemove.TotalCasesPerMillion
            currentDeathsSum <- currentDeathsSum - entryToRemove.TotalDeaths
            currentDeaths1MSum <-
                currentDeaths1MSum - entryToRemove.TotalDeathsPerMillion

        | _ -> ignore()

    for i in 0 .. entriesCount-1 do
        movingAverageFunc i

    averages


type XAxisType =
    | ByDate
    | DaysSinceFirstDeath
    | DaysSinceOneDeathPerMillion

type OwidDataState =
    | NotLoaded
    | PreviousAndLoadingNew of OurWorldInDataRemoteData
    | Current of OurWorldInDataRemoteData

let aggregateOurWorldInData
    xAxisType
    daysOfMovingAverage
    (owidDataState: OwidDataState)
    : CountriesData option =

    let filterEntries (entry: CountryDataDayEntry) =
        match xAxisType with
        | ByDate -> entry.TotalDeaths > 0.
        | DaysSinceFirstDeath -> entry.TotalDeaths >= 1.
        | DaysSinceOneDeathPerMillion -> entry.TotalDeathsPerMillion  >= 1.

    let doAggregate (owidData: OurWorldInDataRemoteData): CountriesData option =
        match owidData with
        | Success dataPoints ->
            let groupedByCountries: CountriesData =
                dataPoints |> groupEntriesByCountries

            let averagedAndFilteredByCountries: CountriesData  =
                groupedByCountries
                |> Map.map (fun _ (countryData: CountryData) ->
                    let averagedEntries =
                        countryData.Entries
                        |> calculateMovingAverages daysOfMovingAverage
                        |> Array.filter filterEntries
                    { countryData with Entries = averagedEntries })

            Some averagedAndFilteredByCountries
        | _ -> None

    match owidDataState with
    | PreviousAndLoadingNew owidData -> doAggregate owidData
    | Current owidData -> doAggregate owidData
    | NotLoaded -> None
