[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Highcharts
open Types

type Metric =
    | PerformedTests
    | PerformedTestsToDate
    | ConfirmedCasesToday
    | ConfirmedCasesToDate
    | ConfirmedCases
    | RecoveredToDate
    | InHospital
    | InHospitalToDate
    | InICU
    | OnVentilator
    | OutOfHospital
    | OutOfHospitalToDate
    | Deceased
    | DeceasedToDate

type MetricCfg = {
    Metric: Metric
    Color : string
    Visible : bool
    Label : string
    Line : Highcharts.DashStyle
    Class: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric=PerformedTests;       Color="#19aebd"; Visible=false; Line=Solid; Label="Testiranja (na dan)"; Class="cs-tests" }
        { Metric=PerformedTestsToDate; Color="#73ccd5"; Visible=false; Line=Dot; Label="Testiranja (skupaj)"; Class="cs-testsToDate" }
        { Metric=ConfirmedCasesToday;  Color="#bda506"; Visible=true;  Line=Solid; Label="Potrjeno okuženi (na dan)"; Class="cs-positiveTests" }
        { Metric=ConfirmedCasesToDate; Color="#d5c768"; Visible=false; Line=Dot; Label="Potrjeno okuženi (skupaj)"; Class="cs-positiveTestsToDate" }
        { Metric=ConfirmedCases;       Color="#bda506"; Visible=false; Line=Dash; Label="Potrjeno okuženi (aktivni)"; Class="cs-positiveTestsActive" }
        { Metric=RecoveredToDate;      Color="#8cd4b2"; Visible=false; Line=Dash; Label="Preboleli (skupaj)"; Class="cs-recoveredToDate" }
        { Metric=InHospital;           Color="#be7A2a"; Visible=true;  Line=Solid; Label="Hospitalizirani (aktivni)"; Class="cs-inHospital" }
        { Metric=InHospitalToDate;     Color="#de9a5a"; Visible=false; Line=Dot; Label="Hospitalizirani (skupaj)"; Class="cs-inHospitalToDate" }
        { Metric=InICU;                Color="#d99a91"; Visible=true;  Line=Solid; Label="V intenzivni enoti (aktivni)"; Class="cs-inHospitalICU" }
        { Metric=OnVentilator;         Color="#bf5747"; Visible=false; Line=Solid; Label="Na respiratorju (aktivni)"; Class="cs-inHospitalVentilator" }
        { Metric=OutOfHospital;        Color="#20b16d"; Visible=false; Line=Solid; Label="Odpuščeni iz bolnišnice (na dan)"; Class="cs-outOfHospital" }
        { Metric=OutOfHospitalToDate;  Color="#57c491"; Visible=false; Line=Dot; Label="Odpuščeni iz bolnišnice (skupaj)"; Class="cs-outOfHospitalToDate" }
        { Metric=Deceased;             Color="#000000"; Visible=false; Line=Solid; Label="Umrli (na dan)"; Class="cs-deceased" }
        { Metric=DeceasedToDate;       Color="#666666"; Visible=true;  Line=Dot; Label="Umrli (skupaj)"; Class="cs-deceasedToDate" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
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
            | PerformedTests -> point.Tests.Positive.Today
            | PerformedTestsToDate -> point.Tests.Positive.ToDate
            | ConfirmedCasesToday -> point.Cases.ConfirmedToday
            | ConfirmedCasesToDate -> point.Cases.ConfirmedToDate
            | ConfirmedCases -> point.Cases.Active
            | RecoveredToDate -> point.Cases.RecoveredToDate
            | InHospital -> point.StatePerTreatment.InHospital
            | InHospitalToDate -> point.StatePerTreatment.InHospitalToDate
            | InICU -> point.StatePerTreatment.InICU
            | OnVentilator -> point.StatePerTreatment.Critical
            | OutOfHospital -> point.StatePerTreatment.OutOfHospital
            | OutOfHospitalToDate -> point.StatePerTreatment.OutOfHospitalToDate
            | Deceased -> point.StatePerTreatment.Deceased
            | DeceasedToDate -> point.StatePerTreatment.DeceasedToDate

    let allSeries = [
        let mutable startTime = DateTime.Today |> jsTime
        for metric in metrics do
            let pointData = metricDataGenerator metric
            yield pojo
                {|
                    visible = metric.Visible
                    color = metric.Color
                    name = metric.Label
                    //className = metric.Class
                    dashStyle = metric.Line |> DashStyle.toString
                    data =
                        data
                        |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime12h, pointData dp))
                        |> Seq.skipWhile (fun (ts,value) ->
                            if metric.Visible && value.IsSome then
                                startTime <- min startTime ts
                            value.IsNone)
                        |> Seq.toArray
                    //yAxis = 0 // axis index
                    //showInLegend = true
                    //fillOpacity = 0
                |}
        yield addContainmentMeasuresFlags startTime None |> pojo
    ]

    let baseOptions = basicChartOptions scaleType "covid19-metrics-comparison"
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = scaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
    |}

let renderChartContainer scaleType data metrics =
    Html.div [
        prop.style [ style.height 480 ] //; style.width 500; ]
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

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
