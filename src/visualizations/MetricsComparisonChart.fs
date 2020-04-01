[<RequireQualifiedAccess>]
module MetricsComparisonChart

open Elmish

open Feliz
open Feliz.ElmishComponents
open Highcharts

open Types

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
    Class: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric = Tests;               Color = "#19aebd" ; Visible = false ; Label = "Testiranja"; Class="cs-tests" }
        { Metric = TotalTests;          Color = "#73ccd5" ; Visible = false ; Label = "Testiranja - skupaj"; Class="cs-testsToDate" }
        { Metric = PositiveTests;       Color = "#bda506" ; Visible = true ; Label = "Potrjeno okuženi"; Class="cs-positiveTests" }
        { Metric = TotalPositiveTests;  Color = "#d5c768" ; Visible = false  ; Label = "Potrjeno okuženi - skupaj"; Class="cs-positiveTestsToDate" }
        { Metric = Hospitalized;        Color = "#be7A2a" ; Visible = true  ; Label = "Hospitalizirani"; Class="cs-inHospital" }
        { Metric = HospitalizedToDate;  Color = "#de9a5a" ; Visible = false ; Label = "Hospitalizirani - skupaj"; Class="cs-inHospitalToDate" }
        { Metric = HospitalizedIcu;     Color = "#bf5747" ; Visible = true  ; Label = "V intenzivni enoti"; Class="cs-inHospitalICU" }
        { Metric = OutOfHospital;       Color = "#20b16d" ; Visible = false ; Label = "Odpuščeni iz bolnišnice"; Class="cs-outOfHospital" }
        { Metric = OutOfHospitalToDate; Color = "#57c491" ; Visible = false ; Label = "Odpuščeni iz bolnišnice - skupaj"; Class="cs-outOfHospitalToDate" }
        { Metric = RecoveredToDate;     Color = "#8cd4b2" ; Visible = true  ; Label = "Ozdraveli - skupaj"; Class="cs-recoveredToDate" }
        { Metric = Deaths;              Color = "#000000" ; Visible = false ; Label = "Umrli"; Class="cs-deceased" }
        { Metric = TotalDeaths;         Color = "#666666" ; Visible = true  ; Label = "Umrli - skupaj"; Class="cs-deceasedToDate" }
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

let renderChartOptions (scaleType: ScaleType) (data : StatsData) (metrics : Metrics) =

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
            | Tests -> point.PerformedTests |> Utils.zeroToNone
            | TotalTests -> point.PerformedTestsToDate |> Utils.zeroToNone
            | PositiveTests -> point.PositiveTests |> Utils.zeroToNone
            | TotalPositiveTests -> point.PositiveTestsToDate |> Utils.zeroToNone
            | Hospitalized -> point.Hospitalized |> Utils.zeroToNone
            | HospitalizedToDate -> point.HospitalizedToDate |> Utils.zeroToNone
            | HospitalizedIcu -> point.HospitalizedIcu |> Utils.zeroToNone
            | OutOfHospital -> point.OutOfHospital |> Utils.zeroToNone
            | OutOfHospitalToDate -> point.OutOfHospitalToDate |> Utils.zeroToNone
            | RecoveredToDate -> point.RecoveredToDate |> Utils.zeroToNone
            | Deaths -> point.Deaths |> Utils.zeroToNone
            | TotalDeaths -> point.TotalDeaths |> Utils.zeroToNone

    let allSeries =
        metrics
        |> List.map (fun metric ->
            let pointData = metricDataGenerator metric
            {|
                visible = metric.Visible
                color = metric.Color
                name = metric.Label
                className = metric.Class
                data =
                    data
                    |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime, pointData dp))
                    |> Seq.skipWhile (fun (ts,value) -> value.IsNone)
                    |> Seq.toArray
                //yAxis = 0 // axis index
                //showInLegend = true
                //fillOpacity = 0
            |}
            |> pojo
        )
        |> List.toArray

    // return highcharts options
    {| basicChartOptions scaleType "covid19-metrics-comparison" with series = allSeries |}

let renderChartContainer scaleType data metrics =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions scaleType data metrics
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
