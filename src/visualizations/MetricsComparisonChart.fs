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

let chartText = I18N.chartText "metricsComparison"

type MetricType =
    | Active
    | Today
    | ToDate

type FullMetricType = {
    MetricType: MetricType
    IsAveraged: bool
}
  with
    member this.Name =
        match this.MetricType, this.IsAveraged with
        | Active, _ -> chartText "showActive"
        | Today, false -> chartText "showToday"
        | Today, true -> chartText "show7DaysAverage"
        | ToDate, _ -> chartText "showToDate"
    static member All =
        [ { MetricType = Active; IsAveraged = false }
          { MetricType = Today; IsAveraged = false }
          { MetricType = ToDate; IsAveraged = false }
          { MetricType = Today; IsAveraged = true } ]
    static member Default = { MetricType = Active; IsAveraged = false }

type Metric =
    | PerformedTestsToday
    | PerformedTestsToDate
    | ConfirmedCasesToday
    | ConfirmedCasesToDate
    | ActiveCases
    | RecoveredToDate
    | HospitalIn
    | HospitalOut
    | HospitalToday
    | HospitalToDate
    | HospitalOutToDate
    | ICUIn
    | ICUOut
    | ICUToday
    | ICUToDate
    | VentilatorIn
    | VentilatorOut
    | VentilatorToday
    | VentilatorToDate
    | DeceasedToday
    | DeceasedToDate
    | VacAdministeredToday
    | VacAdministeredToDate
    with
        static member UseStatsData metric =
            [PerformedTestsToday; PerformedTestsToDate; ConfirmedCasesToday
             ConfirmedCasesToDate; ActiveCases; RecoveredToDate 
             VacAdministeredToday; VacAdministeredToDate ]
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
        { Metric=ActiveCases;           Color="#dba51d"; Visible=true;  Type=Active; Id="activeCases" }
        { Metric=HospitalToday;         Color="#be7A2a"; Visible=true;  Type=Active; Id="hospitalized" }
        { Metric=ICUToday;              Color="#d96756"; Visible=true;  Type=Active; Id="icu" }
        { Metric=VentilatorToday;       Color="#bf5747"; Visible=true;  Type=Active; Id="ventilator" }
        { Metric=PerformedTestsToday;   Color="#19aebd"; Visible=false; Type=Today;  Id="testsPerformed" }
        { Metric=ConfirmedCasesToday;   Color="#bda506"; Visible=true;  Type=Today;  Id="confirmedCases" }
        { Metric=VacAdministeredToday;  Color="#189a73"; Visible=true;  Type=Today;  Id="vaccinationAdministered" }
        { Metric=HospitalIn;            Color="#be7A2a"; Visible=true;  Type=Today;  Id="hospitalAdmitted" }
        { Metric=HospitalOut;           Color="#8cd4b2"; Visible=false; Type=Today;  Id="hospitalDischarged" }
        { Metric=ICUIn;                 Color="#d96756"; Visible=true;  Type=Today;  Id="icuAdmitted" }
        { Metric=ICUOut;                Color="#ffb4a2"; Visible=false; Type=Today;  Id="icuDischarged" }
        { Metric=VentilatorIn;          Color="#bf5747"; Visible=true;  Type=Today;  Id="ventilatorAdmitted" }
        { Metric=VentilatorOut;         Color="#d99a91"; Visible=false; Type=Today;  Id="ventilatorDischarged" }
        { Metric=DeceasedToday;         Color="#6d5b80"; Visible=true;  Type=Today;  Id="deceased" }
        { Metric=PerformedTestsToDate;  Color="#19aebd"; Visible=false; Type=ToDate; Id="testsPerformed" }
        { Metric=ConfirmedCasesToDate;  Color="#bda506"; Visible=true;  Type=ToDate; Id="confirmedCases" }
        { Metric=RecoveredToDate;       Color="#20b16d"; Visible=true;  Type=ToDate; Id="recovered" }
        { Metric=VacAdministeredToDate; Color="#189a73"; Visible=true;  Type=ToDate; Id="vaccinationAdministered" }
        { Metric=HospitalToDate;        Color="#be7A2a"; Visible=true;  Type=ToDate; Id="hospitalAdmitted" }
        { Metric=HospitalOutToDate;     Color="#8cd4b2"; Visible=false; Type=ToDate; Id="hospitalDischarged" }
        { Metric=ICUToDate;             Color="#d96756"; Visible=false; Type=ToDate; Id="icuAdmitted" }
        { Metric=VentilatorToDate;      Color="#d96756"; Visible=false; Type=ToDate; Id="ventilatorAdmitted" }
        { Metric=DeceasedToDate;        Color="#6d5b80"; Visible=true;  Type=ToDate; Id="deceased" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      MetricType : FullMetricType
      Metrics : Metrics
      StatsData : StatsData
      PatientsData : PatientsStats []
      Error : string option
      RangeSelectionButtonIndex: int
      ShowAll : bool
    }

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ToggleMetricVisible of Metric
    | ToggleAllMetrics of bool
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of FullMetricType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either getOrFetch () ConsumePatientsData ConsumeServerError
    let state = {
        ScaleType = Linear
        MetricType = FullMetricType.Default
        Metrics = Metrics.initial
        StatsData = data
        PatientsData = [||]
        Error = None
        RangeSelectionButtonIndex = 0
        ShowAll = true
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
    | ToggleAllMetrics visibleOrHidden ->
        let newMetricsConfig =
            state.Metrics
            |> List.map (fun m ->
                { Metric = m.Metric
                  Color = m.Color
                  Type = m.Type
                  Id = m.Id
                  Visible = visibleOrHidden } )
        { state with
            Metrics = newMetricsConfig
            ShowAll = not state.ShowAll }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | MetricTypeChanged metricType ->
        { state with
            MetricType = metricType
            }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none


let statsDataGenerator metric =
    fun point ->
        match metric.Metric with
        | PerformedTestsToday -> point.Tests.Performed.Today
        | PerformedTestsToDate -> point.Tests.Performed.ToDate
        | ConfirmedCasesToday -> point.Cases.ConfirmedToday
        | ConfirmedCasesToDate -> point.Cases.ConfirmedToDate
        | ActiveCases -> point.Cases.Active
        | RecoveredToDate -> point.Cases.RecoveredToDate
        | VacAdministeredToday -> point.Vaccination.Administered.Today
        | VacAdministeredToDate -> point.Vaccination.Administered.ToDate
        | _ -> None

let patientsDataGenerator metric =
    fun point ->
        match metric.Metric with
        | HospitalToday -> point.total.inHospital.today
        | HospitalIn -> point.total.inHospital.``in``
        | HospitalOut -> point.total.inHospital.out
        | HospitalToDate -> point.total.inHospital.toDate
        | HospitalOutToDate -> point.total.outOfHospital.toDate
        | ICUToday -> point.total.icu.today
        | ICUIn -> point.total.icu.``in``
        | ICUOut -> point.total.icu.out
        | ICUToDate -> point.total.icu.toDate
        | VentilatorToday -> point.total.critical.today
        | VentilatorIn -> point.total.critical.``in``
        | VentilatorOut -> point.total.critical.out
        | VentilatorToDate -> point.total.critical.toDate
        | DeceasedToday -> point.total.deceased.today |> Utils.zeroToNone
        | DeceasedToDate -> point.total.deceased.toDate
        | _ -> None


let prepareMetricsData (metric: MetricCfg) (state: State) =

    let statsData = statsDataGenerator metric
    let patientsData = patientsDataGenerator metric

    let untrimmedData =
        if Metric.UseStatsData metric.Metric then
            state.StatsData
            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, statsData dp))
        else
            state.PatientsData
            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, patientsData dp))

    let isValueMissing ((_, value): (JsTimestamp * int option)) = value.IsNone

    let intOptionToFloat value =
        match value with
        | Some x -> float x
        | None -> 0.

    let trimmedData =
        untrimmedData
        |> Seq.toArray
        |> Array.skipWhile isValueMissing
        |> Array.rev
        |> Array.skipWhile isValueMissing
        |> Array.rev
        |> Array.map(fun (date, value) -> (date, value |> intOptionToFloat))

    let finalData =
        match state.MetricType.IsAveraged with
        | true -> trimmedData |> Statistics.calcRunningAverage
        | false -> trimmedData

    finalData


