[<RequireQualifiedAccess>]
module AgeGroupsTimelineViz.Rendering

open DataAnalysis.AgeGroupsTimeline
open DataVisualization.ChartingTypes
open Synthesis
open Highcharts
open Types

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser.Types

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

type State = {
    Data : StatsData
    MetricType : MetricType
    ChartType : BarChartType
    RangeSelectionButtonIndex: int
}

type Msg =
    | MetricTypeChanged of MetricType
    | BarChartTypeChanged of BarChartType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        MetricType = MetricType.Default
        ChartType = AbsoluteChart
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | BarChartTypeChanged chartType ->
        { state with ChartType = chartType }, Cmd.none
    | MetricTypeChanged metricType ->
        { state with MetricType = metricType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =
    let allSeries =
        getAgeGroupTimelineAllSeriesData
            state.Data state.MetricType.Value
            (fun dataPoint -> dataPoint.StatePerAgeToDate)

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let className = "covid19-agegroups-timeline"
    let baseOptions =
        basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis = baseOptions.xAxis
        yAxis = baseOptions.yAxis

        plotOptions = pojo
            {|
                column = pojo
                        {|
                          dataGrouping = pojo {| enabled = false |}
                          groupPadding = 0
                          pointPadding = 0
                          borderWidth = 0 |}
                series =
                    match state.ChartType with
                    | AbsoluteChart -> pojo {| stacking = "normal" |}
                    | RelativeChart -> pojo {| stacking = "percent" |}
                    | _ -> invalidOp "not supported"
            |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        tooltip = pojo {|
                          formatter = fun () -> tooltipFormatter jsThis
                          shared = true
                          useHTML = true
                        |}
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


let renderMetricTypeSelectors (activeMetricType: MetricType) dispatch =
    let renderMetricTypeSelector (metricTypeToRender: MetricType) =
        let active = metricTypeToRender = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch metricTypeToRender)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (I18N.tt "charts.common" metricTypeToRender.Id)
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        MetricType.All
        |> Array.map renderMetricTypeSelector
        |> prop.children
    ]

let render state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderMetricTypeSelectors
                state.MetricType (MetricTypeChanged >> dispatch)
            Utils.renderBarChartTypeSelector
                state.ChartType (BarChartTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("AgeGroupsTimelineViz/Chart", init props.data, update, render)
