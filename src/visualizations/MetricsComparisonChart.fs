[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

open Data.Patients

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
    with
        static member UseStatsData metric =
            [PerformedTests; PerformedTestsToDate; ConfirmedCasesToday; ConfirmedCasesToDate; ActiveCases; RecoveredToDate]
            |> List.contains metric 

type MetricCfg = {
    Metric: Metric
    Color : string
    Visible : bool
    Type : MetricType
    Id: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric=ActiveCases;          Color="#dba51d"; Visible=true;  Type=Active; Id="activeCases" }
        { Metric=InHospital;           Color="#be7A2a"; Visible=true;  Type=Active; Id="hospitalized" }
        { Metric=InICU;                Color="#d96756"; Visible=true;  Type=Active; Id="icu" }
        { Metric=OnVentilator;         Color="#bf5747"; Visible=true;  Type=Active; Id="ventilator" }
        { Metric=PerformedTests;       Color="#19aebd"; Visible=false; Type=Today;  Id="testsPerformed" }
        { Metric=ConfirmedCasesToday;  Color="#bda506"; Visible=true;  Type=Today;  Id="confirmedCases" }
        { Metric=OutOfHospital;        Color="#20b16d"; Visible=false; Type=Today;  Id="hospitalDischarged" }
        { Metric=Deceased;             Color="#000000"; Visible=true;  Type=Today;  Id="deceased" }
        { Metric=PerformedTestsToDate; Color="#19aebd"; Visible=false; Type=ToDate; Id="testsPerformed" }
        { Metric=ConfirmedCasesToDate; Color="#bda506"; Visible=true;  Type=ToDate; Id="confirmedCases" }
        { Metric=OutOfHospitalToDate;  Color="#20b16d"; Visible=false; Type=ToDate; Id="hospitalDischarged" }
        { Metric=DeceasedToDate;       Color="#000000"; Visible=true;  Type=ToDate; Id="deceased" }
        { Metric=RecoveredToDate;      Color="#8cd4b2"; Visible=true;  Type=ToDate; Id="recovered" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)
        
type State =
    { ScaleType : ScaleType
      MetricType : MetricType
      Metrics : Metrics
      StatsData : StatsData
      PatientsData : PatientsStats []
      Error : string option
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of MetricType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either getOrFetch () ConsumePatientsData ConsumeServerError
    let state = {
        ScaleType = Linear
        MetricType = Active
        Metrics = Metrics.initial
        StatsData = data
        PatientsData = [||]
        Error = None
        RangeSelectionButtonIndex = 0
    }
    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data; }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
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

    let statsDataGenerator mc =
        fun point ->
            match mc.Metric with
            | PerformedTests -> point.Tests.Performed.Today
            | PerformedTestsToDate -> point.Tests.Performed.ToDate
            | ConfirmedCasesToday -> point.Cases.ConfirmedToday
            | ConfirmedCasesToDate -> point.Cases.ConfirmedToDate
            | ActiveCases -> point.Cases.Active
            | RecoveredToDate -> point.Cases.RecoveredToDate
            | _ -> None

    let patientsDataGenerator mc =
        fun point ->
            match mc.Metric with
            | InHospital -> point.total.inHospital.today
            | InHospitalToDate -> point.total.inHospital.toDate
            | InICU -> point.total.icu.today
            | OnVentilator -> point.total.critical.today
            | OutOfHospital -> point.total.inHospital.out
            | OutOfHospitalToDate -> point.total.outOfHospital.toDate
            | Deceased -> point.total.deceased.today |> Utils.zeroToNone
            | DeceasedToDate -> point.total.deceased.toDate
            | _ -> None

    let allSeries = [
        let mutable startTime = DateTime.Today |> jsTime
        for metric in state.Metrics do
            let statsData = statsDataGenerator metric
            let patientsData = patientsDataGenerator metric
            yield pojo
                {|
                    visible = metric.Type = state.MetricType && metric.Visible
                    color = metric.Color
                    name = I18N.tt "charts.metricsComparison" metric.Id
                    marker = if metric.Metric = Deceased then pojo {| enabled = true; symbol = "diamond" |} else pojo {| enabled = false |}
                    lineWidth = if metric.Metric = Deceased then 0 else 3
                    states = if metric.Metric = Deceased then pojo {| hover = {| lineWidthPlus = 0 |} |} else pojo {||}
                    dashStyle = 
                        match state.MetricType with
                        | Active -> "Solid" 
                        | Today -> "ShortDot"
                        | ToDate -> "Dot"
                    data =
                        if Metric.UseStatsData metric.Metric 
                        then
                            state.StatsData
                            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, statsData dp))
                            |> Seq.skipWhile (fun (ts,value) ->
                                if metric.Type = state.MetricType && metric.Visible && value.IsSome then
                                    startTime <- min startTime ts
                                value.IsNone)
                            |> Seq.toArray
                        else
                            state.PatientsData
                            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, patientsData dp))
                            |> Seq.skipWhile (fun (ts,value) ->
                                if metric.Type = state.MetricType && metric.Visible && value.IsSome then
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
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
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
