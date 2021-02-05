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
    Metrics : DisplayMetrics
    Data : StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        Metrics = DisplayMetrics.Default
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeMetrics metrics -> { state with Metrics=metrics }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =
    let allSeries =
        getAgeGroupTimelineAllSeriesData
            state.Data state.Metrics.ValueCalculation
            (fun dataPoint -> dataPoint.StatePerAgeToDate)

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
                    match state.Metrics.ChartType with
                    | StackedBarNormal -> pojo {| stacking = "normal" |}
                    | StackedBarPercent -> pojo {| stacking = "percent" |}
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

let renderMetricsSelectors activeMetrics dispatch =
    let renderSelector (metrics : DisplayMetrics) =
        let active = metrics = activeMetrics
        Html.div [
            prop.text (I18N.chartText "ageGroupsTimeline" metrics.Id)
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch metrics)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        DisplayMetrics.All
        |> Array.map renderSelector
        |> prop.children
    ]

let render state dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("AgeGroupsTimelineViz/Chart", init props.data, update, render)
