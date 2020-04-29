module CountriesChartViz.Analysis

open Statistics
open Data.OurWorldInData
open Types
open System

type IndexedDate = (int * DateTime)

type CountryData = {
    CountryIsoCode: CountryIsoCode
    Data: SeriesValues<IndexedDate>
}

type CountriesData = CountryData[]

type StartingDayMode =
    | FirstDeath
    | OneDeathPerMillion

type OwidDataState =
    | NotLoaded
    | PreviousAndLoadingNew of OurWorldInDataRemoteData
    | Current of OurWorldInDataRemoteData

let aggregateOurWorldInData
    startingDayMode
    daysOfMovingAverage
    (owidDataState: OwidDataState)
    : CountriesData option =

    let filterRecords entry =
        match startingDayMode with
        | FirstDeath -> entry.TotalDeaths >= 1
        | OneDeathPerMillion ->
            match entry.TotalDeathsPerMillion with
            | Some x -> x >= 1.
            | None -> false

    let doAggregate (owidData: OurWorldInDataRemoteData) =
        match owidData with
        | Success dataPoints ->
            dataPoints
            |> Seq.filter filterRecords
            |> Seq.map (fun entry ->
                let countryIsoCode = entry.CountryCode
                let dateStr = entry.Date
                let deathsPerMillion = entry.TotalDeathsPerMillion

                let date = DateTime.Parse(dateStr)

                (countryIsoCode, date, deathsPerMillion))
            |> Seq.sortBy (fun (isoCode, _, _) -> isoCode)
            |> Seq.groupBy (fun (isoCode, _, _) -> isoCode)
            |> Seq.map (fun (isoCode, countryLines) ->
                let dailyEntries =
                    countryLines
                    |> Seq.mapi(fun dayIndex (_, date, deathsPerMillion) ->
                        ((dayIndex, date), deathsPerMillion) )
                    |> Seq.toArray
                    |> (movingAverages
                            movingAverageCentered daysOfMovingAverage
                            (fun (day, _) -> day)
                            (fun (_, value) -> value |> Option.defaultValue 0.))
                { CountryIsoCode = isoCode
                  Data = dailyEntries }
                )
            |> Seq.toArray
            |> Some
        | _ -> None

    match owidDataState with
    | PreviousAndLoadingNew owidData -> doAggregate owidData
    | Current owidData -> doAggregate owidData
    | NotLoaded -> None
