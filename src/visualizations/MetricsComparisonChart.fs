[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Fable.Core
open Elmish

open Feliz
open Feliz.ElmishComponents

open Types
open Recharts

// plain old javascript object
let inline pojo o = JsInterop.toPlainJsObj o

// plain old javascript object
[<Emit """Array.prototype.slice.call($0)""">]
let poja (a: 'T[]) : obj = jsNative


[<Emit("$0.getTime()")>]
let jsTime (x: DateTime): float = jsNative


type Metric =
    | Tests
    | TotalTests
    | PositiveTests
    | TotalPositiveTests
    | Hospitalized
    | HospitalizedToDate
    | HospitalizedIcu
    | OutOfHospital
    | OutOfHospitalToDate
    | RecoveredToDate
    | Deaths
    | TotalDeaths

type MetricCfg = {
    Metric: Metric
    Color : string
    Visible : bool
    Label : string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric = Tests;               Color = "#19aebd" ; Visible = false ; Label = "Testiranja" }
        { Metric = TotalTests;          Color = "#73ccd5" ; Visible = false ; Label = "Testiranja - skupaj" }
        { Metric = PositiveTests;       Color = "#bda506" ; Visible = false ; Label = "Potrjeno okuženi" }
        { Metric = TotalPositiveTests;  Color = "#d5c768" ; Visible = true  ; Label = "Potrjeno okuženi - skupaj" }
        { Metric = Hospitalized;        Color = "#be7A2a" ; Visible = true  ; Label = "Hospitalizirani" }
        { Metric = HospitalizedToDate;  Color = "#de9a5a" ; Visible = false ; Label = "Hospitalizirani - vsi" }
        { Metric = HospitalizedIcu;     Color = "#bf5747" ; Visible = true  ; Label = "Intenzivna nega" }
        { Metric = OutOfHospital;       Color = "#20b16d" ; Visible = false ; Label = "Odpuščeni iz bolnišnice" }
        { Metric = OutOfHospitalToDate; Color = "#57c491" ; Visible = false ; Label = "Odpuščeni iz bolnišnice - skupaj" }
        { Metric = RecoveredToDate;     Color = "#8cd4b2" ; Visible = true  ; Label = "Ozdraveli - skupaj" }
        { Metric = Deaths;              Color = "#000000" ; Visible = false ; Label = "Umrli" }
        { Metric = TotalDeaths;         Color = "#666666" ; Visible = true  ; Label = "Umrli - skupaj" }
    ]
    /// find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      Data : StatsData
      Metrics : Metrics }

type Msg =
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        Data = data
        Metrics = Metrics.initial
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleMetricVisible metric ->
        { state with
            Metrics = state.Metrics |> Metrics.update (fun mc -> { mc with Visible = not mc.Visible}) metric
        }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let renderChartConfig (scaleType: ScaleType) (data : StatsData) (metrics : Metrics) =

    let renderLineLabel (input: ILabelProperties) =
        Html.text [
            prop.x(input.x)
            prop.y(input.y)
            prop.fill color.black
            prop.textAnchor.middle
            prop.dy(-10)
            prop.fontSize 10
            prop.text input.value
        ]

    let maxOption a b =
        match a, b with
        | None, None -> None
        | Some x, None -> Some x
        | None, Some y -> Some y
        | Some x, Some y -> Some (max x y)

    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc =
        fun point ->
            match mc.Metric with
            | Tests -> maxOption point.Tests point.TestsAt14.Performed |> Utils.zeroToNone
            | TotalTests -> maxOption point.TotalTests point.TestsAt14.PerformedToDate |> Utils.zeroToNone
            | PositiveTests -> maxOption point.PositiveTests point.TestsAt14.Positive |> Utils.zeroToNone
            | TotalPositiveTests -> maxOption point.TotalPositiveTests point.TestsAt14.PositiveToDate |> Utils.zeroToNone
            | Hospitalized -> point.Hospitalized |> Utils.zeroToNone
            | HospitalizedToDate -> point.HospitalizedToDate |> Utils.zeroToNone
            | HospitalizedIcu -> point.HospitalizedIcu |> Utils.zeroToNone
            | OutOfHospital -> point.OutOfHospital |> Utils.zeroToNone
            | OutOfHospitalToDate -> point.OutOfHospitalToDate |> Utils.zeroToNone
            | RecoveredToDate -> point.RecoveredToDate |> Utils.zeroToNone
            | Deaths -> point.Deaths |> Utils.zeroToNone
            | TotalDeaths -> point.TotalDeaths |> Utils.zeroToNone


    let renderMetric (metric : MetricCfg) (dataKey : StatsDataPoint -> int option) =
        Recharts.line [
            line.name metric.Label
            line.monotone
            line.isAnimationActive false
            line.stroke metric.Color
            line.strokeWidth 2
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let series =
        metrics
        |> List.map (fun metric ->
            let pointData = metricDataGenerator metric
            {|
                visible = metric.Visible
                color = metric.Color
                name = metric.Label
                data = data |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime, pointData dp)) |> Seq.toArray
                //yAxis = 0 // axis index
                //showInLegend = true
                //fillOpacity = 0
            |}
            |> pojo
        )
        |> List.toArray
        |> poja

    let config =
        {|
            chart = pojo
                {|
                    //height = "100%"
                    ``type`` = "spline"
                    zoomType = "x"
                |}
            title = null //{| text = "Graf" |}
            xAxis = [|
                {|
                    index=0; crosshair=true; ``type``="datetime"
                    gridLineWidth=1 //; isX=true
                    tickInterval=86400000
                    //labels = {| rotation= -45 |}
                    plotLines=[|
                        {| value=jsTime (DateTime.Parse "2020-03-14"); label={|text=" Sprememba štetja"; zIndex=100; color="#ccc" |} |}
                    |]
                |}
            |]
            yAxis = [|
                {|
                    index = 0
                    ``type`` = if scaleType=Linear then "linear" else "logarithmic"
                    min = if scaleType=Linear then None else Some 1.0
                    opposite = true // right side
                    title = {| text = null |} // "oseb" |}
                    //showFirstLabel = false
                    tickInterval = if scaleType=Linear then None else Some 0.25
                |}
            |]
            legend = pojo
                {|
                    enabled = false
                    align = "left"
                    verticalAlign = "top"
                    borderColor = "#ddd"
                    borderWidth = 1
                    //labelFormatter = string //fun series -> series.name
                    layout = "vertical"
                |}
            plotOptions = pojo
                {|
                    spline = pojo
                        {|
                            dataLabels = pojo {| enabled = true |}
                            //enableMouseTracking = false
                        |}
                |}
            series = series
        |}
    JS.console.log ("highcharts config:", config)

    config

let renderChartContainer scaleType data metrics =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartConfig scaleType data metrics
            |> Highcharts.chart
        ]
    ]

let renderMetricSelector (metric : MetricCfg) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleMetricVisible metric.Metric |> dispatch)
        prop.className [ true, "btn  btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
        prop.style style
        prop.text metric.Label ]

let renderMetricsSelectors metrics dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for mc in metrics do
                yield renderMetricSelector mc dispatch
        ]
    ]

let render state dispatch =
    Html.div [
        Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state.ScaleType state.Data state.Metrics
        renderMetricsSelectors state.Metrics dispatch
    ]

type Props = {
    data : StatsData
}

let metricsComparisonChart (props : Props) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
