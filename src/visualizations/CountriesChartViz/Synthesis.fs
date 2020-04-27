module CountriesChartViz.Synthesis

open System
open CountriesChartViz.Analysis
open Fable.Core
open Statistics
open JsInterop

type ChartState = {
    Data: Data.OurWorldInData.OurWorldInDataRemoteData
    DisplayedCountriesSet: int
}

let ColorPalette =
    [ "#ffa600"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#f95d6a"
      "#d45087"
      "#a05195"
      "#665191"
      "#10829a"
      "#024a66" ]

type CountrySeries = {
    CountryAbbr: string
    CountryName: string
    Color: string
    Data: SeriesValues<DateTime>
}

type ChartData = CountrySeries[]

let legendFormatter jsThis =
    let countryCode = jsThis?series?name
//                let date = jsThis?point?category
    let dataValue: float = jsThis?point?y

    sprintf
        "<b>%s</b><br/>Umrli na 1 milijon preb.: %A"
        countryCode
        (Utils.roundTo1Decimal dataValue)


let prepareChartData
    daysOfMovingAverage
    (state: ChartState)
    : ChartData option =

    let aggregated =
        state.Data
        |> aggregateOurWorldInData
               (fun entry -> entry.TotalDeaths >= 5)
               daysOfMovingAverage

    match aggregated with
    | Some aggregated ->
        let colorsInPalette = ColorPalette |> List.length
        let countriesCount = aggregated |> Seq.length

        aggregated
        |> Map.toArray
        |> Array.mapi (fun countryIndex (_, countryData) ->
                let colorIndex =
                    countryIndex * colorsInPalette / countriesCount
                let color = ColorPalette.[colorIndex]

                { CountryAbbr = countryData.CountryIsoCode
                  CountryName = countryData.CountryName
                  Color = color
                  Data = countryData.Data }
            )
        |> Array.sortBy (fun x -> x.CountryAbbr)
        |> Some
    | None -> None
