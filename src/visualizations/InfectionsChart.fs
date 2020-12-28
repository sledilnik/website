[<RequireQualifiedAccess>]
module InfectionsChart

open DataVisualization.ChartingTypes
open Statistics
open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

let chartText = I18N.chartText "infections"

type Metric =
    | HospitalStaff
    | RestHomeStaff
    | RestHomeOccupant
    | OtherPeople
    | AllConfirmed

type MetricCfg = {
    Metric : Metric
    Color : string
    Id : string
}

type Metrics = MetricCfg list

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

type ShowAllOrOthers = ShowAllConfirmed | ShowOthers

module Metrics  =
    let All = [
        { Metric=AllConfirmed;      Color="#bda506"; Id="allConfirmed" }
        { Metric=OtherPeople;       Color="#FFDBA3"; Id="otherPersons" }
        { Metric=HospitalStaff;     Color="#73ccd5"; Id="hcStaff" }
        { Metric=RestHomeStaff;     Color="#20b16d"; Id="rhStaff" }
        { Metric=RestHomeOccupant;  Color="#bf5747"; Id="rhOccupant" }
    ]

    let metricsToDisplay filter =
        let without metricType =
            All |> List.filter (fun metric -> metric.Metric <> metricType)

        match filter with
        | ShowAllConfirmed -> without OtherPeople
        | ShowOthers -> without AllConfirmed

type ValueTypes = RunningTotals | MovingAverages

type DisplayType = {
    Id: string
    ValueTypes: ValueTypes
    ShowAllOrOthers: ShowAllOrOthers
    ChartType: ChartType
    ShowPhases: bool
} with
    static member All =
        [|
            {   Id = "averageByDay"
                ValueTypes = MovingAverages
                ShowAllOrOthers = ShowAllConfirmed
                ChartType = SplineChart
                ShowPhases = true
            }
            {   Id = "all";
                ValueTypes = RunningTotals
                ShowAllOrOthers = ShowOthers
                ChartType = StackedBarNormal
                ShowPhases = false
            }
            {   Id = "relative";
                ValueTypes = RunningTotals
                ShowAllOrOthers = ShowOthers
                ChartType = StackedBarPercent
                ShowPhases = false
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
        RangeSelectionButtonIndex = 0
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
            | HospitalStaff -> fun pt -> pt.HospitalEmployeePositiveTestsToDate
            | RestHomeStaff -> fun pt -> pt.RestHomeEmployeePositiveTestsToDate
            | RestHomeOccupant -> fun pt -> pt.RestHomeOccupantPositiveTestsToDate
            | OtherPeople -> fun pt -> pt.UnclassifiedPositiveTestsToDate
            | AllConfirmed -> fun pt -> pt.Cases.ConfirmedToDate

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
            Metrics.metricsToDisplay state.DisplayType.ShowAllOrOthers
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

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` =
                    match state.DisplayType.ChartType with
                    | SplineChart -> "spline"
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
        yAxis =     // need to hide negative label for addContainmentMeasuresFlags
            let showFirstLabel = not state.DisplayType.ShowPhases
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})

        plotOptions = pojo
            {|
                series =
                    match state.DisplayType.ChartType with
                    | SplineChart -> pojo {| stacking = ""; |}
                    | StackedBarNormal -> pojo {| stacking = "normal" |}
                    | StackedBarPercent -> pojo {| stacking = "percent" |}
            |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
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

        Html.div [
            prop.className "disclaimer"
            prop.children [
                Html.text (chartText "disclaimer")
            ]
        ]
    ]


let infectionsChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
