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

let countryNames =
    [
        "AUT", "Avstrija"
        "BEL", "Belgija"
        "BIH", "Bosna in Hercegovina"
        "CZE", "Češka"
        "DEU", "Nemčija"
        "DNK", "Danska"
        "ESP", "Španija"
        "FIN", "Finska"
        "GBR", "Združeno kraljestvo"
        "HRV", "Hrvaška"
        "HUN", "Madžarska"
        "ISL", "Island"
        "ITA", "Italija"
        "MKD", "Severna Makedonija"
        "MNE", "Črna gora"
        "NOR", "Norveška"
        "RKS", "Kosovo"
        "SRB", "Srbija"
        "SVK", "Slovaška"
        "SVN", "Slovenija"
        "SWE", "Švedska"
        "SWZ", "Švica"
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

    /// <summary>
    /// Ensures Slovenia is über alles ;-).
    /// </summary>
    let countriesComparer (a, countryNameA: string) (b, countryNameB: string) =
        match a.CountryIsoCode, b.CountryIsoCode with
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
            |> Array.map (fun countryData ->
                (countryData, countryNames.[countryData.CountryIsoCode]))
            // sort by country names (but keep Slovenia at the top)
            |> Array.sortWith countriesComparer
            // assign colors to countries and transform into final records
            |> Array.mapi (fun countryIndex (countryData, countryName) ->
                    let colorIndex =
                        countryIndex * colorsInPalette / countriesCount
                    let color = ColorPalette.[colorIndex]

                    { CountryAbbr = countryData.CountryIsoCode
                      CountryName = countryName
                      Color = color
                      Data = countryData.Data }
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
