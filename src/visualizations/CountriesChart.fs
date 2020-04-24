[<RequireQualifiedAccess>]
module CountriesChart

open Browser
open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Highcharts
open Types

type DailyData = {
    Day: DateTime
    TotalDeathsPerMillion: float
}

type CountryIsoCode = string

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: DailyData[]
}

type CountriesData = Map<CountryIsoCode, CountryData>

type CountriesSelection =
    | Scandinavia

type State = {
    Data: CountriesData
    DisplayedCountries: CountriesSelection
}

/// <summary>
/// A function that calculates the moving average value for a given array of
/// day values.
/// </summary>
type MovingAverageFunc = (DailyData[]) -> DailyData

/// <summary>
/// Calculates the centered moving average for a given array of day values.
/// </summary>
/// <remarks>
/// The centered moving average takes the day that is at the center of the
/// values array as the target day of the average.
/// </remarks>
let movingAverageCentered: MovingAverageFunc = fun (daysValues) ->
    match (daysValues |> Seq.length) % 2 with
    | 1 ->
        let centerIndex = (daysValues |> Seq.length) / 2
        let targetDate = daysValues.[centerIndex].Day
        let averageValue =
            daysValues
            |> Seq.averageBy(
                fun dayValue -> dayValue.TotalDeathsPerMillion)
        { Day = targetDate; TotalDeathsPerMillion = averageValue }
    | _ -> ArgumentException "daysValues needs to be an odd number" |> raise

/// <summary>
/// Calculates the moving averages array for a given array of day values.
/// </summary>
let movingAverages
    (averageFunc: MovingAverageFunc)
    (daysOfMovingAverage: int)
    (series: DailyData[]): DailyData[] =
    series
    |> Array.windowed daysOfMovingAverage
    |> Array.map averageFunc

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

[<Literal>]
let DaysOfMovingAverage = 5

type Msg =
    | ChangeCountriesSelection of CountriesSelection

let parseCountriesCsv(): CountriesData =

    let countriesData =
        Data.CountriesData.countriesRawData
        |> Seq.map (fun entry ->
            let (countryIsoCode, countryName, dateStr, deathsPerMillion)
                = entry

            let date = DateTime.Parse(dateStr)

            (countryIsoCode, countryName, date, deathsPerMillion))
        |> Seq.sortBy (fun (isoCode, _, _, _) -> isoCode)
        |> Seq.groupBy (fun (isoCode, countryName, _, _)
                         -> (isoCode, countryName))
        |> Seq.map (fun ((isoCode, countryName), countryLines) ->
            let dailyEntries =
                countryLines
                |> Seq.map(fun (_, _, date, deathsPerMillion) ->
                    { Day = date; TotalDeathsPerMillion = deathsPerMillion })
                |> Seq.toArray
                |> (movingAverages movingAverageCentered DaysOfMovingAverage)
            { CountryIsoCode = isoCode
              CountryName = countryName
              Data = dailyEntries }
            )

    countriesData
    |> Seq.map (fun x -> (x.CountryIsoCode, x))
    |> Map.ofSeq

type CountrySeries = {
    CountryAbbr: string
    CountryName: string
    Color: string
    Data: DailyData[]
}

type ChartData = CountrySeries[]

let prepareChartData (state: State): ChartData =
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

let init data : State * Cmd<Msg> =
    let state = {
        Data = parseCountriesCsv()
        DisplayedCountries = Scandinavia
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeCountriesSelection countries ->
        { state with DisplayedCountries=countries }, Cmd.none

let renderChartCode (state: State) (chartData: ChartData) =
    let myLoadEvent(name: String) =
        let ret(event: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let allSeries =
        chartData
        |> Array.map (fun countrySeries ->
             pojo
                {|
                visible = true
                color = countrySeries.Color
                name = countrySeries.CountryAbbr
                data =
                    countrySeries.Data
                    |> Array.map (fun entry ->
                        ((entry.Day |> jsTime12h), entry.TotalDeathsPerMillion))
                marker = pojo {| enabled = false |}
                |}
            )

    let legend =
        {|
            enabled = true
            title = ""
            align = "left"
            verticalAlign = "top"
            borderColor = "#ddd"
            borderWidth = 1
            //labelFormatter = string //fun series -> series.name
            layout = "vertical"
            floating = true
            x = 20
            y = 30
            backgroundColor = "rgba(255,255,255,0.5)"
            reversed = true
        |}

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("countries") |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis = baseOptions.xAxis |> Array.map (fun ax ->
            {| ax with
                plotBands = shadedWeekendPlotBands
                plotLines = [||]
            |})
        plotOptions = pojo
            {|
                series = pojo {| stacking = ""; |}
            |}
        legend = pojo {| legend with enabled = true |}
        tooltip = pojo
           {| formatter = fun () ->
                let countryCode = jsThis?series?name
//                let date = jsThis?point?category
                let dataValue: float = jsThis?point?y

                sprintf
                    "<b>%s</b><br/>Št. umrlih na 1 milijon prebivalcev: %A"
                    countryCode
                    (Utils.roundTo1Decimal dataValue)
           |}
    |}


let renderChartContainer state chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartCode state chartData
            |> chart
        ]
    ]

let render state dispatch =
    let chartData = prepareChartData state

    Html.div [
        renderChartContainer state chartData
//        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)

        Html.div [
            prop.className "disclaimer"
        ]
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("CountriesChart", init props.data, update, render)
