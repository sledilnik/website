[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

type XAxisType =
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
    Id: string
}

type Metrics = MetricCfg list

module Metrics  =
    let metricsActive = [
        { Metric=ActiveCases;          Color="#dba51d"; Visible=true;  Line=Dash;   Id="activeCases" }
        { Metric=InHospital;           Color="#be7A2a"; Visible=true;  Line=Solid;  Id="hospitalized" }
        { Metric=InICU;                Color="#d96756"; Visible=true;  Line=Solid;  Id="icu" }
        { Metric=OnVentilator;         Color="#bf5747"; Visible=true;  Line=Solid;  Id="ventilator" }
    ]

    let metricsToday = [
        { Metric=PerformedTests;       Color="#19aebd"; Visible=false; Line=Solid;  Id="testsPerformed" }
        { Metric=ConfirmedCasesToday;  Color="#bda506"; Visible=true;  Line=Solid;  Id="confirmedCases" }
        { Metric=OutOfHospital;        Color="#20b16d"; Visible=false; Line=Solid;  Id="hospitalDischarged" }
        { Metric=Deceased;             Color="#000000"; Visible=false; Line=Solid;  Id="deceased" }
    ]

    let metricsToDate = [
        { Metric=PerformedTestsToDate; Color="#19aebd"; Visible=false; Line=Dot;    Id="testsPerformed" }
        { Metric=ConfirmedCasesToDate; Color="#bda506"; Visible=true;  Line=Dot;    Id="confirmedCases" }
        { Metric=OutOfHospitalToDate;  Color="#20b16d"; Visible=false; Line=Dot;    Id="hospitalDischarged" }
        { Metric=DeceasedToDate;       Color="#000000"; Visible=false; Line=Dot;    Id="deceased" }
        { Metric=RecoveredToDate;      Color="#8cd4b2"; Visible=false; Line=Dash;   Id="recovered" }
    ]

    let byType mType =
        match mType with
        | Active -> metricsActive
        | Today -> metricsToday
        | ToDate -> metricsToDate

    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      XAxisType : XAxisType
      Data : StatsData
      Metrics : Metrics
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType
    | XAxisTypeChanged of XAxisType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        XAxisType = Active
        Metrics = Metrics.byType(Active)
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
    | XAxisTypeChanged xAxisType ->
        { state with 
            XAxisType = xAxisType
            Metrics = Metrics.byType(xAxisType) }, Cmd.none
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
                yield renderMetricSelector mc dispatch
        ]
    ]

let renderXAxisSelectors (activeXAxisType: XAxisType) dispatch =
    let renderXAxisSelector (axisSelector: XAxisType) =
        let active = axisSelector = activeXAxisType
        Html.div [
            prop.onClick (fun _ -> dispatch axisSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (axisSelector |> XAxisType.getName)
        ]

    let xAxisTypesSelectors =
        [ Active; Today; ToDate ]
        |> List.map renderXAxisSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children ((Html.text (I18N.t "charts.common.xAxis")) :: xAxisTypesSelectors)
    ]

let render state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderXAxisSelectors state.XAxisType (XAxisTypeChanged >> dispatch)
            Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
        renderMetricsSelectors state dispatch
    ]

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
