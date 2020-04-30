module CountriesChartViz.Synthesis

open CountriesChartViz.Analysis
open Fable.Core
open Highcharts
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

// source: https://en.wikipedia.org/wiki/ISO_3166-1_alpha-3
let countryNames =
    [
        "AUT", "Avstrija"
        "BEL", "Belgija"
        "BIH", "Bosna in Hercegovina"
        "CHE", "Švica"
        "CHN", "Kitajska"
        "CZE", "Češka"
        "DEU", "Nemčija"
        "DNK", "Danska"
        "ESP", "Španija"
        "FIN", "Finska"
        "FRA", "Francija"
        "GBR", "Združeno kraljestvo"
        "HRV", "Hrvaška"
        "HUN", "Madžarska"
        "IRN", "Iran"
        "ISL", "Islandija"
        "ITA", "Italija"
        "MKD", "Severna Makedonija"
        "MNE", "Črna gora"
        "NOR", "Norveška"
        "RKS", "Kosovo"
        "RUS", "Rusija"
        "SRB", "Srbija"
        "SVK", "Slovaška"
        "SVN", "Slovenija"
        "SWE", "Švedska"
        "TUR", "Turčija"
        "USA", "ZDA"
    ]
    |> List.map (fun (code, name) -> code,  name)
    |> Map.ofList

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

let Dashes = [ DashStyle.Solid; DashStyle.Dash ]

type CountrySeries = {
    CountryAbbr: string
    CountryName: string
    Color: string
    Entries: CountryDataDayEntry[]
}

type ChartData = {
    XAxisTitle: string
    YAxisTitle: string
    Series: CountrySeries[]
}

let legendFormatter jsThis =
    let countryCode = jsThis?series?name
    let date = jsThis?point?date
    let dataValue: float = jsThis?point?y

//        let x: obj[] = jsThis?points
//    x.[0]?series?name
//
//    let countryCode = x.[0]?series?name
//    let date = x.[0]?point?date
//    let dataValue: float = x.[0]?point?y

    sprintf
        "<b>%s</b><br/>%s<br/>Umrli na 1 milijon preb.: %A"
        countryCode
        date
        (Utils.roundTo1Decimal dataValue)

let prepareChartData
    startingDayMode
    daysOfMovingAverage
    (state: ChartState)
    : ChartData option =

    /// <summary>
    /// Ensures Slovenia is über alles ;-).
    /// </summary>
    let countriesComparer (a, countryNameA: string) (b, countryNameB: string) =
        match a.IsoCode, b.IsoCode with
        | "SVN", _ -> -1
        | _, "SVN" -> 1
        | _ -> countryNameA.CompareTo countryNameB

    let aggregated =
        state.OwidDataState
        |> aggregateOurWorldInData startingDayMode daysOfMovingAverage

    match aggregated with
    | Some aggregated ->
        let colorsInPalette = ColorPalette |> List.length
        let countriesCount = aggregated |> Seq.length

        let series =
            aggregated
            // assign country names
            |> Map.map (fun countryIsoCode countryData ->
                (countryData, countryNames.[countryIsoCode]))
            |> Map.toArray
            |> Array.map (fun (_, value) -> value)
            // sort by country names (but keep Slovenia at the top)
            |> Array.sortWith countriesComparer
            // assign colors to countries and transform into final records
            |> Array.mapi (fun countryIndex (countryData, countryName) ->
                    let colorIndex =
                        countryIndex * colorsInPalette / countriesCount
                    let color = ColorPalette.[colorIndex]

                    { CountryAbbr = countryData.IsoCode
                      CountryName = countryName
                      Color = color
                      Entries = countryData.Entries }
                )

        {
            Series = series
            XAxisTitle =
              match startingDayMode with
                | FirstDeath -> "Št. dni od prvega smrtnega primera"
                | OneDeathPerMillion ->
                    "Št. dni od vrednosti 1 umrlega na 1 milijon prebivalcev"
            YAxisTitle = "Št. umrlih na 1 milijon prebivalcev"
        }
        |> Some
    | None -> None
