[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

type MetricType =
    | Active
    | Today
    | ToDate
  with
    static member getName = function
        | Active -> I18N.t "charts.metricsComparison.showActive"
        | Today -> I18N.t "charts.metricsComparison.showToday"
        | ToDate -> I18N.t "charts.metricsComparison.showToDate"

type Metric =
    | PerformedTests
    | PerformedTestsToDate
    | ConfirmedCasesToday
    | ConfirmedCasesToDate
    | ActiveCases
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
    Type : MetricType
    Id: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric=ActiveCases;          Color="#dba51d"; Visible=true;  Line=Solid;  Type=Active;    Id="activeCases" }
        { Metric=InHospital;           Color="#be7A2a"; Visible=true;  Line=Solid;  Type=Active;    Id="hospitalized" }
        { Metric=InICU;                Color="#d96756"; Visible=true;  Line=Solid;  Type=Active;    Id="icu" }
        { Metric=OnVentilator;         Color="#bf5747"; Visible=true;  Line=Solid;  Type=Active;    Id="ventilator" }
        { Metric=PerformedTests;       Color="#19aebd"; Visible=false; Line=Solid;  Type=Today;     Id="testsPerformed" }
        { Metric=ConfirmedCasesToday;  Color="#bda506"; Visible=true;  Line=Solid;  Type=Today;     Id="confirmedCases" }
        { Metric=OutOfHospital;        Color="#20b16d"; Visible=false; Line=Solid;  Type=Today;     Id="hospitalDischarged" }
        { Metric=Deceased;             Color="#000000"; Visible=true;  Line=Solid;  Type=Today;     Id="deceased" }
        { Metric=PerformedTestsToDate; Color="#19aebd"; Visible=false; Line=Dot;    Type=ToDate;    Id="testsPerformed" }
        { Metric=ConfirmedCasesToDate; Color="#bda506"; Visible=true; Line=Dot;     Type=ToDate;    Id="confirmedCases" }
        { Metric=OutOfHospitalToDate;  Color="#20b16d"; Visible=false; Line=Dot;    Type=ToDate;    Id="hospitalDischarged" }
        { Metric=DeceasedToDate;       Color="#000000"; Visible=true; Line=Dot;     Type=ToDate;    Id="deceased" }
        { Metric=RecoveredToDate;      Color="#8cd4b2"; Visible=true; Line=Dot;     Type=ToDate;    Id="recovered" }
    ]


    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)
        

type State =
    { ScaleType : ScaleType
      MetricType : MetricType
      Data : StatsData
      Metrics : Metrics
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of MetricType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        MetricType = Active
        Metrics = Metrics.initial
        Data = data
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleMetricVisible metric ->
        { state with
            Metrics = state.Metrics
                      |> Metrics.update (fun mc -> { mc with Visible = not mc.Visible}) metric
        }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | MetricTypeChanged metricType ->
        { state with 
            MetricType = metricType 
            }, Cmd.none
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
            | ActiveCases -> point.Cases.Active
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
            if metric.Type = state.MetricType  
            then
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
        prop.style [ style.height 480 ]
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
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (metric.Visible, "metric-selector--selected")]
        prop.style style
        prop.text (I18N.tt "charts.metricsComparison" metric.Id) ]

let renderMetricsSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for mc in state.Metrics do
                if mc.Type = state.MetricType 
                then yield renderMetricSelector mc dispatch
        ]
    ]

let renderMetricTypeSelectors (activeMetricType: MetricType) dispatch =
    let renderMetricTypeSelector (typeSelector: MetricType) =
        let active = typeSelector = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch typeSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (typeSelector |> MetricType.getName)
        ]

    let metricTypesSelectors =
        [ Active; Today; ToDate ]
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children ((Html.text (I18N.t "charts.common.view")) :: metricTypesSelectors)
    ]

let render state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderMetricTypeSelectors state.MetricType (MetricTypeChanged >> dispatch)
            Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
        renderMetricsSelectors state dispatch
    ]

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