let renderChartOptions state dispatch =

    let allSeries = [
        let mutable startTime = DateTime.Today |> jsTime

        let visibleMetrics =
            state.Metrics
            |> Seq.filter (fun metric ->
                metric.Type = state.MetricType.MetricType
                && metric.Visible)

        for metric in visibleMetrics do
            let data = prepareMetricsData metric state

            if data |> Array.length > 0 then
                let metricStartTime = data.[0] |> fst
                if metricStartTime < startTime then
                    startTime <- metricStartTime

            yield pojo
                {|
                    visible = true
                    ``type`` = "line"
                    color = metric.Color
                    name = chartText metric.Id
                    marker =
                        if metric.Metric = DeceasedToday then
                            pojo {| enabled = true; symbol = "diamond" |}
                        else pojo {| enabled = false |}
                    lineWidth = if metric.Metric = DeceasedToday then 0 else 2
                    states =
                        if metric.Metric = DeceasedToday then
                            pojo {| hover = {| lineWidthPlus = 0 |} |}
                        else pojo {||}
                    dashStyle =
                        match state.MetricType.MetricType with
                        | Active -> "Solid"
                        | Today -> "ShortDot"
                        | ToDate -> "Dot"
                    data = data
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
            |> chartFromWindow
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
        prop.text (chartText metric.Id) ]

let renderMetricsSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            [ Html.div [
                prop.onClick (fun _ -> ToggleAllMetrics ( if state.ShowAll then false else true ) |> dispatch)
                prop.className "btn btn-sm metric-selector"
                prop.text ( if state.ShowAll then I18N.t "charts.common.hideAll" else I18N.t "charts.common.showAll" ) ] ]
            |> List.append ( state.Metrics |> List.map (fun metric -> if metric.Type = state.MetricType.MetricType then renderMetricSelector metric dispatch else Html.none) )
        )  ]

let renderMetricTypeSelectors (activeMetricType: FullMetricType) dispatch =
    let renderMetricTypeSelector (metricTypeToRender: FullMetricType) =
        let active = metricTypeToRender = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch metricTypeToRender)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (metricTypeToRender.Name)
        ]

    let metricTypesSelectors =
        FullMetricType.All
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]

let render state dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            Utils.renderChartTopControls [
                renderMetricTypeSelectors
                    state.MetricType (MetricTypeChanged >> dispatch)
                Utils.renderScaleSelector
                    state.ScaleType (ScaleTypeChanged >> dispatch)
            ]
            renderChartContainer state dispatch
            renderMetricsSelectors state dispatch
        ]

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
