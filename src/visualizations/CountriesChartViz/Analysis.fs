module CountriesChartViz.Analysis

open Types
open Statistics
open Data.OurWorldInData
open System

type IndexedDate = (int * DateTime)

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: SeriesValues<IndexedDate>
}

type CountriesData = Map<CountryIsoCode, CountryData>

let aggregateOurWorldInData
    filterRecords
    daysOfMovingAverage
    (ourWorldInData: OurWorldInDataRemoteData)
    : CountriesData option =

    match ourWorldInData with
    | Success ourWorldInData ->
        let countriesData =
            ourWorldInData
            |> Seq.filter filterRecords
            |> Seq.map (fun entry ->
                let countryIsoCode = entry.CountryCode
                let countryName = entry.CountryName
                let dateStr = entry.Date
                let deathsPerMillion = entry.TotalDeathsPerMillion

                let date = DateTime.Parse(dateStr)

                (countryIsoCode, countryName, date, deathsPerMillion))
            |> Seq.sortBy (fun (isoCode, _, _, _) -> isoCode)
            |> Seq.groupBy (fun (isoCode, countryName, _, _)
                             -> (isoCode, countryName))
            |> Seq.map (fun ((isoCode, countryName), countryLines) ->
                let dailyEntries =
                    countryLines
                    |> Seq.mapi(fun dayIndex (_, _, date, deathsPerMillion) ->
                        ((dayIndex, date), deathsPerMillion) )
                    |> Seq.toArray
                    |> (movingAverages
                            movingAverageCentered daysOfMovingAverage
                            (fun (day, _) -> day)
                            (fun (_, value) -> value |> Option.defaultValue 0.))
                { CountryIsoCode = isoCode
                  CountryName = countryName
                  Data = dailyEntries }
                )

        countriesData
        |> Seq.map (fun x -> (x.CountryIsoCode, x))
        |> Map.ofSeq
        |> Some
    | _ -> None
