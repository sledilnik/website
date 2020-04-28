module CountriesChartViz.Synthesis

open CountriesChartViz.Analysis
open Fable.Core
open Statistics
open JsInterop
open Types

type CountriesDisplaySet = {
    Label: string
    CountriesCodes: string[]
}

type ChartState = {
    OwidDataState: OwidDataState
    DisplayedCountriesSet: CountriesDisplaySet
    ScaleType : ScaleType
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
    Data: SeriesValues<IndexedDate>
}

type ChartData = {
    XAxisTitle: string
    YAxisTitle: string
    Series: CountrySeries[]
}

let legendFormatter jsThis =
    let countryCode = jsThis?series?name
//                let date = jsThis?point?category
    let dataValue: float = jsThis?point?y

    sprintf
        "<b>%s</b><br/>Umrli na 1 milijon preb.: %A"
        countryCode
        (Utils.roundTo1Decimal dataValue)


let prepareChartData
    startingDayMode
    daysOfMovingAverage
    (state: ChartState)
    : ChartData option =

    let aggregated =
        state.OwidDataState
        |> aggregateOurWorldInData FirstDeath daysOfMovingAverage

    match aggregated with
    | Some aggregated ->
        let colorsInPalette = ColorPalette |> List.length
        let countriesCount = aggregated |> Seq.length

        let series =
            aggregated
            |> Array.mapi (fun countryIndex countryData ->
                    let colorIndex =
                        countryIndex * colorsInPalette / countriesCount
                    let color = ColorPalette.[colorIndex]

                    { CountryAbbr = countryData.CountryIsoCode
                      CountryName = countryData.CountryName
                      Color = color
                      Data = countryData.Data }
                )

        {
            Series = series
            XAxisTitle =
              match startingDayMode with
                | FirstDeath -> "Št. dni od prvega smrtnega primera"
            YAxisTitle = "Št. umrlih na 1 milijon prebivalcev"
        }
        |> Some
    | None -> None
