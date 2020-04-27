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

type StartingDayMode =
    | FirstDeath

let countryNames =
    [
        "AUT", "Avstrija"
        "BIH", "Bosna in Hercegovina"
        "DNK", "Danska"
        "FIN", "Finska"
        "HRV", "Hrvaška"
        "HUN", "Madžarska"
        "ISL", "Island"
        "ITA", "Italija"
        "MKD", "Severna Makedonija"
        "MNE", "Črna gora"
        "NOR", "Norveška"
        "RKS", "Kosovo"
        "SRB", "Srbija"
        "SVN", "Slovenija"
        "SWE", "Švedska" ]
    |> List.map (fun (code, name) -> code,  name)
    |> Map.ofList

let aggregateOurWorldInData
    startingDayMode
    daysOfMovingAverage
    (ourWorldInData: OurWorldInDataRemoteData)
    : CountriesData option =

    let filterRecords entry =
        match startingDayMode with
        | FirstDeath -> entry.TotalDeaths >= 1

    match ourWorldInData with
    | Success ourWorldInData ->
        let countriesData =
            ourWorldInData
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
                  CountryName = countryNames.[isoCode]
                  Data = dailyEntries }
                )

        countriesData
        |> Seq.map (fun x -> (x.CountryIsoCode, x))
        |> Map.ofSeq
        |> Some
    | _ -> None
