module CountriesChartViz.Analysis

open Types
open Statistics
open Data.OurWorldInData
open System

type CountriesDisplaySet = {
    Label: string
    CountriesCodes: string[]
}

// source: https://unstats.un.org/unsd/tradekb/knowledgebase/country-code
let countriesDisplaySets = [|
    { Label = "Nordijske države"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
    }
    { Label = "Ex-Yugoslavia"
      CountriesCodes = [| "BIH"; "HRV"; "MKD"; "MNE"; "RKS"; "SRB" |]
    }
    { Label = "Sosedje"
      CountriesCodes = [| "AUT"; "HRV"; "HUN"; "ITA" |]
    }
|]

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: SeriesValues<DateTime>
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
