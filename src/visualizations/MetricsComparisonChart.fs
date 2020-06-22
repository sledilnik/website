[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

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
    Line : Highcharts.DashStyle
    Id: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric=PerformedTests;       Color="#19aebd"; Visible=false; Line=Solid;  Id="tests" }
        { Metric=PerformedTestsToDate; Color="#73ccd5"; Visible=false; Line=Dot;    Id="testsToDate" }
        { Metric=ConfirmedCasesToday;  Color="#bda506"; Visible=true;  Line=Solid;  Id="confirmed" }
        { Metric=ConfirmedCasesToDate; Color="#d5c768"; Visible=false; Line=Dot;    Id="confirmedToDate" }
        { Metric=ConfirmedCases;       Color="#d5c768"; Visible=false; Line=Dash;   Id="active" }
        { Metric=RecoveredToDate;      Color="#8cd4b2"; Visible=false; Line=Dash;   Id="recovered" }
        { Metric=InHospitalToDate;     Color="#de9a5a"; Visible=false; Line=Dot;    Id="hospitalizedToDate" }
        { Metric=InHospital;           Color="#be7A2a"; Visible=true;  Line=Solid;  Id="hospitalized" }
        { Metric=InICU;                Color="#d99a91"; Visible=true;  Line=Solid;  Id="icu" }
        { Metric=OnVentilator;         Color="#bf5747"; Visible=false; Line=Solid;  Id="ventilator" }
        { Metric=OutOfHospital;        Color="#20b16d"; Visible=false; Line=Solid;  Id="discharged" }
        { Metric=OutOfHospitalToDate;  Color="#57c491"; Visible=false; Line=Dot;    Id="dischargedToDate" }
        { Metric=Deceased;             Color="#000000"; Visible=false; Line=Solid;  Id="deceased" }
        { Metric=DeceasedToDate;       Color="#666666"; Visible=true;  Line=Dot;    Id="deceasedToDate" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      Data : StatsData
      Metrics : Metrics
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        Data = data
        Metrics = Metrics.initial
        RangeSelectionButtonIndex = 0
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
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =
    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc =
        fun point ->
            match mc.Metric with
            | PerformedTests -> point.Tests.Performed.Today
            | PerformedTestsToDate -> point.Tests.Performed.ToDate
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
        for metric in state.Metrics do
            let pointData = metricDataGenerator metric
            yield pojo
                {|
                    visible = metric.Visible
                    color = metric.Color
                    name = I18N.tt "charts.metricsComparison" metric.Id
                    dashStyle = metric.Line |> DashStyle.toString
                    data =
                        state.Data
                        |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime12h, pointData dp))
                        |> Seq.skipWhile (fun (ts,value) ->
                            if metric.Visible && value.IsSome then
                                startTime <- min startTime ts
                            value.IsNone)
                        |> Seq.toArray
                |}
        yield addContainmentMeasuresFlags startTime None |> pojo
    ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions state.ScaleType "covid19-metrics-comparison"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = state.ScaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
        ]
    ]

let renderMetricSelector (metric : MetricCfg) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleMetricVisible metric.Metric |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
        prop.style style
        prop.text (I18N.tt "charts.metricsComparison" metric.Id) ]

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
        Utils.renderChartTopControlRight
            (Utils.renderScaleSelector
                state.ScaleType (ScaleTypeChanged >> dispatch))
        renderChartContainer state dispatch
        renderMetricsSelectors state.Metrics dispatch
    ]

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
