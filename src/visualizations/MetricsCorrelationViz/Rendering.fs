module MetricsCorrelationViz.Rendering

open DataVisualization.ChartingTypes
open Statistics
open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

let chartText = I18N.chartText "metricsCorrelation"

type Metric =
    | Cases
    | Hospitalized
    | Deceased

type MetricCfg = {
    Metric : Metric
    Color : string
    Id : string
    YAxisIndex: int
}

type Metrics = MetricCfg list

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

module Metrics  =
    let all = [
        { Metric=Cases; Color="#bda506";Id="cases"; YAxisIndex = 0 }
        { Metric=Hospitalized; Color="#be7A2a"
          Id="hospitalized"; YAxisIndex = 1 }
        { Metric=Deceased; Color="#8C71A8"; Id="deceased"; YAxisIndex = 2 }
    ]

type ValueTypes = RunningTotals | MovingAverages

type DisplayType = {
    Id: string
    ValueTypes: ValueTypes
    ChartType: ChartType
    ShowPhases: bool
} with
    static member All = [|
        {   Id = "averageByDay"
            ValueTypes = MovingAverages
            ChartType = LineChart
            ShowPhases = true
        }
        {   Id = "all";
            ValueTypes = RunningTotals
            ChartType = LineChart
            ShowPhases = true
        }
    |]
    static member Default = DisplayType.All.[0]

[<Literal>]
let DaysOfMovingAverage = 7

type State = {
    DisplayType : DisplayType
    Data : StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        DisplayType = DisplayType.Default
        RangeSelectionButtonIndex = 1
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with DisplayType=rt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =

    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc : (StatsDataPoint -> int option) =
        let metricFunc =
            match mc.Metric with
            | Cases -> fun pt -> pt.Cases.ConfirmedToDate
            | Hospitalized -> fun pt -> pt.StatePerTreatment.InHospitalToDate
            | Deceased -> fun pt -> pt.StatePerTreatment.DeceasedToDate

        fun pt -> (pt |> metricFunc |> Utils.zeroToNone)

    /// <summary>
    /// Calculates running totals for a given metric.
    /// </summary>
    let calcRunningTotals metric =
        let pointData = metricDataGenerator metric

        let skipLeadingMissing data =
            data |> List.skipWhile (fun (_,value: 'T option) -> value.IsNone)

        let skipTrailingMissing data =
            data
            |> List.rev
            |> skipLeadingMissing
            |> List.rev

        state.Data
        |> List.map (fun dp -> ((xAxisPoint dp |> jsTime12h), pointData dp))
        |> skipLeadingMissing
        |> skipTrailingMissing
        |> Seq.toArray

    /// <summary>
    /// Converts running total series to daily (delta) values.
    /// </summary>
    let toDailyValues (series: DayValueIntMaybe[]) =
        let mutable last = 0
        Array.init series.Length (fun i ->
            match series.[i] with
            | ts, None -> ts, None
            | ts, Some current ->
                let result = current - last
                last <- current
                ts, Some result
        )

    let toFloatValues (series: DayValueIntMaybe[]) =
        series
        |> Array.map (fun (date, value) ->
            (date, value |> Option.defaultValue 0 |> float))

    let allSeries = [
        let allMetricsData =
            Metrics.all
            |> Seq.map(fun metric ->
                let data =
                    let runningTotals = calcRunningTotals metric
                    match state.DisplayType.ValueTypes with
                    | RunningTotals -> runningTotals |> toFloatValues
                    | MovingAverages ->
                        runningTotals |> toDailyValues
                        |> (movingAverages
                                movingAverageCentered
                                DaysOfMovingAverage
                                (fun (timestamp, _) -> timestamp)
                                (fun (_, value) ->
                                    value |> Option.defaultValue 0 |> float)
                            )
                        |> roundKeyValueFloatArray 1

                (metric, data))

        for (metric, metricData) in allMetricsData do
            yield pojo
                {|
                visible = true
                yAxis = metric.YAxisIndex
                color = metric.Color
                name = chartText metric.Id
                data = metricData
                marker = pojo {| enabled = false |}
                |}

        let allDates =
            allMetricsData
            |> Seq.map (fun (_, metricData) ->
                metricData |> Seq.map (fun (date, _) -> date))
            |> Seq.concat
        let startDate = allDates |> Seq.min
        let endDate = allDates |> Seq.max |> Some

        if state.DisplayType.ShowPhases then
            yield addContainmentMeasuresFlags startDate endDate |> pojo
    ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let className = "covid19-infections"
    let baseOptions =
        basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let axisWithPhases() = baseOptions.xAxis

    let axisWithWithoutPhases() =
        baseOptions.xAxis
        |> Array.map (fun ax ->
        {| ax with
            plotBands = shadedWeekendPlotBands
            plotLines = [||]
        |})

    let casesYAxis = {|
        gridLineWidth = 0
        title = pojo {| text = chartText Metrics.all.[0].Id |}
        index = 0
        opposite = false
    |}

    let hospitalizedYAxis = {|
        gridLineWidth = 0
        title = pojo {| text = chartText Metrics.all.[1].Id |}
        index = 1
        opposite = true
    |}

    let deceasedYAxis = {|
        gridLineWidth = 0
        title = pojo {| text = chartText Metrics.all.[2].Id |}
        index = 2
        opposite = true
    |}

    let yAxes = [| casesYAxis; hospitalizedYAxis; deceasedYAxis |]

    let yAxesResponsive =
        [|
            {| visible = false |}
            {| visible = false |}
            {| visible = false |}
        |]

    let responsive =
        {| rules =
           [| {|
                 condition = {| maxWidth = 1000 |}
                 chartOptions = {| yAxis = yAxesResponsive |}
           |} |] |}

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` =
                    match state.DisplayType.ChartType with
                    | LineChart -> "line"
                    | StackedBarNormal -> "column"
                    | StackedBarPercent -> "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        title = pojo {| text = None |}
        series = List.toArray allSeries
        xAxis =
            if state.DisplayType.ShowPhases then axisWithPhases()
            else axisWithWithoutPhases()
        yAxis = yAxes

        plotOptions = pojo
            {|
                series =
                    match state.DisplayType.ChartType with
                    | LineChart -> pojo {| stacking = ""; |}
                    | StackedBarNormal -> pojo {| stacking = "normal" |}
                    | StackedBarPercent -> pojo {| stacking = "percent" |}
            |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        responsive = responsive
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> chartFromWindow
        ]
    ]

let renderDisplaySelectors activeDisplayType dispatch =
    let renderSelector (displayType : DisplayType) =
        let active = displayType = activeDisplayType
        Html.div [
            prop.text (chartText displayType.Id)
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch displayType)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        DisplayType.All
        |> Array.map renderSelector
        |> prop.children
    ]


let halfDaysOfMovingAverage = DaysOfMovingAverage / 2

// TODO: the last date of the graph should be determined from the actual data
// because the graph cuts trailing days without any data. This requires some
// bit of refactoring of the code, to first prepare the data and only then
// render everything. Also the series arrays should work with native DateTime
// so it can be used in arithmetic calculations, instead of JsTimestamp (it should be
// transformed to JsTimestamp at the moment of setting the Highcharts objects).
let lastDateOfGraph =
    DateTime.Now.AddDays(-(halfDaysOfMovingAverage + 1) |> float)


let render state dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)
    ]


let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
