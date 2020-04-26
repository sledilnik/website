module CountriesChartViz.Synthesis

open System
open CountriesChartViz.Analysis

type ChartState = {
    Data: CountriesData
    OurWorldInData: Data.OurWorldInData.Data
    DisplayedCountries: CountriesSelection
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

let prepareChartData (state: ChartState): ChartData =
    let colorsInPalette = ColorPalette |> List.length
    let countriesCount = state.Data |> Seq.length

    state.Data
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
