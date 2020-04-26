module CountriesChartViz.Analysis

open Types
open Statistics
open Data.OurWorldInData
open System

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: SeriesValues<DateTime>
}

type CountriesData = Map<CountryIsoCode, CountryData>

type CountriesSelection =
    | Scandinavia

let aggregateOurWorldInData
    daysOfMovingAverage
    (ourWorldInData: OurWorldInDataRemoteData)
    : CountriesData option =

    match ourWorldInData with
    | Success ourWorldInData ->
        let countriesData =
            ourWorldInData
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
                    |> Seq.map(fun (_, _, date, deathsPerMillion) ->
                        (date, deathsPerMillion) )
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
