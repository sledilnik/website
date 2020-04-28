module CountriesChartViz.Analysis

open Statistics
open Data.OurWorldInData
open Types
open System

type IndexedDate = (int * DateTime)

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: SeriesValues<IndexedDate>
}

type CountriesData = CountryData[]

type StartingDayMode =
    | FirstDeath

type OwidDataState =
    | NotLoaded
    | PreviousAndLoadingNew of OurWorldInDataRemoteData
    | Current of OurWorldInDataRemoteData

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
    (owidDataState: OwidDataState)
    : CountriesData option =

    let filterRecords entry =
        match startingDayMode with
        | FirstDeath -> entry.TotalDeaths >= 1

    let countriesComparer a b =
        match a.CountryIsoCode, b.CountryIsoCode with
        | "SVN", _ -> -1
        | _, "SVN" -> 1
        | _ -> a.CountryName.CompareTo b.CountryName

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
                  CountryName = countryNames.[isoCode]
                  Data = dailyEntries }
                )
            |> Seq.sortWith countriesComparer
            |> Seq.toArray
            |> Some
        | _ -> None

    match owidDataState with
    | PreviousAndLoadingNew owidData -> doAggregate owidData
    | Current owidData -> doAggregate owidData
    | NotLoaded -> None
