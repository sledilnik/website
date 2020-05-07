module CountriesChartViz.Synthesis

open System.Text
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
    XAxisType: XAxisType
    ScaleType: ScaleType
}

// source: https://en.wikipedia.org/wiki/ISO_3166-1_alpha-3
let countryNames =
    [
        "AUS", "Avstralija"
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
        "JPN", "Japonska"
        "KOR", "J. Koreja"
        "MKD", "S. Makedonija"
        "MNE", "Črna gora"
        "NZL", "Nova Zelandija"
        "NOR", "Norveška"
        "RKS", "Kosovo"
        "RUS", "Rusija"
        "SGP", "Singapur"
        "SRB", "Srbija"
        "SVK", "Slovaška"
        "SVN", "Slovenija"
        "SWE", "Švedska"
        "TWN", "Tajvan"
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
    DataDescription: string
    XAxisTitle: string
    YAxisTitle: string
    Series: CountrySeries[]
}

let tooltipFormatter state chartData jsThis =
    let points: obj[] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        let dataDescription =
            sprintf "<b>%s:</b>" chartData.DataDescription

        let s = StringBuilder()
        s.Append dataDescription |> ignore
        s.Append "<br/>" |> ignore

        match state.XAxisType with
        | ByDate ->
            let date = points.[0]?point?date
            s.AppendFormat ("{0}<br/>", date.ToString()) |> ignore
        | _ -> ignore()

        s.Append "<table>" |> ignore

        points
        |> Array.sortByDescending
               (fun country ->
                    let dataValue: float = country?point?y
                    dataValue)
        |> Array.iter
               (fun country ->
                    let countryName = country?series?name
                    let date = country?point?date
                    let dataValue: float = country?point?y

                    s.Append "<tr>" |> ignore
                    let countryTooltip =
                        match state.XAxisType with
                        | ByDate ->
                            sprintf
                                "<td>%s</td><td style='text-align: right; padding-left: 10px'>%A</td>"
                                countryName
                                (Utils.formatTo1DecimalWithTrailingZero dataValue)
                        | _ ->
                            sprintf
                                "<td>%s</td><td style='padding-left: 10px'>%s</td><td style='text-align: right; padding-left: 10px'>%A</td>"
                                countryName
                                date
                                (Utils.formatTo1DecimalWithTrailingZero dataValue)
                    s.Append countryTooltip |> ignore
                    s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()

let prepareChartData
    xAxisType
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
        |> aggregateOurWorldInData xAxisType daysOfMovingAverage

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
            DataDescription = "Umrli na 1 milijon prebivalcev"
            XAxisTitle =
                match xAxisType with
                | ByDate -> ""
                | DaysSinceFirstDeath -> "Št. dni od prvega smrtnega primera"
                | DaysSinceOneDeathPerMillion ->
                    "Št. dni od vrednosti 1 umrlega na 1 milijon prebivalcev"
            YAxisTitle = "Št. umrlih na 1 milijon prebivalcev"
        }
        |> Some
    | None -> None
